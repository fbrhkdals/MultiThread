using UnityEngine;

public class Character : MonoBehaviour
{
    [field: SerializeField] public int CharacterId { get; private set; }
    public Vector2Int CurrentAxial => HexUtils.WorldToAxial(transform.position, GridManager.Instance.HexSize);

    [Header("Settings")]
    public float moveSpeed = 5f;

    // 상태 참조용
    public CharacterMover Mover { get; private set; }
    public Node CurrentNode { get; set; }

    public void Initialize(int characterId, Vector2Int axial)
    {
        CharacterId = characterId;
        Mover = GetComponent<CharacterMover>();

        float h = CharacterFactory.Instance != null ? CharacterFactory.Instance.TileHeight : 0.2f;
        Vector3 worldPos = HexUtils.AxialToWorld(axial, GridManager.Instance.HexSize);
        worldPos.y = h;
        transform.position = worldPos;

        CurrentNode = GridManager.Instance.GetHexGrid().GetNode(axial);
        CurrentNode?.SetCharacter(this);
    }
    public void SetSelected(bool isSelected)
    {
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
            renderer.material.color = isSelected ? Color.yellow : Color.white;
    }
}