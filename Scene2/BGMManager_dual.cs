using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BGMManager_dual : MonoBehaviour
{
    [Header("🎧 BGM 소스들")]
    public AudioSource[] bgmSources; // 두 개 이상 허용
    public float fadeInDuration = 3f;

    [Header("🧭 유지할 씬 설정")]
    public string startScene;
    public string endScene; // 이 씬 도달 시 파괴

    [Header("🔊 최종 볼륨")]
    public float targetVolume = 1f;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene != startScene)
        {
            Destroy(gameObject); // 시작 씬이 아니면 제거
            return;
        }

        foreach (var source in bgmSources)
        {
            source.volume = 0f;
            source.loop = true;
            source.Play();
        }

        StartCoroutine(FadeInAll());
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
        if (scene.name == endScene)
        {
            Destroy(gameObject); // 🎯 페이드아웃은 외부에서 처리됨
        }
    }

    IEnumerator FadeInAll()
    {
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);
            foreach (var source in bgmSources)
            {
                source.volume = targetVolume * t;
            }
            yield return null;
        }

        foreach (var source in bgmSources)
        {
            source.volume = targetVolume;
        }
    }
}
