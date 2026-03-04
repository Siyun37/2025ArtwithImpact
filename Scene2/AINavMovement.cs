using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class AINavMovement : MonoBehaviour
{
    [Header("Timing")]
    public float waitTime = 0f;
    public float movingDuration = 25f;          // 애니메이션 시간 포함 전체 움직임 지속시간

    [Header("Move Direction")]
    public int direction = 1;                    // 1 = forward, -1 = back
    public float destinationFar = 400f;

    [Header("Start Position")]
    public bool useCustomStartPosition = false;
    public Vector3 startPosition;

    [Header("Refs")]
    public Transform cameraTransform;

    private NavMeshAgent agent;
    [HideInInspector] public Animator animator;

    [HideInInspector] public float movingTimer = 0f;
    [HideInInspector] public bool hasExited = false;
    [HideInInspector] public bool reachedCamera = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // 씬 11은 NavMeshAgent 주도 이동 권장
        animator.applyRootMotion = false;

        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.updateUpAxis = true;

        // ★ 시작 위치 동기화: transform.position 대신 반드시 Warp 사용
        if (useCustomStartPosition)
        {
            if (NavMesh.SamplePosition(startPosition, out var hit, 2f, NavMesh.AllAreas))
                agent.Warp(hit.position);
            else
                agent.Warp(startPosition);
        }
        else
        {
            // 커스텀 위치를 쓰지 않아도 내부 좌표와 동기화
            agent.Warp(transform.position);
        }

        movingTimer = 0f;
        hasExited = false;
        reachedCamera = false;
        agent.ResetPath();

        this.TryStartCoroutine(WaitAndMove());
    }

    IEnumerator WaitAndMove()
    {
        yield return new WaitForSeconds(waitTime);

        animator.SetBool("moving", true);

        // ★ 캐릭터의 로컬 앞 방향 기준으로 목적지 설정
        Vector3 destination = transform.position + transform.forward * direction * destinationFar;

        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogWarning("목적지 위치가 NavMesh에 없음: " + destination);
        }
    }

    void Update()
    {
        if (hasExited) return;

        // 이동/정지 스위치
        bool isMoving = animator.GetBool("moving");
        agent.isStopped = !isMoving;

        // 타이머 및 종료 처리
        movingTimer += Time.deltaTime;
        if (movingTimer >= movingDuration + waitTime)
        {
            agent.isStopped = true;                 // 안정화
            animator.SetFloat("speed", 0f);
            animator.SetTrigger("exit");
            hasExited = true;
            gameObject.SetActive(false);
            return;
        }

        // 애니 파라미터: 현재 에이전트 속도
        float speed = agent.velocity.magnitude;
        animator.SetFloat("speed", speed);

        // 카메라 지남 여부 체크(카메라가 없으면 스킵)
        if (!reachedCamera && cameraTransform != null)
        {
            if (transform.position.z >= cameraTransform.position.z)
                reachedCamera = true;
        }
    }
}
