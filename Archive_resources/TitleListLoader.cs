using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Linq;

[DisallowMultipleComponent]
public class TitleListLoader : MonoBehaviour
{
    public GameObject itemPrefab;
    public Transform contentParent;

    private static bool _alreadyRendered = false; // 씬에서 한 번만 그리기(원치 않으면 제거)
    private bool _loading = false;

    void Start()
    {
        Debug.Log("[TitleListLoader] Start() 호출");
        if (_alreadyRendered) { Debug.Log("[TitleListLoader] 이미 렌더됨, 리턴"); return; }
        StartCoroutine(FetchAllMetadata());
    }

    IEnumerator FetchAllMetadata()
    {
        if (_loading) { Debug.Log("[TitleListLoader] 로딩 중 재진입 차단"); yield break; }
        _loading = true;

        string url = "https://ltveoyyzdpzngwzihlrt.supabase.co/rest/v1/artworks?select=*&order=date.desc";
        var request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", SupabaseConfig.ANON_KEY);
        request.SetRequestHeader("Authorization", "Bearer " + SupabaseConfig.ANON_KEY);

        Debug.Log("[TitleListLoader] 요청 시작");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ 메타데이터 로딩 실패: " + request.error);
            _loading = false;
            yield break;
        }

        var list = JsonHelper.FromJsonList<ArtworkMetadata>(request.downloadHandler.text);
        int distinctIds = list.Select(x => x.id).Distinct().Count();
        Debug.Log($"[TitleListLoader] 받은 개수:{list.Count}, 고유 id:{distinctIds}, 기존 child:{contentParent.childCount}");

        // (안전) id 기준 중복 제거
        list = list.GroupBy(x => x.id).Select(g => g.First()).ToList();
        Debug.Log($"📌 원본 JSON: {request.downloadHandler.text}");
        Debug.Log($"📌 파싱된 개수: {list.Count}");

        // 중복 호출 대비: 기존 아이템 제거
        ClearContent();

        for (int i = 0; i < list.Count; i++)
        {
            var go = Instantiate(itemPrefab, contentParent);
            go.GetComponent<TitleButton>()?.Init(list[i], i + 1);
        }

        Debug.Log($"[TitleListLoader] 생성 완료, 최종 child:{contentParent.childCount}");
        _alreadyRendered = true;   // 씬에서 한 번만
        _loading = false;
        request.Dispose();
    }

    void ClearContent()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);
    }
}
