using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SearchSpaceVisualizer : MonoBehaviour, IVisualizeStrategy
{
    [SerializeField] private float radiusStep = 20f;
    [SerializeField] private float blockSpacing = 2.1f;

    private float NormalizeAngleRad(float angle)
    {
        while (angle <= -Mathf.PI) angle += 2 * Mathf.PI;
        while (angle > Mathf.PI) angle -= 2 * Mathf.PI;
        return angle;
    }

    public Dictionary<PuzzleState, Vector3> VisualizeSearchSpace(Dictionary<PuzzleState, PuzzleNodeData> searchDataMap, PuzzleState initialPuzzleState)
    {
        Dictionary<PuzzleState, Vector3> puzzleViewMap = new Dictionary<PuzzleState, Vector3>();

        if (searchDataMap.ContainsKey(initialPuzzleState))
        {
            Vector3 initPuzzlePos = Vector3.zero;
            puzzleViewMap[initialPuzzleState] = initPuzzlePos;
        }

        Queue<PuzzleState> layoutQueue = new Queue<PuzzleState>();
        if (puzzleViewMap.ContainsKey(initialPuzzleState))
        {
            layoutQueue.Enqueue(initialPuzzleState);
        }

        while (layoutQueue.Any())
        {
            PuzzleState currentPuzzle = layoutQueue.Dequeue();
            
            if (!searchDataMap.TryGetValue(currentPuzzle, out PuzzleNodeData currentNodeData))
            {
                continue;
            }
            if (!puzzleViewMap.TryGetValue(currentPuzzle, out Vector3 currentPosition))
            {
                continue;
            }

            int currentPuzzleDepth = currentNodeData.Depth;

            List<PuzzleState> childrenToLayout = new List<PuzzleState>();
            List<PuzzleState> potentialChildren = currentNodeData.AdjacentStates;

            if (potentialChildren != null)
            {
                foreach (var pChild in potentialChildren)
                {
                    if (searchDataMap.TryGetValue(pChild, out PuzzleNodeData childNodeData) &&
                        childNodeData.Parent == currentPuzzle &&
                        childNodeData.Depth == currentPuzzleDepth + 1)
                    {
                        if (!puzzleViewMap.ContainsKey(pChild))
                        {
                            childrenToLayout.Add(pChild);
                        }
                    }
                }
            }

            if (!childrenToLayout.Any()) continue;

            float referenceAngleRad = 0f;
            bool isInitial = (currentPuzzle == initialPuzzleState);

            if (!isInitial)
            {
                PuzzleState? parentOfCurrent = currentNodeData.Parent;
                if (parentOfCurrent != null && puzzleViewMap.ContainsKey(parentOfCurrent.Value))
                {
                    Vector3 dirToParent = (puzzleViewMap[parentOfCurrent.Value] - currentPosition).normalized;
                    referenceAngleRad = Mathf.Atan2(dirToParent.y, dirToParent.x);
                }
                else
                {
                    referenceAngleRad = (currentPosition != Vector3.zero) ?
                        Mathf.Atan2(-currentPosition.normalized.y, -currentPosition.normalized.x) :
                        Mathf.PI;
                }
            }

            int numChildBranches = childrenToLayout.Count;
            int totalBranches = numChildBranches + (!isInitial ? 1 : 0);

            float angleStepRad = (totalBranches > 0) ? (2 * Mathf.PI) / totalBranches : 0f;

            float currentBranchAngleRad = referenceAngleRad;
            if (!isInitial)
            {
                currentBranchAngleRad += angleStepRad;
            }

            for (int i = 0; i < numChildBranches; i++)
            {
                PuzzleState childPuzzle = childrenToLayout[i];
                float finalAngleRad = NormalizeAngleRad(currentBranchAngleRad);

                Vector3 childPosition = currentPosition + new Vector3(
                    radiusStep * Mathf.Cos(finalAngleRad),
                    radiusStep * Mathf.Sin(finalAngleRad),
                    0);

                if (!puzzleViewMap.ContainsKey(childPuzzle))
                {
                    puzzleViewMap[childPuzzle] = childPosition;
                    layoutQueue.Enqueue(childPuzzle);
                }
                currentBranchAngleRad += angleStepRad;
            }
        }
        return puzzleViewMap;
    }
} 