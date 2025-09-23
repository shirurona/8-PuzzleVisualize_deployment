using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Fruchterman-Reingoldアルゴリズムを使用したグラフ可視化
/// Barnes-Hut法による最適化を含む
/// </summary>
public class FruchtermanReingoldVisualizer : MonoBehaviour, IVisualizeStrategy
{
    [Header("Fruchterman-Reingold Parameters")]
    [SerializeField] private int maxIterations = 100;
    [SerializeField] private float k = 10f; // 理想的な距離
    [SerializeField] private float initialTemperature = 100f;
    [SerializeField] private float coolingRate = 0.95f;
    [SerializeField] private float minTemperature = 0.1f;
    
    [Header("Barnes-Hut Optimization")]
    [SerializeField] private bool useBarnesHut = true;
    [SerializeField] private float theta = 0.5f; // 精度パラメータ
    
    [Header("Layout Parameters")]
    [SerializeField] private float initialAreaSize = 50f;
    [SerializeField] private bool centerInitialPuzzle = true;

    /// <summary>
    /// Barnes-Hut法用の3D OctTree実装
    /// </summary>
    private class OctTree
    {
        public struct Bounds
        {
            public float minX, maxX, minY, maxY, minZ, maxZ;
            public float width => maxX - minX;
            public float height => maxY - minY;
            public float depth => maxZ - minZ;
            public Vector3 center => new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, (minZ + maxZ) * 0.5f);
            public float maxDimension => Mathf.Max(width, height, depth);

            public Bounds(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
            {
                this.minX = minX;
                this.maxX = maxX;
                this.minY = minY;
                this.maxY = maxY;
                this.minZ = minZ;
                this.maxZ = maxZ;
            }

            public bool Contains(Vector3 point)
            {
                return point.x >= minX && point.x <= maxX && 
                       point.y >= minY && point.y <= maxY && 
                       point.z >= minZ && point.z <= maxZ;
            }
        }

        public struct Node
        {
            public PuzzleState puzzle;
            public Vector3 position;
            public float mass;

            public Node(PuzzleState puzzle, Vector3 position, float mass = 1f)
            {
                this.puzzle = puzzle;
                this.position = position;
                this.mass = mass;
            }
        }

        public Bounds bounds;
        public OctTree[] children;
        public Node? node;
        public Vector3 centerOfMass;
        public float totalMass;
        public bool isLeaf => children == null;

        public OctTree(Bounds bounds)
        {
            this.bounds = bounds;
            this.children = null;
            this.node = null;
            this.centerOfMass = Vector3.zero;
            this.totalMass = 0f;
        }

        public bool Insert(Node newNode)
        {
            if (!bounds.Contains(newNode.position))
                return false;

            // 空のリーフノードの場合
            if (totalMass == 0)
            {
                node = newNode;
                centerOfMass = newNode.position;
                totalMass = newNode.mass;
                return true;
            }

            // 内部ノードの場合
            if (!isLeaf)
            {
                // 子ノードに挿入を試行
                for (int i = 0; i < 8; i++)
                {
                    if (children[i].Insert(newNode))
                    {
                        UpdateCenterOfMass();
                        return true;
                    }
                }
                return false;
            }

            // 既存ノードがあるリーフノードの場合 - 分割
            Subdivide();
            
            // 既存ノードを子ノードに再配置
            Node existingNode = node.Value;
            node = null;
            
            // 既存ノードと新ノードの両方を子ノードに挿入
            bool existingInserted = false;
            bool newInserted = false;
            
            for (int i = 0; i < 8; i++)
            {
                if (!existingInserted && children[i].Insert(existingNode))
                    existingInserted = true;
                if (!newInserted && children[i].Insert(newNode))
                    newInserted = true;
            }
            
            UpdateCenterOfMass();
            return existingInserted && newInserted;
        }

