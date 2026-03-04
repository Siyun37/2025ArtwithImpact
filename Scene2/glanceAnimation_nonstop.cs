using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class glanceAnimation_nonstop : MonoBehaviour
{
    private Animator animator;
    public float glanceDelay = 10.0f;
    private bool hasGlanced = false;

    private float timer = 0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (hasGlanced || animator == null) return;


        timer += Time.deltaTime;
        if (timer >= glanceDelay)
        {
            animator.SetBool("glance", true);
            hasGlanced = true;
        }
    }

    public void OnGlanceEnd()
    {
        animator.SetBool("glance", false);
        
    }
}
