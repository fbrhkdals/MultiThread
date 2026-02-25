using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Settings")]
    public string mapFileName = "Map";
    public List<GameObject> tilePrefabs;
    public Transform playerCamera;

    [Header("Systems")]
    public PathfindingManager pathfinding;
    public ChunkManager chunkManager;

    [Header("Environment")]
    [SerializeField] private GameObject seaPlanePrefab;
    [SerializeField] private int seaPadding = 15;
    private GameObject seaInstance;

    private HexGrid hexGrid;
    private float hexSize;

    public HexGrid GetHexGrid() => hexGrid;
    public float HexSize => hexSize;
    public Vector2 MapMin { get; private set; }
    public Vector2 MapMax { get; private set; }

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main.transform;
        CalculateHexSize();
        LoadAndGenerateMap();
    }

    private void CalculateHexSize()
    {
        if (tilePrefabs.Count > 0)
        {
            var mesh = tilePrefabs[0].GetComponentInChildren<MeshFilter>().sharedMesh;
            hexSize = mesh.bounds.extents.x / (Mathf.Sqrt(3f) / 2f);
        }
    }

    private void LoadAndGenerateMap()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Map/" + mapFileName);
        if (jsonFile == null) return;

        MapData mapData = JsonUtility.FromJson<MapData>(jsonFile.text);

        hexGrid = new HexGrid();
        hexGrid.Initialize(mapData.width, mapData.height, mapData.tiles, tilePrefabs);

        // 하위 시스템 초기화
        pathfinding.Initialize(mapData.width, mapData.height, hexGrid);

        var renderer = gameObject.AddComponent<GridMapRenderer>();
        StartCoroutine(renderer.GenerateMapRoutine(mapData, 20, hexSize, tilePrefabs, (chunks, min, max) => {
            MapMin = min;
            MapMax = max;
            chunkManager.Initialize(chunks, playerCamera);

            CreateSeaPlane();
        }));
    }

    private void CreateSeaPlane()
    {
        if (seaPlanePrefab == null) return;

        float padX = seaPadding * hexSize * 1.7f;
        float padZ = seaPadding * hexSize * 1.5f;
        float width = (MapMax.x - MapMin.x) + (padX * 2);
        float height = (MapMax.y - MapMin.y) + (padZ * 2);
        Vector3 center = new Vector3((MapMin.x + MapMax.x) / 2f, -0.1f, (MapMin.y + MapMax.y) / 2f);

        seaInstance = Instantiate(seaPlanePrefab, transform);
        seaInstance.transform.position = center;
        seaInstance.transform.localScale = new Vector3(width / 10f, 1f, height / 10f);
    }
}