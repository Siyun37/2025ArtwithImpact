using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]

public class CharacterMovement_noNav : MonoBehaviour
{
    public float waitTime = 40f;
    public float movingDuration = 20f;
    public float speed = 20f;
    public float acc = 20f;
    public LayerMask groundLayerMask;
    public float treeYAngle = 240f;

    [HideInInspector] public bool hasExited = false;

    private Animator animator;
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private float waitTimer = 0f;
    private float movingTimer = 0f;
    private bool hasStartedMoving = false;
    private float lerpSpeed = 10f;

    private float currentSpeed = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = true;

        capsuleCollider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();

    }

    void Update()
    {
        if (hasExited) return;

        if (!hasStartedMoving)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                StartMovement();
            }
            else return;
        }

        movingTimer += Time.deltaTime;

        bool isMoving = animator.GetBool("moving");

        if (isMoving)
        {
            // 가속도 반영하여 속도 증가
            currentSpeed = Mathf.MoveTowards(currentSpeed, speed, acc * Time.deltaTime);
            Vector3 move = Vector3.forward * currentSpeed * Time.deltaTime;
            transform.Translate(move, Space.World);
        }
        else
        {
            // 멈춤 처리
            currentSpeed = 0f;
        }

        if (animator.GetBool("reachedTree"))
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
        else
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }

        // 종료 처리
        if (movingTimer >= movingDuration)
        {
            animator.SetTrigger("exit");
            hasExited = true;
            gameObject.SetActive(false);
        }
    }

    void LateUpdate()
    {
        float characterBottomOffset = capsuleCollider.height / 2f;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName("Running"))
        {
            Vector3 rot = transform.eulerAngles;
            transform.eulerAngles = new Vector3(rot.x, 0f, rot.z);
        }
        else if (animator.GetBool("behindTree") == true) // 두 번째 애니메이션(나무 뒤) 재생 시, 다시 뛰기 위한 준비 전
        {
            Vector3 rot = transform.eulerAngles;

            Quaternion targetRotation = Quaternion.Euler(rot.x, treeYAngle, rot.z);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 180f * Time.deltaTime);

        }
        else { }

        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 5f, groundLayerMask))
        {
            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(pos.y, hit.point.y + characterBottomOffset, Time.deltaTime * lerpSpeed);
            transform.position = pos;
        }
    }

    void StartMovement()
    {
        hasStartedMoving = true;
        animator.SetBool("moving", true);
    }
}
