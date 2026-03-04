using UnityEngine;
using System.Collections;

public class popupFading : MonoBehaviour
{
    public float moveDistance = 100f;      // 시작/종료 위치 오프셋
    public float duration = 0.5f;          // 페이드 인/아웃 시간
    public float waitTime = 20f;           // 몇 초 후에 자동 팝업할지

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPos;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        originalPos = rectTransform.anchoredPosition;

        // ✅ 처음엔 안 보이게 초기화
        canvasGroup.alpha = 0f;
        rectTransform.anchoredPosition = originalPos - new Vector2(0, moveDistance);
    }

    void Start()
    {
        StartCoroutine(DelayedFadeIn());
    }

    IEnumerator DelayedFadeIn()
    {
        yield return new WaitForSeconds(waitTime);
        PlayFadeIn();
    }

    public void PlayFadeIn()
    {
        StopAllCoroutines();
        StartCoroutine(FadeInCoroutine());
    }

    public void PlayFadeOut()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutCoroutine());
    }

    IEnumerator FadeInCoroutine()
    {
        float t = 0f;
        Vector2 startPos = originalPos + new Vector2(0, moveDistance); // 위에서 아래로
        Vector2 endPos = originalPos;

        canvasGroup.alpha = 0f;
        rectTransform.anchoredPosition = startPos;

        while (t < duration)
        {
            t += Time.deltaTime;
            float eased = Mathf.SmoothStep(0f, 1f, t / duration);

            canvasGroup.alpha = eased;
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, eased);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        rectTransform.anchoredPosition = endPos;
    }

    IEnumerator FadeOutCoroutine()
    {
        float t = 0f;
        Vector2 startPos = originalPos;
        Vector2 endPos = originalPos + new Vector2(0, moveDistance);

        canvasGroup.alpha = 1f;
        rectTransform.anchoredPosition = startPos;

        while (t < duration)
        {
            t += Time.deltaTime;
            float eased = Mathf.SmoothStep(0f, 1f, t / duration);

            canvasGroup.alpha = 1f - eased;
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, eased);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        rectTransform.anchoredPosition = endPos;
    }
}
