using UnityEngine;

[CreateAssetMenu(menuName = "Map/Tile Data")]
public class TileData : ScriptableObject
{
    [SerializeField] private bool walkable;

    // 읽기 전용
    public bool Walkable => walkable;
}
