using UnityEngine;
using UnityEngine.InputSystem;

public class MapClickHandler : MonoBehaviour
{
    private Node pendingNode; // 생성 대기 노드
    private Character selectedCharacter; // 현재 선택된 캐릭터

    private void Update()
    {
        // 마우스 클릭 감지
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleMapClick();
        }

        // 생성 확정
        if (Keyboard.current != null && Keyboard.current.yKey.wasPressedThisFrame)
        {
            ConfirmSpawn();
        }

        // 생성 취소
        if (Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame)
        {
            CancelSpawn();
        }
    }

    private void HandleMapClick()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float enterDistance))
        {
            Vector3 hitPoint = ray.GetPoint(enterDistance);
            Vector2Int axial = HexUtils.WorldToAxial(hitPoint, GridManager.Instance.HexSize);
            Node node = GridManager.Instance.GetHexGrid()?.GetNode(axial);

            if (node == null)
            {
                DeselectAll();
                return;
            }

            // 1. 캐릭터가 있는 노드를 클릭한 경우 (선택/교체)
            if (node.IsOccupied)
            {
                CancelSpawn();
                SelectCharacter(node.OccupiedCharacter);
            }
            // 2. 빈 노드를 클릭한 경우
            else
            {
                if (!node.walkable)
                {
                    Debug.Log($"[{node.axial}] 이동 불가 지역입니다.");
                    DeselectAll();
                    return;
                }

                // 캐릭터가 선택되어 있다면 이동 명령 수행!
                if (selectedCharacter != null)
                {
                    CancelSpawn();

                    Vector2Int target = node.axial;

                    // 자기 자신 위치를 클릭한 경우 무시
                    if (selectedCharacter.CurrentAxial == target) return;

                    // Character 스크립트 내부의 SetNewDestination이 비동기 길찾기를 시작합니다.
                    selectedCharacter.Mover.SetDestination(target);

                    Debug.Log($"{target}으로 이동 명령을 내렸습니다.");
                }
                // 캐릭터가 선택되지 않았다면 생성 프로세스 진행
                else
                {
                    if (pendingNode != node)
                    {
                        CancelSpawn();
                        ProcessNodeSelection(node);
                    }
                }
            }
        }
    }

    private void SelectCharacter(Character character)
    {
        if (selectedCharacter == character) return;

        // 기존 선택 해제
        if (selectedCharacter != null) selectedCharacter.SetSelected(false);

        // 새 캐릭터 선택 및 하이라이트
        selectedCharacter = character;
        selectedCharacter.SetSelected(true);
        Debug.Log($"캐릭터 {character.CharacterId} 선택됨");
    }

    private void DeselectCharacter()
    {
        if (selectedCharacter == null) return;
        selectedCharacter.SetSelected(false);
        selectedCharacter = null;
    }

    private void DeselectAll()
    {
        CancelSpawn();
        DeselectCharacter();
    }

    private void ProcessNodeSelection(Node node)
    {
        if (!node.walkable)
        {
            Debug.Log($"[{node.axial}] 이동 불가 지역");
            return;
        }

        if (node.IsOccupied)
        {
            Debug.Log($"[{node.axial}] 이미 캐릭터가 있음");
            return;
        }

        pendingNode = node;

        Debug.Log(
            $"[{node.axial}] 여기에 캐릭터를 생성할까요?\n" +
            $"Y: 생성 / N: 취소"
        );
    }

    private void ConfirmSpawn()
    {
        if (pendingNode == null) return;

        Character character =
            CharacterFactory.Instance.SpawnCharacter(pendingNode.axial);

        pendingNode.SetCharacter(character);

        Debug.Log(
            $"[{pendingNode.axial}] 캐릭터 생성 완료 (ID: {character.CharacterId})"
        );

        SelectCharacter(character);

        pendingNode = null;
    }

    private void CancelSpawn()
    {
        if (pendingNode == null) return;

        Debug.Log($"[{pendingNode.axial}] 생성 취소됨");

        pendingNode = null;
    }
}
