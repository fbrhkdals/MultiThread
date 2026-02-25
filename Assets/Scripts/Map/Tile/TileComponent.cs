using UnityEngine;

public class TileComponent : MonoBehaviour
{
    [SerializeField] private TileData tileData;

    // 읽기 전용 접근
    public TileData TileData => tileData;
}
