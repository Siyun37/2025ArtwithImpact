using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonSceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneNameToLoad = "";

    public void LoadScene()
    {

        if (sceneNameToLoad == "01_START")
        {
            SceneTracker.Reset(); // 🎯 처음으로 돌아갈 때만 리셋
            StartCoroutine(GameHardReset.ToFirstScene("01_START"));
            return; // 아래 로직 타지 않게 종료
        }

        if (sceneNameToLoad == "exitScene")
        {
            // 이전 씬이 어디였는지 보고 분기
            if (SceneTracker.previousSceneName == "15_ArtworkArchive")
            {
                SceneManager.LoadScene("15_ArtworkArchive");
            }
            else
            {
                SceneManager.LoadScene("13_MAIN");
            }
        }

        else
        {
            SceneManager.LoadScene(sceneNameToLoad);
        }
    }
}
