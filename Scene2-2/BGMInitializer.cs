using UnityEngine;
using System.Collections;

public class BGMInitializer : MonoBehaviour
{
    [Header("🎶 오디오 설정")]
    public AudioSource bgmSource;
    public float targetVolume = 1f;
    public float fadeDuration = 3f;
    public float delayBeforeFadeIn = 5f;

    void Start()
    {
        

        if (bgmSource == null)
        {
            bgmSource = GetComponent<AudioSource>();
        }

        bgmSource.volume = 0f;      // 시작 볼륨 0
        bgmSource.Play();           // 즉시 재생 (소리는 안 들림)
        StartCoroutine(FadeInAfterDelay());
    }

    IEnumerator FadeInAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeFadeIn);

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / fadeDuration);
            Debug.Log("소리 재생");
            yield return null;
        }

        bgmSource.volume = targetVolume;
    }
}
