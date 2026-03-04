using UnityEngine;

public class NoddingTrigger : MonoBehaviour
{
    private Animator animator;           // 연결할 Animator
    public float noddingDelay = 5f;     // 몇 초 뒤에 고개 끄덕임 시작할지 설정

    private float timer = 0f;
    private bool triggered = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (triggered)
        {
            animator.SetBool("isNodding", false);
            return;
        }

        timer += Time.deltaTime;

        if (timer >= noddingDelay)
        {
            animator.SetBool("isNodding", true);
            triggered = true;
        }
    }
}
