using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public class PathfindingManager : MonoBehaviour
{
    private NativeArray<PathNode> baseMeshNodeArray;
    private NativeArray<PathNode> nodesForJob;
    private NativeArray<int2> dirArray;
    private int mapWidth, mapHeight;
    private bool isInitialized;
    private HexGrid gridRef;

    public void Initialize(int width, int height, HexGrid grid)
    {
        mapWidth = width;
        mapHeight = height;
        gridRef = grid;

        baseMeshNodeArray = new NativeArray<PathNode>(width * height, Allocator.Persistent);
        nodesForJob = new NativeArray<PathNode>(width * height, Allocator.Persistent);
        dirArray = new NativeArray<int2>(6, Allocator.Persistent);

        for (int r = 0; r < mapHeight; r++)
        {
            for (int q = 0; q < mapWidth; q++)
            {
                Vector2Int axial = new Vector2Int(q - (r / 2), r);
                Node node = gridRef.GetNode(axial);
                int index = r * mapWidth + q;
                baseMeshNodeArray[index] = new PathNode
                {
                    axial = new int2(axial.x, axial.y),
                    walkable = node.walkable,
                    isOccupied = false,
                    gCost = 1000000,
                    parentIndex = -1
                };
            }
        }

        for (int i = 0; i < 6; i++)
        {
            Vector2Int d = HexUtils.Directions[i];
            dirArray[i] = new int2(d.x, d.y);
        }
        isInitialized = true;
    }

    public void RequestPathAsync(Vector2Int start, Vector2Int end, Character requester, Character blockTarget, Action<List<Vector2Int>> onComplete)
    {
        if (!isInitialized) return;
        StartCoroutine(PathfindingRoutine(start, end, requester, blockTarget, onComplete));
    }

    private IEnumerator PathfindingRoutine(Vector2Int start, Vector2Int end, Character requester, Character blockTarget, Action<List<Vector2Int>> onComplete)
    {
        NativeArray<PathNode>.Copy(baseMeshNodeArray, nodesForJob);

        // 점유 데이터 업데이트 (이 로직은 나중에 Job 내부로 옮기면 더 빠릅니다)
        for (int i = 0; i < nodesForJob.Length; i++)
        {
            PathNode pNode = nodesForJob[i];
            Vector2Int axial = new Vector2Int(pNode.axial.x, pNode.axial.y);
            Node node = gridRef.GetNode(axial);

            if (node != null && node.IsOccupied)
            {
                Character occupant = node.OccupiedCharacter;
                if (occupant == requester) pNode.isOccupied = false;
                else if (blockTarget != null && occupant == blockTarget) pNode.isOccupied = true;
                else if (occupant != null && occupant.Mover.IsMoving) pNode.isOccupied = false;
                else pNode.isOccupied = true;
                nodesForJob[i] = pNode;
            }
        }

        NativeList<int2> pathResult = new NativeList<int2>(512, Allocator.TempJob);
        FindPathJob job = new FindPathJob
        {
            startNodeIndex = GetIndex(start),
            endNodeIndex = GetIndex(end),
            mapWidth = mapWidth,
            nodes = nodesForJob,
            directions = dirArray,
            resultPath = pathResult
        };

        JobHandle handle = job.Schedule();
        while (!handle.IsCompleted) yield return null;
        handle.Complete();

        List<Vector2Int> finalPath = new List<Vector2Int>();
        for (int i = pathResult.Length - 1; i >= 0; i--)
            finalPath.Add(new Vector2Int(pathResult[i].x, pathResult[i].y));

        onComplete?.Invoke(finalPath);
        pathResult.Dispose();
    }

    private int GetIndex(Vector2Int axial)
    {
        int r = axial.y;
        int q = axial.x + (r / 2);
        return Mathf.Clamp(r * mapWidth + q, 0, (mapWidth * mapHeight) - 1);
    }

    private void OnDestroy()
    {
        if (baseMeshNodeArray.IsCreated) baseMeshNodeArray.Dispose();
        if (nodesForJob.IsCreated) nodesForJob.Dispose();
        if (dirArray.IsCreated) dirArray.Dispose();
    }
}