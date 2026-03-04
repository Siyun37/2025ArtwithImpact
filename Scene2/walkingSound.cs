using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepAudioLoop : MonoBehaviour
{
    private AudioSource audioSource;
    private Animator animator;
    private Renderer characterRenderer;
    private bool wasMoving = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        animator = GetComponent<Animator>();
        characterRenderer = GetComponentInChildren<Renderer>(); // 캐릭터 Mesh 기준

        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        bool isMoving = animator.GetBool("moving");
        bool isVisible = characterRenderer != null && characterRenderer.isVisible;

        if (isMoving && isVisible && !wasMoving)
        {
            audioSource.Play();
        }
        else if ((!isMoving || !isVisible) && wasMoving)
        {
            audioSource.Stop();
        }

        wasMoving = isMoving && isVisible;
    }
}
