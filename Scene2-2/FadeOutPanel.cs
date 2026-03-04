using UnityEngine;

public class FadeOutPanel : MonoBehaviour
{
    public CanvasGroup targetPanel;  // 알파 조절할 패널 (CanvasGroup이 붙어 있어야 함)
    public float delay = 15f;         // 몇 초 후에 시작할지
    public float fadeDuration = 1f;  // 부드럽게 나타나는 데 걸리는 시간

    private float timer = 0f;
    private bool isFading = false;

    void Start()
    {
        if (targetPanel != null)
        {
            targetPanel.alpha = 0f;         // 처음엔 보이지 않게
            targetPanel.interactable = false;
            targetPanel.blocksRaycasts = false;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (!isFading && timer >= delay)
        {
            isFading = true;
            this.TryStartCoroutine(FadeIn());
        }
    }

    System.Collections.IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            targetPanel.alpha = t;
            yield return null;
        }

        targetPanel.alpha = 1f;
        targetPanel.interactable = true;
        targetPanel.blocksRaycasts = true;
    }
}