        private void Subdivide()
        {
            Vector3 center = bounds.center;
            children = new OctTree[8];
            
            // 8つの子ノードを作成（3D空間の8つの象限）
            children[0] = new OctTree(new Bounds(center.x, bounds.maxX, center.y, bounds.maxY, center.z, bounds.maxZ)); // +X+Y+Z
            children[1] = new OctTree(new Bounds(bounds.minX, center.x, center.y, bounds.maxY, center.z, bounds.maxZ)); // -X+Y+Z
            children[2] = new OctTree(new Bounds(bounds.minX, center.x, bounds.minY, center.y, center.z, bounds.maxZ)); // -X-Y+Z
            children[3] = new OctTree(new Bounds(center.x, bounds.maxX, bounds.minY, center.y, center.z, bounds.maxZ)); // +X-Y+Z
            children[4] = new OctTree(new Bounds(center.x, bounds.maxX, center.y, bounds.maxY, bounds.minZ, center.z)); // +X+Y-Z
            children[5] = new OctTree(new Bounds(bounds.minX, center.x, center.y, bounds.maxY, bounds.minZ, center.z)); // -X+Y-Z
            children[6] = new OctTree(new Bounds(bounds.minX, center.x, bounds.minY, center.y, bounds.minZ, center.z)); // -X-Y-Z
            children[7] = new OctTree(new Bounds(center.x, bounds.maxX, bounds.minY, center.y, bounds.minZ, center.z)); // +X-Y-Z
        }

        private void UpdateCenterOfMass()
        {
            if (isLeaf)
            {
                if (node.HasValue)
                {
                    centerOfMass = node.Value.position;
                    totalMass = node.Value.mass;
                }
                else
                {
                    centerOfMass = Vector3.zero;
                    totalMass = 0f;
                }
            }
            else
            {
                Vector3 weightedSum = Vector3.zero;
                totalMass = 0f;
                
                for (int i = 0; i < 8; i++)
                {
                    if (children[i].totalMass > 0)
                    {
                        weightedSum += children[i].centerOfMass * children[i].totalMass;
                        totalMass += children[i].totalMass;
                    }
                }
                
                centerOfMass = totalMass > 0 ? weightedSum / totalMass : Vector3.zero;
            }
        }

        public Vector3 CalculateForce(Vector3 targetPosition, float k, float theta)
        {
            if (totalMass == 0)
                return Vector3.zero;

            Vector3 direction = centerOfMass - targetPosition;
            float distance = direction.magnitude;
            
            if (distance < 0.01f)
                return Vector3.zero;

            // リーフノードまたは十分に遠い場合は近似計算
            if (isLeaf || bounds.maxDimension / distance < theta)
            {
                float force = k * k * totalMass / (distance * distance);
                return -direction.normalized * force;
            }

            // 子ノードの力を合計
            Vector3 totalForce = Vector3.zero;
            for (int i = 0; i < 8; i++)
            {
                totalForce += children[i].CalculateForce(targetPosition, k, theta);
            }
            
            return totalForce;
        }
    }

    public Dictionary<PuzzleState, Vector3> VisualizeSearchSpace(Dictionary<PuzzleState, PuzzleNodeData> searchDataMap, PuzzleState initialPuzzleState)
    {
        var puzzleViewMap = new Dictionary<PuzzleState, Vector3>();
        var nodes = searchDataMap.Keys.ToList();
        
        if (nodes.Count == 0)
            return puzzleViewMap;

        // 初期位置の設定
        InitializePositions(nodes, puzzleViewMap, initialPuzzleState);

        // Fruchterman-Reingoldアルゴリズムの実行
        ExecuteFruchtermanReingold(nodes, puzzleViewMap, searchDataMap);

        return puzzleViewMap;
    }

    private void InitializePositions(List<PuzzleState> nodes, Dictionary<PuzzleState, Vector3> puzzleViewMap, PuzzleState initialPuzzleState)
    {
        // 初期パズルを中央に配置
        if (centerInitialPuzzle && nodes.Contains(initialPuzzleState))
        {
            puzzleViewMap[initialPuzzleState] = Vector3.zero;
        }

        // 他のノードを3D空間にランダムに配置
        foreach (var node in nodes)
        {
            if (!puzzleViewMap.ContainsKey(node))
            {
                float x = Random.Range(-initialAreaSize * 0.5f, initialAreaSize * 0.5f);
                float y = Random.Range(-initialAreaSize * 0.5f, initialAreaSize * 0.5f);
                float z = Random.Range(-initialAreaSize * 0.5f, initialAreaSize * 0.5f);
                puzzleViewMap[node] = new Vector3(x, y, z);
            }
        }
    }

