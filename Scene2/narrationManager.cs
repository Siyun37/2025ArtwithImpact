using UnityEngine;
using TMPro;
using System.Collections;
using System.IO;
using System.Diagnostics;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class narrationManager : MonoBehaviour
{
    [Header("🔤 JSON에서 불러올 나레이션 파일")]
    public string jsonFileName = "ani2Narration"; // 확장자 제외

    [Header("🎬 UI 요소")]
    public TMP_Text narrationText;
    public CanvasGroup narrationGroup;

    [Header("📋 불러온 나레이션 데이터")]
    public NarrationLine[] narrationLines; // JSON에서 불러온 값 자동으로 들어옴 (인스펙터 확인 가능)

    [Header("나레이션 후 씬 전환")]
    public string nextSceneName;

    [Header("페이드아웃 블랙 패널")]
    public CanvasGroup blackPanel;
    public float sceneFadeDuration = 1f;

    [Header("BGM")]
    public AudioSource bgmAudioSource;


    void Awake()
    {
        if (narrationText != null) narrationText.text = "";
        if (narrationGroup != null) narrationGroup.alpha = 0f;
    }


    void Start()
    {
        LoadNarrationFromJson();
        if (narrationLines != null && narrationLines.Length > 0)
            StartCoroutine(PlayNarration());
        else
            Debug.LogWarning("No Narration data");
    }

    void LoadNarrationFromJson()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(jsonFileName);
        if (jsonFile == null)
        {
            Debug.LogError("🟥 JSON 파일을 찾을 수 없습니다: " + jsonFileName);
            return;
        }

        //narrationLines = JsonHelper.FromJson<NarrationLine>(jsonFile.text);
        narrationLines = JsonHelper.FromJsonList<NarrationLine>(jsonFile.text).ToArray();
    }

    IEnumerator PlayNarration()
    {
        for (int i = 0; i < narrationLines.Length; i++)
        {
            float preDelay = narrationLines[i].preDelay;
            float postDelay = narrationLines[i].postDelay;
            string line = narrationLines[i].text;

            if (string.IsNullOrEmpty(line))
            {
                narrationText.text = "";
                narrationGroup.alpha = 0f;
                if (preDelay > 0f) yield return new WaitForSeconds(preDelay);
                if (postDelay > 0f) yield return new WaitForSeconds(postDelay);
                continue;
            }

            if (preDelay > 0f)
            {
                // 자막 숨기고 대기
                narrationText.text = "";
                narrationGroup.alpha = 0f;
                yield return new WaitForSeconds(preDelay);

                // 자막 등장
                narrationText.text = line;
                yield return FadeInText();
            }
            else
            {
                // 🔥 숨기지도 않고 바로 보여줌
                narrationText.text = line;
                narrationGroup.alpha = 1f;
            }

            // 자막 유지 시간
            if (postDelay > 0f) yield return new WaitForSeconds(postDelay);


            // 마지막 자막이 아니면 페이드아웃
            if (i < narrationLines.Length - 1)
            {
                yield return FadeOutText();
            }
        }

        // 마지막 자막 숨기기
        narrationText.text = "";
        narrationGroup.alpha = 0f;


        // 페이드아웃 후 씬 전환
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            yield return FadeOutScreenAndAudio(sceneFadeDuration, useUnscaledTime: true);
            SceneManager.LoadScene(nextSceneName);
        }
    }

     // 화면과 오디오를 '같은 타이머'로 함께 보간하는 안전한 페이드
    IEnumerator FadeOutScreenAndAudio(float duration, bool useUnscaledTime = false)
    {
        // BGM 소스 찾기(드래그 우선, 없으면 이름으로)
        if (bgmAudioSource == null)
        {
            Debug.Log("bgm not found");
        }

        float t = 0f;
        float startAlpha = blackPanel ? blackPanel.alpha : 0f;
        float startVol = (bgmAudioSource != null) ? bgmAudioSource.volume : 0f;

        // duration이 0 이하거나, 이미 목표치면 즉시 적용
        if (duration <= 0f)
        {
            if (blackPanel) blackPanel.alpha = 1f;
            if (bgmAudioSource) { bgmAudioSource.volume = 0f; bgmAudioSource.Stop(); }
            yield break;
        }

        while (t < duration)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);

            if (blackPanel) blackPanel.alpha = Mathf.Lerp(startAlpha, 1f, k);
            if (bgmAudioSource) bgmAudioSource.volume = Mathf.Lerp(startVol, 0f, k);

            yield return null;
        }

        if (blackPanel)
        {
            blackPanel.alpha = 1f;
            blackPanel.interactable = true;
            blackPanel.blocksRaycasts = true;
        }

        if (bgmAudioSource)
        {
            bgmAudioSource.volume = 0f;
            bgmAudioSource.Stop();
        }
    }


    IEnumerator FadeInText()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            narrationGroup.alpha = Mathf.Lerp(0, 1, t);
            yield return null;
        }
    }

    IEnumerator FadeOutText()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            narrationGroup.alpha = Mathf.Lerp(1, 0, t);
            yield return null;
        }
    }
}

[System.Serializable]
public class NarrationLine
{
    public string text;
    public float preDelay;
    public float postDelay;
}


