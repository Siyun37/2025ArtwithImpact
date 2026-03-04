using System.Collections;
using UnityEngine;

public class ImageFading : MonoBehaviour
{
    public float fadeDuration = 0.2f;   // 페이드 시간

    private CanvasGroup canvasGroup;
    private Coroutine fadeRoutine;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // 처음에는 안 보이게
        canvasGroup.alpha = 0f;
    }

    // 시퀀스 끝났을 때 호출해 줄 함수
    public void FadeIn()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        gameObject.SetActive(true);   // 혹시 꺼져 있으면 켜주기
        fadeRoutine = StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / fadeDuration);
            canvasGroup.alpha = a;
            yield return null;
        }

        canvasGroup.alpha = 1f;
        fadeRoutine = null;
    }
}
