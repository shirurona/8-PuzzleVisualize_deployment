using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class PuzzleVisualizer : MonoBehaviour
{
    [SerializeField] private TMP_Text numberTextPrefab;
    [SerializeField] private InstancedMeshInfo puzzleBlockInfo;
    [SerializeField] private InstancedMeshInfo edgeInfo;
    [SerializeField] private InstancedMeshInfo vertexInfo;
    [SerializeField] private InstancedMeshInfo edgeRouteInfo;
    
    [SerializeField] private Toggle vertexToggle;
    [SerializeField] private float radiusStep = 20f;
    [SerializeField] private float blockSpacing = 2.1f;

    private InstancedMeshRenderer[] puzzleBlockInstancedRenderer = new InstancedMeshRenderer[PuzzleState.TotalCells];
    private InstancedMeshRenderer[] numberTextInstancedRenderer = new InstancedMeshRenderer[PuzzleState.TotalCells];
    private InstancedMeshRenderer vertexInstancedRenderer;
    private InstancedMeshRenderer edgeBlockInstancedRenderer;
    private InstancedMeshRenderer edgeRouteBlockInstancedRenderer;
    
    public void ClearEdgeMatrices()
    {
        edgeBlockInstancedRenderer.ClearMatrices();
        edgeRouteBlockInstancedRenderer.ClearMatrices();
    }

    public void Initialize()
    {
        for (int i = 0; i < puzzleBlockInstancedRenderer.Length; i++)
        {
            puzzleBlockInstancedRenderer[i] = new InstancedMeshRenderer(puzzleBlockInfo);
        }
        for (int i = 0; i < numberTextInstancedRenderer.Length; i++)
        {
            var tmp = Instantiate(numberTextPrefab);
            tmp.text = i.ToString();
            tmp.ForceMeshUpdate();
            var tmpMesh = tmp.textInfo.meshInfo[0].mesh;
            var numberTextMaterial = tmp.fontMaterial;
            
            Mesh numberTextMesh = new Mesh();
            
            numberTextMesh.vertices = tmpMesh.vertices;
            numberTextMesh.triangles = tmpMesh.triangles;
            numberTextMesh.uv = tmpMesh.uv;
            numberTextMesh.normals = tmpMesh.normals;
            numberTextMesh.colors32 = tmpMesh.colors32;
            numberTextMesh.tangents = tmpMesh.tangents;
            
            DestroyImmediate(tmp.gameObject);
            InstancedMeshInfo numberTextMeshInfo = new InstancedMeshInfo
            {
                mesh = numberTextMesh,
                mat = numberTextMaterial
            };
            numberTextInstancedRenderer[i] = new InstancedMeshRenderer(numberTextMeshInfo);
        }
        vertexInstancedRenderer = new InstancedMeshRenderer(vertexInfo);
        edgeBlockInstancedRenderer = new InstancedMeshRenderer(edgeInfo);
        edgeRouteBlockInstancedRenderer = new InstancedMeshRenderer(edgeRouteInfo);
    }

    private void Update()
    {
        if (!vertexToggle.isOn)
        {
            for (int i = 0; i < puzzleBlockInstancedRenderer.Length; i++)
            {
                puzzleBlockInstancedRenderer[i].Render();
            }
            for (int i = 1; i < numberTextInstancedRenderer.Length; i++)
            {
                numberTextInstancedRenderer[i].Render();
            }
        }
        else
        {
            vertexInstancedRenderer.Render();
        }
        edgeBlockInstancedRenderer.Render();
        edgeRouteBlockInstancedRenderer.Render();
    }

    public void AddAdjacencyEdges(Dictionary<PuzzleState, PuzzleNodeData> searchDataMap, Dictionary<PuzzleState, Vector3> puzzleViewMap, HashSet<PuzzleState> routes)
    {
        foreach (var entry in searchDataMap)
        {
            PuzzleState parentState = entry.Key;
            PuzzleNodeData parentNodeData = entry.Value;

            if (!puzzleViewMap.TryGetValue(parentState, out Vector3 parentPuzzlePos))
            {
                continue;
            }

            if (parentNodeData.AdjacentStates == null) continue;
            
            foreach (PuzzleState childState in parentNodeData.AdjacentStates)
            {
                if (!puzzleViewMap.TryGetValue(childState, out Vector3 childPuzzlePos))
                {
                    continue;
                }
                Matrix4x4 matrix = AddRenderEdge(parentPuzzlePos, childPuzzlePos);
                InstancedMeshRenderer edgeRenderer = !routes.Contains(childState)
                    ? edgeBlockInstancedRenderer
                    : edgeRouteBlockInstancedRenderer;
                edgeRenderer.AddMatrix(matrix);
            }
        }
    }

    public void AddPuzzleInstances(Dictionary<PuzzleState, Vector3> puzzleViewMap)
    {
        vertexInstancedRenderer.AddMatrices(puzzleViewMap.Values.Select(x => Matrix4x4.TRS(x, Quaternion.identity, Vector3.one)));
        foreach (var puzzleVisualize in puzzleViewMap)
        {
            for (int row = 0; row < PuzzleState.RowCount; row++)
            {
                for (int col = 0; col < PuzzleState.ColumnCount; col++)
                {
                    Vector3 position = new Vector3(
                        (col - 1) * blockSpacing,
                        (1 - row) * blockSpacing,
                        0
                    );
                    puzzleBlockInstancedRenderer[puzzleVisualize.Key[new BlockPosition(row, col)]].AddMatrix(Matrix4x4.TRS(puzzleVisualize.Value + position, Quaternion.identity, new Vector3(2, 2, 0.5f)));
                    numberTextInstancedRenderer[puzzleVisualize.Key[new BlockPosition(row, col)]].AddMatrix(Matrix4x4.TRS(puzzleVisualize.Value + position + Vector3.back * 0.26f, Quaternion.identity, Vector3.one));
                }
            }
        }
    }

    public void ApplyAllMatrixData()
    {
        for (int i = 0; i < numberTextInstancedRenderer.Length; i++)
        {
            numberTextInstancedRenderer[i].ApplyMatrixData();
        }
        for (int i = 0; i < puzzleBlockInstancedRenderer.Length; i++)
        {
            puzzleBlockInstancedRenderer[i].ApplyMatrixData();
        }
        vertexInstancedRenderer.ApplyMatrixData();
        ApplyEdgeMatrixData();
    }

    public void ApplyEdgeMatrixData()
    {
        edgeBlockInstancedRenderer.ApplyMatrixData();
        edgeRouteBlockInstancedRenderer.ApplyMatrixData();
    }

    private Matrix4x4 AddRenderEdge(Vector3 parentPuzzlePos, Vector3 childPuzzlePos)
    {
        Vector3 position = (parentPuzzlePos + childPuzzlePos) / 2f;
        Vector3 direction = childPuzzlePos - parentPuzzlePos;
        Quaternion rotation = Quaternion.identity;

        if (direction != Vector3.zero)
        {
            rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90, 0, 0);
        }

        float length = direction.magnitude;
        Vector3 scale = new Vector3(0.1f, length / 2f, 0.1f);

        return Matrix4x4.TRS(position, rotation, scale);
    }

    void OnDestroy()
    {
        DisposeInstances();
    }

    void DisposeInstances()
    {
        for (int i = 0; i < puzzleBlockInstancedRenderer.Length; i++)
        {
            puzzleBlockInstancedRenderer[i].Dispose();
        }
        for (int i = 0; i < numberTextInstancedRenderer.Length; i++)
        {
            numberTextInstancedRenderer[i].Dispose();
        }
        vertexInstancedRenderer.Dispose();
        edgeBlockInstancedRenderer.Dispose();
        edgeRouteBlockInstancedRenderer.Dispose();
    }
} 