using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;

public class CaptureNUploadManager : MonoBehaviour
{
    [Header("캡처 대상")]
    public RectTransform resultPanel;          // 기본 캡처 영역(프레임)
    public RectTransform captureArea;          // ⛳ resultPanel 내부에서 실제로 캡처할 '일부' 영역(자식 RectTransform 권장)
    public Canvas uiCanvas;

    [Header("크롭 패딩(px)")]
    public float paddingLeft = 0f;
    public float paddingRight = 0f;
    public float paddingTop = 0f;
    public float paddingBottom = 0f;

    [Header("연결된 데이터")]
    public PoemPromptGenerator poemPromptGenerator;
    public leonardo_forPoem leonardoPromptSource;

    [Header("작품 타입")]
    public string type = "poem";

    [Header("UI 캡처용")]
    public Camera uiCamera;
    public RenderTexture uiRenderTexture;

    public void StartCountdown()
    {
        this.TryStartCoroutine(TryCaptureIfVisible());
    }

    IEnumerator TryCaptureIfVisible()
    {
        if (resultPanel != null && resultPanel.gameObject.activeInHierarchy)
        {
            Debug.Log("✅ resultPanel 활성 → 3초 대기 후 캡처");
            yield return new WaitForSeconds(3f);
            yield return CaptureAndUploadCoroutine();
        }
        else
        {
            Debug.Log("⏹️ resultPanel 비활성 → 캡처 스킵");
        }
    }

