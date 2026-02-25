using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 20f;
    public float dragSpeed = 0.02f;

    [Header("Zoom")]
    public float zoomSpeed = 10f;
    public float minZoom = -10f;
    public float maxZoom = 10f;

    public Transform cameraPivot;   // 회전 담당 (확장용)
    public Transform cam;           // MainCamera
    private Camera camCamera;

    private float currentZoom;      // 현재 줌 값 저장
    private const string ZOOM_KEY = "CameraZoom";

    void Start()
    {
        if (cam != null)
            camCamera = cam.GetComponent<Camera>();  // Camera 컴포넌트 가져오기

        // 저장된 줌 값 불러오기
        if (PlayerPrefs.HasKey(ZOOM_KEY))
            currentZoom = PlayerPrefs.GetFloat(ZOOM_KEY);
        else
            currentZoom = 0f; // 기본 줌 값

        ApplyZoom(currentZoom);
    }

    void Update()
    {
        HandleKeyboardMovement();
        HandleMouseDrag();
        HandleZoom();
        HandleTouchInput();

        // ==========================
        // 맵 범위 내에서 카메라 위치 제한
        // ==========================
        ClampPositionToMap();
    }

    // ==========================
    // PC 이동
    // ==========================
    void HandleKeyboardMovement()
    {
        if (Keyboard.current == null) return;

        Vector2 input = new Vector2(
            (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0),
            (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0)
        );

        Vector3 move = new Vector3(input.x, 0, input.y);
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
    }

    void HandleMouseDrag()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            Vector3 move = new Vector3(-delta.x, 0, -delta.y);
            transform.Translate(move * dragSpeed, Space.World);
        }
    }

    void HandleZoom()
    {
        if (Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;

        if (scroll != 0)
        {
            currentZoom += scroll * zoomSpeed * Time.deltaTime;
            currentZoom = Mathf.Clamp(currentZoom, -maxZoom, -minZoom);

            ApplyZoom(currentZoom);
            SaveZoom();
        }
    }

    // ==========================
    // 모바일 터치 처리
    // ==========================
    void HandleTouchInput()
    {
        if (Touchscreen.current == null) return;

        var touches = Touchscreen.current.touches;

        int activeTouchCount = 0;
        int first = -1;
        int second = -1;

        // 한 손가락 / 두 손가락 구분
        for (int i = 0; i < touches.Count; i++)
        {
            if (touches[i].isInProgress)
            {
                if (activeTouchCount == 0) first = i;
                if (activeTouchCount == 1) second = i;
                activeTouchCount++;
            }
        }

        // 한 손가락 드래그
        if (activeTouchCount == 1)
        {
            Vector2 delta = touches[first].delta.ReadValue();
            Vector3 move = new Vector3(-delta.x, 0, -delta.y);
            transform.Translate(move * dragSpeed, Space.World);
        }

        // 두 손가락 핀치 줌
        if (activeTouchCount >= 2)
        {
            var touch0 = touches[first];
            var touch1 = touches[second];

            Vector2 prevPos0 = touch0.position.ReadValue() - touch0.delta.ReadValue();
            Vector2 prevPos1 = touch1.position.ReadValue() - touch1.delta.ReadValue();

            float prevDist = Vector2.Distance(prevPos0, prevPos1);
            float currentDist = Vector2.Distance(
                touch0.position.ReadValue(),
                touch1.position.ReadValue()
            );

            float pinchAmount = currentDist - prevDist;

            currentZoom += pinchAmount * 0.01f;
            currentZoom = Mathf.Clamp(currentZoom, -maxZoom, -minZoom);

            ApplyZoom(currentZoom);
            SaveZoom();
        }
    }

    // ==========================
    // 줌 적용 함수 (공통)
    // ==========================
    void ApplyZoom(float zoomValue)
    {
        Vector3 pos = cam.localPosition;
        pos.z = zoomValue;
        cam.localPosition = pos;
    }

    void SaveZoom()
    {
        PlayerPrefs.SetFloat(ZOOM_KEY, currentZoom);
        PlayerPrefs.Save();
    }

    // ==========================
    // 맵 범위 내로 카메라 제한
    // ==========================
    void ClampPositionToMap()
    {
        if (GridManager.Instance == null || camCamera == null) return;

        Vector2 min = GridManager.Instance.MapMin;
        Vector2 max = GridManager.Instance.MapMax;

        float height = camCamera.transform.position.y;

        float halfHeight = height *
            Mathf.Tan(camCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);

        float halfWidth = halfHeight * camCamera.aspect;

        // 좌우 offset
        float sideOffset = halfWidth * 0.8f;

        // 상하 offset
        float topOffset = halfHeight * 1.5f;
        float bottomOffset = halfWidth * 0.3f;

        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, min.x + sideOffset, max.x - sideOffset);
        pos.z = Mathf.Clamp(pos.z, min.y - bottomOffset, max.y - topOffset);

        transform.position = pos;
    }

}
