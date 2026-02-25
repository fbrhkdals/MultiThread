using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Character))]
public class CharacterMover : MonoBehaviour
{
    private Character owner;
    private Coroutine moveCoroutine;
    private Vector2Int finalDestination;

    public bool IsMoving => moveCoroutine != null;

    private void Awake() => owner = GetComponent<Character>();

    public void SetDestination(Vector2Int targetPos)
    {
        finalDestination = targetPos;
        RequestPath(null);
    }

    private void RequestPath(Character blockTarget)
    {
        StopMovement();
        GridManager.Instance.pathfinding.RequestPathAsync(owner.CurrentAxial, finalDestination, owner, blockTarget, (path) => {
            if (path != null && path.Count > 0)
                moveCoroutine = StartCoroutine(FollowPathRoutine(path));
            else
                StopMovement();
        });
    }

    private IEnumerator FollowPathRoutine(List<Vector2Int> path)
    {
        float h = CharacterFactory.Instance != null ? CharacterFactory.Instance.TileHeight : 0.2f;

        while (path.Count > 0)
        {
            Vector2Int nextStep = path[0];
            Node nextNode = GridManager.Instance.GetHexGrid().GetNode(nextStep);

            // 점유 대기 로직 (기차놀이)
            if (nextNode != null && nextNode.IsOccupied && nextNode.OccupiedCharacter != owner)
            {
                yield return StartCoroutine(WaitOrRepath(nextNode));
                if (!IsMoving) yield break; // 우회 결정 시 루틴 종료
            }

            // 이동 실행
            UpdateOccupancy(nextNode);
            path.RemoveAt(0);

            Vector3 targetWorldPos = HexUtils.AxialToWorld(nextStep, GridManager.Instance.HexSize);
            targetWorldPos.y = h;

            // 회전 및 이동
            Vector3 lookDir = (targetWorldPos - transform.position).normalized;
            if (lookDir != Vector3.zero) transform.forward = lookDir;

            while (Vector3.Distance(transform.position, targetWorldPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, owner.moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = targetWorldPos;
        }
        moveCoroutine = null;
    }

    private IEnumerator WaitOrRepath(Node nextNode)
    {
        float timer = 0f;
        while (nextNode.IsOccupied && nextNode.OccupiedCharacter != owner)
        {
            timer += Time.deltaTime;
            // 앞차가 움직이면 타이머 유예
            if (nextNode.OccupiedCharacter?.Mover.IsMoving == true)
                timer = Mathf.Max(0, timer - Time.deltaTime * 0.8f);

            if (timer > 0.7f)
            {
                RequestPath(nextNode.OccupiedCharacter); // 우회
                yield break;
            }
            yield return null;
        }
    }

    public void StopMovement()
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = null;
    }

    private void UpdateOccupancy(Node nextNode)
    {
        owner.CurrentNode?.ClearCharacter();
        owner.CurrentNode = nextNode;
        owner.CurrentNode?.SetCharacter(owner);
    }
}