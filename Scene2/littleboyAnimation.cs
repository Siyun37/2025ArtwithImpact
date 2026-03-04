using UnityEngine;

public class LittleBoyAnimation : MonoBehaviour
{
    public float zTrigger = 500f;
    public Vector3 targetPosition = new Vector3(103.6f, 2.8f, 510f);
    public float snapLerpSpeed = 5f;
    
    private Animator animator;

    private bool turnedAtTree = false;
    private bool isSnapping = false;


    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (turnedAtTree) return;

        float myZ = transform.position.z;

        if (!isSnapping && transform.position.z >= zTrigger)
        {
            isSnapping = true;
            animator.SetBool("moving", false);

        }

        if (isSnapping)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, snapLerpSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 1.5f)
            {
                transform.position = targetPosition;
                transform.rotation = Quaternion.identity;

                animator.SetBool("reachedTree", true);
                turnedAtTree = true;
                isSnapping = false;
            }
        }

    }

    public void OnTurnEnd()
    {
        animator.SetBool("behindTree", true); // 오른쪽으로 회전 이후, 나무에서의 애니메이션 때
        // Debug.Log("Turn Ended.");
    }

    public void OnTreeEnd()
    {
        animator.SetBool("behindTree", false); // 나무에서의 애니메이션 끝났을 때 (다시 뛰기 위한 준비 전)
        // Debug.Log("Second animation ended. Ready to run");
    }

    public void Re_Move()
    {
        animator.SetBool("moving", true);
        animator.SetBool("reachedTree", false); // 나무 애니메이션 완전히 끝
        // Debug.Log("[little boy] animation completely ended.");
        
    }
}