    IEnumerator CaptureAndUploadCoroutine()
    {
        if (uiCanvas == null || uiCamera == null || uiRenderTexture == null || resultPanel == null)
        {
            Debug.LogError("❌ 참조 누락(uiCanvas/uiCamera/uiRenderTexture/resultPanel).");
            yield break;
        }

        // 1) 캔버스 카메라 교체 + RT 바인드
        Camera originalCamera = uiCanvas.worldCamera;
        uiCanvas.worldCamera = uiCamera;
        uiCamera.targetTexture = uiRenderTexture;

        // 2) 렌더 완료 시점
        yield return new WaitForEndOfFrame();
        uiCamera.Render();

        // 3) RT 활성화
        RenderTexture prevActive = RenderTexture.active;
        RenderTexture.active = uiRenderTexture;

        // 4) 크롭 대상 선택: captureArea 있으면 그걸, 없으면 resultPanel
        RectTransform targetArea = captureArea != null ? captureArea : resultPanel;

        // 5) 대상 영역(Viewport→RT 픽셀) 계산
        Rect rtCrop = GetRtPixelRectOf(targetArea, uiCamera, uiRenderTexture);

        // 6) 패딩 적용(px, RT 좌표계: 좌하 원점)
        if (paddingLeft != 0f || paddingRight != 0f || paddingTop != 0f || paddingBottom != 0f)
        {
            rtCrop = new Rect(
                rtCrop.x + paddingLeft,
                rtCrop.y + paddingBottom,
                rtCrop.width - (paddingLeft + paddingRight),
                rtCrop.height - (paddingTop + paddingBottom)
            );
        }

        // 7) resultPanel로 한 번 더 클리핑(원하는 경우) — captureArea가 resultPanel 밖으로 나가 있어도 안전
        Rect panelCrop = GetRtPixelRectOf(resultPanel, uiCamera, uiRenderTexture);
        rtCrop = IntersectRects(rtCrop, panelCrop);

        // 경계 보정
        rtCrop = ClampRectToRT(rtCrop, uiRenderTexture.width, uiRenderTexture.height);

        int cropW = Mathf.Max(1, Mathf.RoundToInt(rtCrop.width));
        int cropH = Mathf.Max(1, Mathf.RoundToInt(rtCrop.height));
        if (cropW <= 1 || cropH <= 1)
        {
            Debug.LogError($"❌ 크롭 영역이 너무 작거나 유효하지 않음: {rtCrop}");
            // 복구
            RenderTexture.active = prevActive;
            uiCamera.targetTexture = null;
            uiCanvas.worldCamera = originalCamera;
            yield break;
        }

        // 8) 크롭 읽기
        Texture2D screenshot = new Texture2D(cropW, cropH, TextureFormat.RGB24, false);
        screenshot.ReadPixels(rtCrop, 0, 0);
        screenshot.Apply();

        // 9) 복구
        RenderTexture.active = prevActive;
        uiCamera.targetTexture = null;
        uiCanvas.worldCamera = originalCamera;

        // 10) 파일 저장(JPG)
        byte[] jpgBytes = screenshot.EncodeToJPG();
        string fileName = $"{Guid.NewGuid()}.jpg";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(filePath, jpgBytes);
        Debug.Log($"📸 [부분 크롭] 저장: {filePath} (w:{cropW}, h:{cropH})");

        // 11) 메타데이터 수집
        string title = poemPromptGenerator != null ? poemPromptGenerator.message : "";
        string prompt = leonardoPromptSource != null ? leonardoPromptSource.generatedPrompt : "";
        string emotion = poemPromptGenerator != null ? poemPromptGenerator.selectedEmotion : "";
        string symbol = poemPromptGenerator != null ? poemPromptGenerator.selectedSymbol : "";
        var tags = new List<string>();
        if (!string.IsNullOrEmpty(emotion)) tags.Add(emotion);
        if (!string.IsNullOrEmpty(symbol))  tags.Add(symbol);

        // 12) 업로드
        var uploader = SupabaseUploader.Instance;
        yield return uploader.Initialize();

        var uploadTask = uploader.UploadFile(jpgBytes, fileName, "image/jpeg");
        while (!uploadTask.IsCompleted) yield return null;

        string uploadedPath = uploadTask.Result;
        if (string.IsNullOrEmpty(uploadedPath))
        {
            Debug.LogError("❌ 업로드 실패");
            yield break;
        }
        Debug.Log($"☁️ 업로드 완료: {uploadedPath}");

        // 13) 메타데이터 업로드
        var metadata = new ArtworkMetadata
        {
            id = Path.GetFileNameWithoutExtension(fileName),
            title = title,
            prompt = prompt,
            tags = tags,
            type = type,
            file_name = fileName,
            date = DateTime.UtcNow.AddHours(9).ToString("o")
        };
        yield return SupabaseHelper.UploadMetadata(metadata);
        Debug.Log("✅ 메타데이터 업로드 완료");

        // 14) 로컬 정리
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("🧹 로컬 파일 삭제: " + filePath);
        }
    }

    /// <summary>
    /// target(RectTransform)의 월드 코너를 카메라 Viewport(0~1)로 투영 → RT 픽셀 Rect로 변환
    /// </summary>
    private Rect GetRtPixelRectOf(RectTransform target, Camera cam, RenderTexture rt)
    {
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners); // 좌하, 좌상, 우상, 우하

        Vector2 minV = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 maxV = new Vector2(float.MinValue, float.MinValue);

        for (int i = 0; i < 4; i++)
        {
            Vector3 vp = cam.WorldToViewportPoint(corners[i]); // (x,y:0~1, z:depth)
            // z<0(카메라 뒤)인 경우 뷰포트 투영이 뒤집힐 수 있음 → 최소/최대 클램프 전에 간단히 0~1로 제한
            Vector2 v = new Vector2(Mathf.Clamp01(vp.x), Mathf.Clamp01(vp.y));
            minV = Vector2.Min(minV, v);
            maxV = Vector2.Max(maxV, v);
        }

        float x = Mathf.Floor(minV.x * rt.width);
        float y = Mathf.Floor(minV.y * rt.height);
        float w = Mathf.Ceil((maxV.x - minV.x) * rt.width);
        float h = Mathf.Ceil((maxV.y - minV.y) * rt.height);

        return new Rect(x, y, w, h);
    }

    /// <summary> 두 Rect의 교집합. 비어 있으면 width/height=0 </summary>
    private Rect IntersectRects(Rect a, Rect b)
    {
        float xMin = Mathf.Max(a.xMin, b.xMin);
        float yMin = Mathf.Max(a.yMin, b.yMin);
        float xMax = Mathf.Min(a.xMax, b.xMax);
        float yMax = Mathf.Min(a.yMax, b.yMax);

        if (xMax <= xMin || yMax <= yMin) return new Rect(0, 0, 0, 0);
        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    /// <summary> Rect가 RT 범위 안에 들도록 클램프 </summary>
    private Rect ClampRectToRT(Rect r, int rtW, int rtH)
    {
        float x = Mathf.Clamp(r.x, 0, rtW);
        float y = Mathf.Clamp(r.y, 0, rtH);
        float maxW = Mathf.Max(0, rtW - x);
        float maxH = Mathf.Max(0, rtH - y);
        float w = Mathf.Clamp(r.width, 0, maxW);
        float h = Mathf.Clamp(r.height, 0, maxH);
        return new Rect(x, y, w, h);
    }
}
