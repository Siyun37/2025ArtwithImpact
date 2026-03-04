using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Linq;

public class SearchController : MonoBehaviour
{
    [Header("🔍 UI 연결")]
    public TMP_InputField searchInput;

    [Header("📋 리스트 출력")]
    public GameObject itemPrefab;           // Ach_list 프리팹
    public Transform contentParent;         // Scroll View의 Content 오브젝트
    public GameObject noResultText;         // "결과 없음" 오브젝트

    void Start()
    {
        searchInput.onSubmit.AddListener(_ => OnSearchClicked());

        StartCoroutine(FetchAllMetadata()); // 초기 전체 불러오기
    }

    void OnSearchClicked()
    {
        string keyword = searchInput.text.Trim();
        StartCoroutine(FetchAllMetadata(keyword));
    }

    IEnumerator FetchAllMetadata(string keyword = "")
    {
        // 1. 기존 리스트 삭제
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        if (noResultText != null)
        {
            noResultText.SetActive(false);
        }

        // 2. Supabase 요청
        string url = "https://ltveoyyzdpzngwzihlrt.supabase.co/rest/v1/artworks?select=*";
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

        // 3. 필터링 (title에 keyword 포함)
        if (!string.IsNullOrEmpty(keyword))
        {
            list = list
                .Where(item => 
                    (!string.IsNullOrEmpty(item.title) && item.title.Contains(keyword))).ToList();
        }

        // 4. 날짜 기준 최신순 정렬
        list = list.OrderByDescending(item => item.date).ToList();

        if (list.Count == 0)
        {
            if (noResultText != null)
            {
                noResultText.SetActive(true);
            }

            yield break;
        }

        // 5. 리스트 출력
        for (int i = 0; i < list.Count; i++)
        {
            GameObject go = Instantiate(itemPrefab, contentParent);
            go.GetComponent<TitleButton>().Init(list[i], i + 1);
        }
    }
}
