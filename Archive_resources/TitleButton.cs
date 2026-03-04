using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;

public class TitleButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text titleText;
    public TMP_Text typeText;
    public TMP_Text dateText;

    private ArtworkMetadata data;

    public void Init(ArtworkMetadata metadata, int number)
    {
        data = metadata;

        //제목
        titleText.text = $"{data.title}";
        typeText.text = data.type;

        titleText.enableWordWrapping = false;
        titleText.overflowMode = TextOverflowModes.Ellipsis;
        titleText.enableAutoSizing = false;
        titleText.maxVisibleLines = 1;


        if (dateText != null)
        {
            dateText.text = data.date.Substring(0, 16).Replace("T", " ");
        }

        // 클릭 이벤트
            GetComponent<Button>().onClick.AddListener(() =>
        {
            SelectedCardDataHolder.selectedMetadata = data;

            if (data.type == "model" || data.type == "3d")
            {
                // SelectedCardDataHolder.selectedMetadata = data;
                SceneManager.LoadScene("ModelCardScene");
            }

            else if (data.type == "poem")
            {
                SceneManager.LoadScene("PoemCardScene");
            }

            else if (data.type == "skybox")
            {

                // 🔁 file_name → public URL로 변환
                string url = SupabaseUploader.Instance.GetPublicUrl(data.file_name);

                // ✅ static 스토리지에 저장
                SkyboxResultStorage.generatedSkyboxUrl = url;
                SkyboxResultStorage.selectedPrompt = data.prompt;

                Debug.Log("📦 저장됨: " + url);

                SceneManager.LoadScene("SkyboxCardScene");
            }

            else
            {
                // SelectedCardDataHolder.selectedMetadata = data;
                SceneManager.LoadScene("ImageCardScene");
            }
                
        });
    }

    // 🖱️ Hover → Bold
    public void OnPointerEnter(PointerEventData eventData)
    {
        titleText.fontStyle = FontStyles.Bold;
        typeText.fontStyle = FontStyles.Bold;
        dateText.fontStyle = FontStyles.Bold;
    }

    // 🖱️ Hover Exit → Normal
    public void OnPointerExit(PointerEventData eventData)
    {
        titleText.fontStyle = FontStyles.Normal;
        typeText.fontStyle = FontStyles.Normal;
        dateText.fontStyle = FontStyles.Normal;
    }
}
