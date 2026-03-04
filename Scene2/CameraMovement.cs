using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target;               // 처음 따라갈 대상
    public AINavMovement charMovement;     // 캐릭터 이동 스크립트

    public float followDuration = 15f;     // 따라가는 시간
    public float waitTime = 25f;           // 대기 시간

    public Transform mainchar;             // 나중에 트래킹할 캐릭터

    private float followTimer = 0f;
    private bool isFollowing = false;
    private bool waiting = false;
    private bool rotatingToMainchar = false;
    private bool rotatingBack = false;

    private float waitTimer = 0f;
    private float initialX;
    private float initialY;

    private Quaternion originalRotation;   // 시작 회전값 저장

    public float rotationSpeed = 50f;

    void Start()
    {
        if (target == null) Debug.LogWarning("Choose a character to follow");

        if (transform != null)
        {
            initialX = transform.position.x;
            initialY = transform.position.y;
            originalRotation = transform.rotation;
        }
    }

    void LateUpdate()
    {
        // 1. 최초 따라가기 시작 조건
        if (!isFollowing && charMovement.reachedCamera)
        {
            isFollowing = true;
            followTimer = 0f;
        }

        // 2. 따라가는 중 (Z축만 따라감)
        if (isFollowing && target != null && followTimer <= followDuration)
        {
            followTimer += Time.deltaTime;
            Vector3 pos = transform.position;
            pos.z = target.position.z;
            pos.x = initialX;
            pos.y = initialY;
            transform.position = pos;

            // 따라가기 완료되면 대기 시작
            if (followTimer >= followDuration)
            {
                waiting = true;
                waitTimer = 0f;
            }
        }

        // 3. 대기 중
        if (waiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                waiting = false;
                rotatingToMainchar = true;
            }
        }

        // 4. mainchar를 향해 회전
        if (rotatingToMainchar && mainchar != null)
        {
            Vector3 direction = mainchar.position - transform.position;
            direction.y = 0f;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // 매 프레임 트래킹: 카메라가 mainchar를 계속 향하게
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.5f)
            {
                // 도달하면, 다시 원래 각도로 돌아가기 시작
                rotatingToMainchar = false;
                rotatingBack = true;
            }
        }

        // 5. 원래 각도로 돌아가는 중
        if (rotatingBack)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, originalRotation, (rotationSpeed - 3f) * Time.deltaTime);

            if (Quaternion.Angle(transform.rotation, originalRotation) < 0.5f)
            {
                rotatingBack = false;
                // 끝
            }
        }
    }
}
