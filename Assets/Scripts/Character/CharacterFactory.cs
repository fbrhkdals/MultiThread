using UnityEngine;

public class CharacterFactory : MonoBehaviour
{
    public static CharacterFactory Instance { get; private set; }

    [SerializeField] private Character characterPrefab;
    [Header("Spawn Settings")]
    [SerializeField] private float tileHeight = 0.2f; // 타일의 실제 윗면 높이값

    public float TileHeight => tileHeight;

    private int idCounter = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public Character SpawnCharacter(Vector2Int axial)
    {
        Vector3 worldPos = HexUtils.AxialToWorld(axial, GridManager.Instance.HexSize);

        worldPos.y = tileHeight;

        Character character = Instantiate(characterPrefab, worldPos, Quaternion.identity);
        character.Initialize(idCounter++, axial);

        return character;
    }
}