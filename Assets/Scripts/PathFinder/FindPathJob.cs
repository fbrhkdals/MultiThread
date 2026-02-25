using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct FindPathJob : IJob
{
    public int startNodeIndex;
    public int endNodeIndex;
    public int mapWidth; // 인덱스 계산을 위해 GridManager에서 넘겨받음

    public NativeArray<PathNode> nodes;
    [ReadOnly] public NativeArray<int2> directions;
    public NativeList<int2> resultPath;

    public void Execute()
    {
        NativeList<int> openList = new NativeList<int>(Allocator.Temp);
        NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

        openList.Add(startNodeIndex);

        while (openList.Length > 0)
        {
            int currentNodeIndex = GetLowestFCostNodeIndex(openList);
            PathNode currentNode = nodes[currentNodeIndex];

            // 목적지 도착
            if (currentNodeIndex == endNodeIndex)
            {
                RetracePath(currentNodeIndex);
                break;
            }

            RemoveFromList(openList, currentNodeIndex);
            closedList.Add(currentNodeIndex);

            for (int i = 0; i < directions.Length; i++)
            {
                int2 neighborAxial = currentNode.axial + directions[i];
                int neighborIndex = GetNodeIndexByAxial(neighborAxial);

                // 맵 범위를 벗어난 경우
                if (neighborIndex == -1 || neighborIndex >= nodes.Length) continue;

                PathNode neighborNode = nodes[neighborIndex];

                // 이동 불가 또는 이미 점유된 경우 또는 이미 탐색한 경우 패스
                if (!neighborNode.walkable || neighborNode.isOccupied || Contains(closedList, neighborIndex))
                    continue;

                // 모든 타일 비용이 동일하므로 gCost는 단순히 +1
                int newGCost = currentNode.gCost + 1;

                if (newGCost < neighborNode.gCost || !Contains(openList, neighborIndex))
                {
                    neighborNode.gCost = newGCost;
                    neighborNode.hCost = GetDistance(neighborNode.axial, nodes[endNodeIndex].axial);
                    neighborNode.parentIndex = currentNodeIndex;
                    nodes[neighborIndex] = neighborNode; // 구조체 데이터 업데이트

                    if (!Contains(openList, neighborIndex))
                        openList.Add(neighborIndex);
                }
            }
        }

        openList.Dispose();
        closedList.Dispose();
    }

    // 인덱스 계산 최적화 (루프 없음)
    private int GetNodeIndexByAxial(int2 axial)
    {
        int q = axial.x;
        int r = axial.y;
        int col = q + (r / 2); // Axial to Array Column

        if (col < 0 || r < 0 || col >= mapWidth) return -1;

        return r * mapWidth + col;
    }

    private int GetLowestFCostNodeIndex(NativeList<int> list)
    {
        int lowest = list[0];
        for (int i = 1; i < list.Length; i++)
        {
            if (nodes[list[i]].fCost < nodes[lowest].fCost) lowest = list[i];
            // F값이 같다면 목적지에 더 가까운(H값이 낮은) 것을 선택하여 경로를 직선화
            else if (nodes[list[i]].fCost == nodes[lowest].fCost && nodes[list[i]].hCost < nodes[lowest].hCost)
                lowest = list[i];
        }
        return lowest;
    }

    private int GetDistance(int2 a, int2 b)
    {
        // 육각형 Axial 거리 공식
        return (math.abs(a.x - b.x) + math.abs(a.x + a.y - b.x - b.y) + math.abs(a.y - b.y)) / 2;
    }

    private void RetracePath(int endIdx)
    {
        int curr = endIdx;
        while (curr != startNodeIndex && curr != -1)
        {
            resultPath.Add(nodes[curr].axial);
            curr = nodes[curr].parentIndex;
        }
    }

    private bool Contains(NativeList<int> list, int val)
    {
        for (int i = 0; i < list.Length; i++) if (list[i] == val) return true;
        return false;
    }

    private void RemoveFromList(NativeList<int> list, int val)
    {
        for (int i = 0; i < list.Length; i++)
        {
            if (list[i] == val) { list.RemoveAtSwapBack(i); return; }
        }
    }
}