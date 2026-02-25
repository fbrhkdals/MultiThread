using UnityEngine;

public static class HexUtils
{
    public static readonly Vector2Int[] Directions = {
        new Vector2Int(1, 0),   // 우측
        new Vector2Int(1, -1),  // 우상단
        new Vector2Int(0, -1),  // 좌상단
        new Vector2Int(-1, 0),  // 좌측
        new Vector2Int(-1, 1),  // 좌하단
        new Vector2Int(0, 1)    // 우하단
    };
    public static Vector3 AxialToWorld(Vector2Int axial, float hexSize)
    {
        float x = hexSize * Mathf.Sqrt(3f) * (axial.x + axial.y / 2f);
        float z = hexSize * 1.5f * axial.y;
        return new Vector3(x, 0f, z);
    }

    public static Vector2Int WorldToAxial(Vector3 worldPos, float hexSize)
    {
        // 1. 월드 좌표를 부동 소수점 좌표(Fractional Hex)로 변환
        float q = (Mathf.Sqrt(3f) / 3f * worldPos.x - 1f / 3f * worldPos.z) / hexSize;
        float r = (2f / 3f * worldPos.z) / hexSize;

        // 2. 육각형 반올림 (Hex Rounding) 로직
        return AxialRound(q, r);
    }

    private static Vector2Int AxialRound(float q, float r)
    {
        float s = -q - r;
        int rq = Mathf.RoundToInt(q);
        int rr = Mathf.RoundToInt(r);
        int rs = Mathf.RoundToInt(s);

        float qDiff = Mathf.Abs(rq - q);
        float rDiff = Mathf.Abs(rr - r);
        float sDiff = Mathf.Abs(rs - s);

        if (qDiff > rDiff && qDiff > sDiff) rq = -rr - rs;
        else if (rDiff > sDiff) rr = -rq - rs;

        return new Vector2Int(rq, rr);
    }
}