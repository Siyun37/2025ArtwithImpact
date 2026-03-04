using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class ArchiveLoader : MonoBehaviour
{
    void Start()
    {
        this.TryStartCoroutine(FetchAllMetadata());
    }

    IEnumerator FetchAllMetadata()
    {
        string url = "https://[project-id].supabase.co/rest/v1/metadata?select=*";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", SupabaseConfig.ANON_KEY);
        request.SetRequestHeader("Authorization", "Bearer " + SupabaseConfig.ANON_KEY);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ 메타데이터 로딩 실패: " + request.error);
            yield break;
        }

        var list = JsonHelper.FromJsonList<ArtworkMetadata>(request.downloadHandler.text);
        ArchiveManager.Instance.allMetadata = list;
    }
}
