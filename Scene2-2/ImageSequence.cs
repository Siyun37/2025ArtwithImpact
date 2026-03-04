using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ImageSequence : MonoBehaviour
{
    [Header("순서대로 보여줄 텍스처들 (마지막 이미지는 제외)")]
    public Texture[] frames;            // 3장 넣기

    [Header("타이밍 설정 (초)")]
    public float fadeInDuration = 0.2f;   // 서서히 나타나는 시간
    public float holdDuration = 0.1f;     // 완전히 보이는 상태 유지 시간
    // public float fadeOutDuration = 0.2f;  // 서서히 사라지는 시간

    [Header("패널/오브젝트가 켜질 때마다 자동 재생할지")]
    public bool playOnEnable = true;

    [Header("시퀀스가 끝났을 때 보낼 신호")]
    public UnityEvent onSequenceFinished;

    private RawImage rawImage;
    private Coroutine playRoutine;

    void Awake()
    {
        rawImage = GetComponent<RawImage>();

        // 처음에는 안 보이게 알파 0으로 세팅
        if (rawImage != null)
        {
            Color c = rawImage.color;
            c.a = 0f;
            rawImage.color = c;
        }
    }

    void OnEnable()
    {
        if (playOnEnable)
            Play();
    }

    public void Play()
    {
        if (playRoutine != null)
            StopCoroutine(playRoutine);

        playRoutine = StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        if (frames == null || frames.Length == 0 || rawImage == null)
            yield break;

        for (int i = 0; i < frames.Length; i++)
        {
            rawImage.texture = frames[i];
            rawImage.enabled = true;

            // 알파 0 → 1 페이드 인
            yield return StartCoroutine(FadeAlpha(0f, 1f, fadeInDuration));

            // 잠깐 유지
            if (holdDuration > 0f)
                yield return new WaitForSeconds(holdDuration);

            // 알파 1 → 0 페이드 아웃
            // yield return StartCoroutine(FadeAlpha(1f, 0f, fadeOutDuration));
        }

        // 다 끝나면 이 시퀀스 이미지는 안 보이게
        rawImage.enabled = false;

        // 마지막 이미지(따로 둔 오브젝트) 등에게 신호 보내기
        onSequenceFinished?.Invoke();

        playRoutine = null;
    }

    private IEnumerator FadeAlpha(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            Color cInstant = rawImage.color;
            cInstant.a = to;
            rawImage.color = cInstant;
            yield break;
        }

        float elapsed = 0f;
        Color c = rawImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            c.a = Mathf.Lerp(from, to, t);
            rawImage.color = c;
            yield return null;
        }

        // 마지막 값 보정
        c.a = to;
        rawImage.color = c;
    }
}
