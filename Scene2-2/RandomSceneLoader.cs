using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RandomSceneLoader : MonoBehaviour
{
    [Header("이동할 씬 목록 (3개)")]
    public string[] sceneNames = new string[] { "12-1_image", "12-2_model", "12-3_image"};

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(LoadRandomScene);
        }
        else
        {
            Debug.LogError("❌ Button 컴포넌트를 찾을 수 없습니다.");
        }
    }

    void LoadRandomScene()
    {
        if (sceneNames.Length == 0)
        {
            Debug.LogWarning("⚠️ 씬 이름이 설정되지 않았습니다.");
            return;
        }

        int index = Random.Range(0, sceneNames.Length);
        string selectedScene = sceneNames[index];

        if (!string.IsNullOrEmpty(selectedScene))
        {
            SceneManager.LoadScene(selectedScene);
        }
        else
        {
            Debug.LogWarning("⚠️ 선택된 씬 이름이 비어 있습니다.");
        }
    }
}
