using UnityEngine;

public class NotificationSoundPlayer : MonoBehaviour
{
    public float delayInSeconds = 26.5f;

    void Start()
    {
        Invoke(nameof(PlayEffect), delayInSeconds);
    }

    void PlayEffect()
    {
        AudioSource sfx = GetComponent<AudioSource>();
        if (sfx != null)
        {
            sfx.Play();
            Debug.Log("✅ 효과음 재생");
        }
    }
}
