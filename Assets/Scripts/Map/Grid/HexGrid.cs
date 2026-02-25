using System.Collections.Generic;
using UnityEngine;

public class HexGrid
{
    private readonly Dictionary<Vector2Int, Node> nodeMap
        = new Dictionary<Vector2Int, Node>();

    public void Initialize(int width, int height, int[] tiles, List<GameObject> prefabs)
    {
        nodeMap.Clear();

        for (int r = 0; r < height; r++)
        {
            for (int q = 0; q < width; q++)
            {
                int tileType = tiles[r * width + q];
                Vector2Int axial = new Vector2Int(q - (r / 2), r);

                // 중요: 프리팹 리스트에서 해당 타입의 TileComponent를 찾아 실제 walkable 값을 가져옴
                bool walkable = false;
                if (tileType >= 0 && tileType < prefabs.Count)
                {
                    var tileComp = prefabs[tileType].GetComponent<TileComponent>();
                    if (tileComp != null && tileComp.TileData != null)
                    {
                        // TileData 안에 있는 실제 이동 가능 여부를 사용 (변수명은 확인 필요)
                        walkable = tileComp.TileData.Walkable;
                    }
                }

                nodeMap[axial] = new Node(axial, walkable);
            }
        }
    }

    public Node GetNode(Vector2Int axial)
    {
        nodeMap.TryGetValue(axial, out Node node);
        return node;
    }
}
