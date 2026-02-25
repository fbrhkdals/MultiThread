using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridMapRenderer : MonoBehaviour
{
    public IEnumerator GenerateMapRoutine(MapData mapData, int chunkSize, float hexSize, List<GameObject> tilePrefabs, System.Action<List<GameObject>, Vector2, Vector2> onComplete)
    {
        var chunkGroups = new Dictionary<Vector2Int, Dictionary<int, List<CombineInstance>>>();
        float minX = float.MaxValue, maxX = float.MinValue, minZ = float.MaxValue, maxZ = float.MinValue;

        for (int r = 0; r < mapData.height; r++)
        {
            for (int q = 0; q < mapData.width; q++)
            {
                int tileType = mapData.tiles[r * mapData.width + q];
                if (tileType < 0 || tileType >= tilePrefabs.Count) continue;

                Vector2Int axial = new Vector2Int(q - (r / 2), r);
                Vector3 worldPos = HexUtils.AxialToWorld(axial, hexSize);

                // 경계값 갱신
                minX = Mathf.Min(minX, worldPos.x); maxX = Mathf.Max(maxX, worldPos.x);
                minZ = Mathf.Min(minZ, worldPos.z); maxZ = Mathf.Max(maxZ, worldPos.z);

                Vector2Int chunkCoord = new Vector2Int(q / chunkSize, r / chunkSize);
                if (!chunkGroups.ContainsKey(chunkCoord))
                    chunkGroups[chunkCoord] = new Dictionary<int, List<CombineInstance>>();

                if (!chunkGroups[chunkCoord].ContainsKey(tileType))
                    chunkGroups[chunkCoord][tileType] = new List<CombineInstance>();

                chunkGroups[chunkCoord][tileType].Add(new CombineInstance
                {
                    mesh = tilePrefabs[tileType].GetComponentInChildren<MeshFilter>().sharedMesh,
                    transform = Matrix4x4.Translate(worldPos)
                });
            }
        }

        List<GameObject> chunkList = new List<GameObject>();
        int count = 0;
        foreach (var chunkPair in chunkGroups)
        {
            GameObject chunkObj = new GameObject($"Chunk_{chunkPair.Key.x}_{chunkPair.Key.y}");
            chunkObj.transform.SetParent(this.transform);

            // 첫 번째 타일 위치를 청크의 중심점으로 설정 (최적화용)
            Vector3 firstPos = chunkPair.Value[new List<int>(chunkPair.Value.Keys)[0]][0].transform.GetColumn(3);
            chunkObj.transform.position = firstPos;

            // 좌표 상대화 (로컬 좌표계로 변환)
            Matrix4x4 offset = Matrix4x4.Translate(-firstPos);
            foreach (var typeList in chunkPair.Value.Values)
            {
                for (int i = 0; i < typeList.Count; i++)
                {
                    CombineInstance ci = typeList[i];
                    ci.transform = offset * ci.transform;
                    typeList[i] = ci;
                }
            }

            var renderer = chunkObj.AddComponent<ChunkRenderer>();
            renderer.Initialize(chunkPair.Value, tilePrefabs);

            chunkList.Add(chunkObj);
            chunkObj.SetActive(false);

            if (++count % 5 == 0) yield return null;
        }

        onComplete?.Invoke(chunkList, new Vector2(minX, minZ), new Vector2(maxX, maxZ));
    }
}