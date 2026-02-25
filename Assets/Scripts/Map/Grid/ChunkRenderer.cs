using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class ChunkRenderer : MonoBehaviour
{
    private List<Mesh> generatedMeshes = new List<Mesh>();

    public void Initialize(Dictionary<int, List<CombineInstance>> typeData, List<GameObject> tilePrefabs)
    {
        foreach (var kvp in typeData)
        {
            int typeKey = kvp.Key;
            GameObject typeObj = new GameObject($"Type_{typeKey}");
            typeObj.transform.SetParent(this.transform, false);

            MeshFilter mf = typeObj.AddComponent<MeshFilter>();
            MeshRenderer mr = typeObj.AddComponent<MeshRenderer>();

            Mesh combinedMesh = new Mesh { indexFormat = IndexFormat.UInt32 };
            combinedMesh.CombineMeshes(kvp.Value.ToArray());

            mf.sharedMesh = combinedMesh;

            // 프리팹에서 재질 가져오기
            var prefabRenderer = tilePrefabs[typeKey].GetComponentInChildren<MeshRenderer>();
            mr.sharedMaterial = prefabRenderer.sharedMaterial;

            generatedMeshes.Add(combinedMesh);
        }
    }

    private void OnDestroy()
    {
        // 메모리 누수 방지: 생성된 메쉬 명시적 제거
        for (int i = 0; i < generatedMeshes.Count; i++)
        {
            if (generatedMeshes[i] != null) Destroy(generatedMeshes[i]);
        }
        generatedMeshes.Clear();
    }
}