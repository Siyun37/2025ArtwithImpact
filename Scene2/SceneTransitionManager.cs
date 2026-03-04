using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("다음으로 넘어갈 씬 이름")]
    public string nextSceneName;

    // 즉시 로드
    public void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("씬 이름이 비어있습니다.");
        }
    }

    // 딜레이 후 로드
    public void LoadNextSceneWithDelay(float delay)
    {
        this.TryStartCoroutine(LoadAfterDelay(delay));
    }

    private System.Collections.IEnumerator LoadAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadNextScene();
    }
}
