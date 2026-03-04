using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    [Header("🧭 유지 시작 및 종료 씬")]
    public string startScene;    // 유지 시작 씬 (예: "ani2")
    public string endScene;      // 마지막 씬 (도달하면 제거됨)

    private bool shouldKeep = false;

    void Awake()
    {

        // 🔁 이미 동일한 BGMManager가 존재하면 자기 자신 제거
        BGMManager[] existingBGM = FindObjectsOfType<BGMManager>();
        foreach (var bgm in existingBGM)
        {
            if (bgm != this && bgm.startScene == this.startScene && bgm.endScene == this.endScene)
            {
                Destroy(gameObject);
                return;
            }
        }
        
        // 모든 씬에서 살아남게 설정
        DontDestroyOnLoad(gameObject);

        string currentScene = SceneManager.GetActiveScene().name;
        shouldKeep = (currentScene == startScene); // 시작씬에서만 유지 시작
        if (!shouldKeep)
        {
            Destroy(gameObject); // 시작씬이 아니면 바로 제거
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string currentScene = scene.name;

        // endScene 도달하면 제거
        if (currentScene == endScene)
        {
            Destroy(gameObject);
        }
    }
}
