using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.InputSystem;
using System.Collections;

public class Ach_SpaceToSphere : MonoBehaviour
{
    [Header("🖼️ 텍스처 적용할 렌더러")]
    public Renderer targetRenderer;

    [Header("▶️ 전환할 씬 이름")]
    public string nextSceneName = "Ai_Skybox2";

    [Header("⌛ 로딩 머티리얼 (선택)")]
    public Material loadingMaterial;

    [Header("🖱️ 클릭하면 씬 전환할지 여부")]
    public bool enableClickToChangeScene = true;   // 🔹 여기서 ON/OFF

    private bool isTextureReady = false;

    private Vector2 mouseDownPos;
    private float dragThreshold = 10f; // 픽셀 단위 거리 차이로 클릭/드래그 구분

    void Start()
    {
        string url = SkyboxResultStorage.generatedSkyboxUrl;

        if (targetRenderer != null && loadingMaterial != null)
            targetRenderer.material = loadingMaterial;

        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError("❌ SkyboxResultStorage.generatedSkyboxUrl 비어 있음");
            return;
        }

        this.TryStartCoroutine(DownloadAndApplyTexture(url));
    }

    void OnMouseDown()
    {
        // 🔹 클릭 전환 비활성화면 아예 처리 안 함
        if (!enableClickToChangeScene)
            return;

        mouseDownPos = Mouse.current.position.ReadValue();
    }

    void OnMouseUp()
    {
        // 🔹 클릭 전환 비활성화면 아예 처리 안 함
        if (!enableClickToChangeScene)
        {
            // 필요하다면 디버그 로그도 여기에
            // Debug.Log("🔇 클릭 씬 전환 기능이 비활성화 상태입니다.");
            return;
        }

        Vector2 mouseUpPos = Mouse.current.position.ReadValue();
        float distance = Vector2.Distance(mouseDownPos, mouseUpPos);

        if (distance <= dragThreshold)
        {
            // ✅ 클릭으로 인식
            if (!isTextureReady)
            {
                Debug.Log("⏳ 텍스처가 아직 적용되지 않아 클릭 무시됨.");
                return;
            }

            Debug.Log("✅ 클릭됨 → 씬 전환");
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.Log("🎯 드래그 동작으로 판단됨 → 씬 전환 안 함");
        }
    }

    IEnumerator DownloadAndApplyTexture(string imageUrl)
    {
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"❌ 텍스처 다운로드 실패: {request.error}");
            yield break;
        }

        Texture2D downloadedTex = DownloadHandlerTexture.GetContent(request);
        downloadedTex.filterMode = FilterMode.Bilinear;

        if (targetRenderer != null)
        {
            Material newMat = new Material(targetRenderer.sharedMaterial);
            newMat.mainTexture = downloadedTex;
            targetRenderer.material = newMat;

            isTextureReady = true;
            Debug.Log("✅ 텍스처가 오브젝트에 성공적으로 적용되었습니다.");
        }
    }
}
