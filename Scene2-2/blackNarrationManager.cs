using UnityEngine;
using TMPro;
using System.Collections;
using System.IO;

public class blackNarrationManager : MonoBehaviour
{
    [Header("🔤 JSON에서 불러올 나레이션 파일")]
    public string jsonFileName = "ani2Narration"; // 확장자 제외

    [Header("🎬 UI 요소")]
    public TMP_Text narrationText;
    public CanvasGroup narrationGroup;

    [Header("📋 불러온 나레이션 데이터")]
    public NarrationLine[] narrationLines; // JSON에서 불러온 값 자동으로 들어옴 (인스펙터 확인 가능)




    void Start()
    {
        LoadNarrationFromJson();
        this.TryStartCoroutine(PlayNarration());
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
            yield return new WaitForSeconds(postDelay);

            if (i < narrationLines.Length - 1)
            {
                yield return FadeOutText();
            }
        }

        // 마지막 자막 숨기기
        narrationText.text = "";

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