    private void ExecuteFruchtermanReingold(List<PuzzleState> nodes, Dictionary<PuzzleState, Vector3> puzzleViewMap, Dictionary<PuzzleState, PuzzleNodeData> searchDataMap)
    {
        float temperature = initialTemperature;
        
        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            var forces = new Dictionary<PuzzleState, Vector3>();
            
            // 力を初期化
            foreach (var node in nodes)
            {
                forces[node] = Vector3.zero;
            }

            // 斥力計算
            if (useBarnesHut)
            {
                CalculateRepulsiveForcesBarnesHut(nodes, puzzleViewMap, forces);
            }
            else
            {
                CalculateRepulsiveForcesNaive(nodes, puzzleViewMap, forces);
            }

            // 引力計算
            CalculateAttractiveForcesFromSearchData(nodes, puzzleViewMap, forces, searchDataMap);

            // 位置更新
            UpdatePositions(nodes, puzzleViewMap, forces, temperature);

            // 温度更新
            temperature *= coolingRate;
            if (temperature < minTemperature)
                break;
        }
    }

    private void CalculateRepulsiveForcesBarnesHut(List<PuzzleState> nodes, Dictionary<PuzzleState, Vector3> puzzleViewMap, Dictionary<PuzzleState, Vector3> forces)
    {
        // 3D境界の計算
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;
        
        foreach (var node in nodes)
        {
            var pos = puzzleViewMap[node];
            minX = Mathf.Min(minX, pos.x);
            maxX = Mathf.Max(maxX, pos.x);
            minY = Mathf.Min(minY, pos.y);
            maxY = Mathf.Max(maxY, pos.y);
            minZ = Mathf.Min(minZ, pos.z);
            maxZ = Mathf.Max(maxZ, pos.z);
        }

        // 境界を少し拡張
        float margin = Mathf.Max(maxX - minX, maxY - minY, maxZ - minZ) * 0.1f;
        var bounds = new OctTree.Bounds(minX - margin, maxX + margin, 
                                        minY - margin, maxY + margin,
                                        minZ - margin, maxZ + margin);

        // OctTreeの構築
        var octTree = new OctTree(bounds);
        foreach (var node in nodes)
        {
            var pos = puzzleViewMap[node];
            var octNode = new OctTree.Node(node, pos);
            octTree.Insert(octNode);
        }

        // 各ノードの斥力を計算
        foreach (var node in nodes)
        {
            var pos = puzzleViewMap[node];
            Vector3 force3D = octTree.CalculateForce(pos, k, theta);
            forces[node] += force3D;
        }
    }

    private void CalculateRepulsiveForcesNaive(List<PuzzleState> nodes, Dictionary<PuzzleState, Vector3> puzzleViewMap, Dictionary<PuzzleState, Vector3> forces)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = i + 1; j < nodes.Count; j++)
            {
                var nodeA = nodes[i];
                var nodeB = nodes[j];
                
                Vector3 delta = puzzleViewMap[nodeA] - puzzleViewMap[nodeB];
                float distance = Mathf.Max(0.01f, delta.magnitude);
                
                Vector3 force = delta.normalized * (k * k / distance);
                forces[nodeA] += force;
                forces[nodeB] -= force;
            }
        }
    }

    private void CalculateAttractiveForcesFromSearchData(List<PuzzleState> nodes, Dictionary<PuzzleState, Vector3> puzzleViewMap, Dictionary<PuzzleState, Vector3> forces, Dictionary<PuzzleState, PuzzleNodeData> searchDataMap)
    {
        foreach (var node in nodes)
        {
            if (!searchDataMap.TryGetValue(node, out PuzzleNodeData nodeData))
                continue;

            if (nodeData.AdjacentStates == null)
                continue;

            foreach (var adjacent in nodeData.AdjacentStates)
            {
                if (!puzzleViewMap.ContainsKey(adjacent))
                    continue;

                Vector3 delta = puzzleViewMap[node] - puzzleViewMap[adjacent];
                float distance = Mathf.Max(0.01f, delta.magnitude);
                
                Vector3 force = delta.normalized * (distance * distance / k);
                forces[node] -= force * 0.5f; // 0.5倍して重複を避ける
                forces[adjacent] += force * 0.5f;
            }
        }
    }

    private void UpdatePositions(List<PuzzleState> nodes, Dictionary<PuzzleState, Vector3> puzzleViewMap, Dictionary<PuzzleState, Vector3> forces, float temperature)
    {
        foreach (var node in nodes)
        {
            Vector3 displacement = forces[node];
            float displacementMagnitude = displacement.magnitude;
            
            if (displacementMagnitude > 0)
            {
                displacement = displacement.normalized * Mathf.Min(displacementMagnitude, temperature);
                puzzleViewMap[node] += displacement;
            }
        }
    }
}