using UnityEngine;
using System.Collections;

public class BGMTerminator : MonoBehaviour
{
    public float delayBeforeStop = 10f; // 예: 10초 후 제거
    public string bgmObjectName = "BGM"; // BGM GameObject 이름
    private float fadeOutDuration = 2f;

    void Start()
    {
        Invoke("BeginFadeOut", delayBeforeStop);
    }

    void BeginFadeOut()
    {
        GameObject bgm = GameObject.Find(bgmObjectName);
        if (bgm != null)
        {
            AudioSource source = bgm.GetComponent<AudioSource>();
            if (source != null)
            {
                StartCoroutine(FadeOutAndDestroy(source, bgm));
            }
            else
            {
                Destroy(bgm); // fallback: AudioSource 없으면 바로 제거
            }
        }
    }

    IEnumerator FadeOutAndDestroy(AudioSource source, GameObject target)
    {
        float startVolume = source.volume;
        float time = 0f;

        while (time < fadeOutDuration)
        {
            time += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, time / fadeOutDuration);
            yield return null;
        }

        source.Stop();
        Destroy(target);
    }
}
