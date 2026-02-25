using Unity.Mathematics;

public struct PathNode
{
    public int2 axial;
    public bool walkable;
    public bool isOccupied;

    public int gCost;
    public int hCost;
    public int fCost => gCost + hCost;

    public int parentIndex; // 역추적을 위해 좌표 대신 배열 인덱스를 저장합니다.
    public int index;       // 자기 자신의 인덱스
}