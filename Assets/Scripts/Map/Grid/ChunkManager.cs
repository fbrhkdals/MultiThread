using UnityEngine;
using System.Collections.Generic;

public class ChunkManager : MonoBehaviour
{
    private List<GameObject> chunks;
    private Transform camTransform;
    private float viewDistSqr;
    public float viewDistance = 150f;

    public void Initialize(List<GameObject> chunkList, Transform camera)
    {
        chunks = chunkList;
        camTransform = camera;
        viewDistSqr = viewDistance * viewDistance;
        InvokeRepeating(nameof(UpdateVisibleChunks), 0.5f, 0.5f);
    }

    private void UpdateVisibleChunks()
    {
        if (camTransform == null || chunks == null) return;

        Vector3 camPos = camTransform.position;
        for (int i = 0; i < chunks.Count; i++)
        {
            if (chunks[i] == null) continue;
            float sqrDist = (camPos - chunks[i].transform.position).sqrMagnitude;
            bool isVisible = sqrDist <= viewDistSqr;

            if (chunks[i].activeSelf != isVisible)
                chunks[i].SetActive(isVisible);
        }
    }
}