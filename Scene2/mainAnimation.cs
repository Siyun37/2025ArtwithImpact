using UnityEngine;
using UnityEngine.AI;

public class mainAnimation : MonoBehaviour
{
    private AINavMovement movement;
    private NavMeshAgent agent;

    [Header("Animators")]
    public Animator charaAnimator;          // 캐릭터 애니메이터 (이 스크립트가 붙은 오브젝트)
    public Animator bottleAnimator;         // 병 애니메이터 (인스펙터에서 할당)

    [Header("애니메이션 설정")]
    public float pickDelay = 10.0f;         // (미사용: 위치 기반으로 트리거)
    public float noddingDelay = 7.0f;       // pick 끝나고 idle에서 대기 후 nodding
    public string nodTriggerName = "nod";
    public string idleStateName = "idle";   // 캐릭터 Animator의 idle 상태 이름

    private bool hasPicked = false;
    private float timer = 0f;               // (미사용)
    private Coroutine noddingRoutine;

    [Header("Bottle State")]
    public string openBottleStateName = "openBottle"; // 병 Animator의 상태 이름
    public int openBottleLayer = 0;

    private bool playedOpenBottle = false;

    // ====== Z 기반 감속/트리거 (AINavMovement의 wait을 존중) ======
    [Header("Z 감속/트리거 설정")]
    [Tooltip("pick 애니메이션을 시작할 월드 Z 좌표(예: 505)")]
    public float targetPickZ = 505f;

    [Tooltip("targetPickZ에 이 거리만큼 접근하면 선형 감속 시작")]
    public float slowDownDistance = 2.0f;

    [Tooltip("Z 차이가 이 값 이하일 때 도착으로 간주")]
    public float zArrivalThreshold = 0.10f;

    [Tooltip("속도가 이 값 이하일 때 완전 정지로 간주")]
    public float arrivalSpeedThreshold = 0.05f;

    [Tooltip("(+Z로 진행 가정) targetPickZ를 지나쳤다면 즉시 도착 처리")]
    public bool treatOvershootAsArrived = true;

    private float originalAgentSpeed = -1f;
    private bool  isDecelerating = false;
    private bool  navHasStarted = false; // ★ AINavMovement가 wait을 끝내고 실제 출발했는지 감지

    void Start()
    {
        // 기본 할당 보조
        if (charaAnimator == null) charaAnimator = GetComponent<Animator>();
        agent    = GetComponent<NavMeshAgent>();
        movement = GetComponent<AINavMovement>();

        if (agent != null) originalAgentSpeed = agent.speed;

        // ★ 중요: 여기서 어떤 이동/SetDestination도 하지 않음
        // 출발 타이밍은 전적으로 AINavMovement(WaitAndMove)가 담당 → waitTime 존중
    }

    void Update()
    {
        if (!charaAnimator || hasPicked || agent == null) return;

        // ★ 출발 감지: AINavMovement가 wait 후 animator.moving을 켜고 경로를 잡으면 true
        if (!navHasStarted)
        {
            // 경로가 생기고 isStopped가 풀렸거나, moving 파라미터가 true면 '출발'로 간주
            navHasStarted = (!agent.isStopped && agent.hasPath) || charaAnimator.GetBool("moving");
            if (!navHasStarted) return; // 아직 대기 중 → 아무것도 하지 않음
        }

        // === 여기부터는 AINavMovement가 출발시킨 뒤에만 실행 ===

        // 목표 pick Z까지 남은 거리(+Z 진행 가정)
        float remainingToPickZ = targetPickZ - transform.position.z;

        // 1) 감속 구간: targetPickZ에서 slowDownDistance 안으로 들어오면 선형 감속
        if (remainingToPickZ <= slowDownDistance && remainingToPickZ > 0f)
        {
            isDecelerating = true;

            // 0(도착)~1(감속 시작점) 사이 t 계산
            float t = Mathf.Clamp01(remainingToPickZ / Mathf.Max(0.0001f, slowDownDistance));
            float targetSpeed = Mathf.Lerp(0f, originalAgentSpeed, t); // 선형 감속
            agent.speed = targetSpeed;
        }

        // 2) 도착 판정: Z 임계 + 속도 임계
        bool zArrived   = Mathf.Abs(transform.position.z - targetPickZ) <= zArrivalThreshold;
        bool slowEnough = agent.velocity.magnitude <= arrivalSpeedThreshold;

        // 오버슈트(넘어섬) 대비: pickZ를 지난 경우 도착 처리
        bool overshot = remainingToPickZ < 0f;

        if ((zArrived && slowEnough) || (treatOvershootAsArrived && overshot))
        {
            // 정확히 멈춤
            if (charaAnimator.GetBool("moving")) charaAnimator.SetBool("moving", false);
            agent.isStopped = true;
            agent.ResetPath();

            // 속도 원복(다음 이동을 대비)
            agent.speed = originalAgentSpeed;
            isDecelerating = false;

            // 집기 시작
            charaAnimator.SetBool("pick", true);
            hasPicked = true;
            return;
        }

        // 3) 아직 감속 전이면 원래 속도로 유지
        if (!isDecelerating && agent.speed != originalAgentSpeed)
        {
            agent.speed = originalAgentSpeed;
        }
    }

    // pickfromtree 마지막 프레임 이벤트
    public void OnPickEnd()
    {
        if (!charaAnimator) return;

        charaAnimator.SetBool("pick", false);
        if (movement) movement.hasExited = true;

        // idle 복귀 확인 후 nod 트리거
        if (noddingRoutine != null) StopCoroutine(noddingRoutine);
        noddingRoutine = StartCoroutine(WaitIdleThenNod());

        // 병 애니메이션은 병 Animator로 재생
        if (!playedOpenBottle && bottleAnimator)
        {
            playedOpenBottle = true;

            // 존재 체크 후 재생(안전)
            if (HasState(bottleAnimator, openBottleLayer, openBottleStateName))
            {
                bottleAnimator.Play(openBottleStateName, openBottleLayer, 0f);
                // 필요하면 CrossFade 사용:
                // bottleAnimator.CrossFade(openBottleStateName, 0.1f, openBottleLayer, 0f);
            }
            else
            {
                Debug.LogWarning($"[mainAnimation] Bottle animator에 '{openBottleStateName}' 상태가 없습니다.");
            }
        }
    }

    private System.Collections.IEnumerator WaitIdleThenNod()
    {
        // 캐릭터가 idle로 완전히 돌아올 때까지 대기
        while (!IsInState(charaAnimator, 0, idleStateName))
            yield return null;

        // idle에서 noddingDelay만큼 대기
        float t = 0f;
        while (t < noddingDelay) { t += Time.deltaTime; yield return null; }

        // nod 트리거
        charaAnimator.ResetTrigger(nodTriggerName);
        charaAnimator.SetTrigger(nodTriggerName);
    }

    private bool IsInState(Animator anim, int layer, string stateName)
    {
        if (!anim) return false;
        var info = anim.GetCurrentAnimatorStateInfo(layer);
        return info.IsName(stateName);
    }

    private bool HasState(Animator anim, int layer, string stateName)
    {
        if (!anim) return false;
        int id = Animator.StringToHash(stateName);
        return anim.HasState(layer, id);
    }

    public void CameraCloseUp()
    {
        if (CameraController.Instance != null)
            CameraController.Instance.SwitchToCloseUpCamera();
    }
}
