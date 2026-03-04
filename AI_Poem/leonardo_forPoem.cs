// FINAL VERSION - cleaned up for SimpleJSON-based Leonardo image upload in Unity

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;
using SimpleJSON;

public class leonardo_forPoem : MonoBehaviour
{
    public string openAiApiKey;
    public string leonardoApiKey;
    private string modelId = "4b2e0f95-f15e-48d8-ada3-c071d6104db8"; // Fairytale style model

    public GameObject resultPanel;
    public GameObject themePanel;
    public RawImage resultImage;

    [Header("Prompt Source")]
    private PoemPromptGenerator poemPromptGenerator;
    public CaptureNUploadManager captureNUploadManager;

    [HideInInspector] public string generatedPrompt = "";

    void Start()
    {
        poemPromptGenerator = GetComponent<PoemPromptGenerator>();

        poemPromptGenerator.OnPoemGenerated += (poem) =>
        {
            if (!string.IsNullOrEmpty(poem))
            {
                string message = InputManager.Instance.GetUserInput();
                string emotion = poemPromptGenerator.selectedEmotion;
                string symbol = poemPromptGenerator.selectedSymbol;

                Debug.Log("✅ 시 생성 완료 → 이미지 생성 시작");
                if (!LoadingManager.Instance.IsLoading())
                {
                    LoadingManager.Instance.SetLoading(true); // 로딩 중이 아니었다면 로딩 시작
                    Debug.Log("로딩 중이 아니었음 → 로딩 시작");
                }
                this.TryStartCoroutine(GeneratePromptAndImage(message, emotion, symbol, poem));
            }
            else
            {
                Debug.LogWarning("⚠️ 시 생성 실패 또는 내용 없음");
            }
        };
            
    }

    IEnumerator GeneratePromptAndImage(string message, string emotion, string symbol, string poem)
    {
        string promptRequest =
            "You are a poetic but visually descriptive image prompt generator for AI-based art creation.\n\n" +
            "Your goal is to create a **fairytale-style**, painterly image prompt based on a Korean poem.\n" +
            "You must first **read and understand the poem's message, symbols, and emotions**, then convert it into a detailed image prompt for an AI image model like Leonardo AI.\n\n" +
            "📌 Important rules you must follow:\n" +
            "1. **The image must have a fairytale-like, dreamlike, or children's storybook style.** Do NOT use realism or photographic style.\n" +
            "2. **The center of the image must remain completely white and empty.** No object, color, or shadow should enter the center. All visual elements must be arranged around the outer frame and softly radiate inward.\n\n" +
            $"User's message: \"{message}\"\n" +
            $"Emotion to express: \"{emotion}\"\n" +
            $"Main symbol: \"{symbol}\"\n" +
            $"Full poem:\n\"\"\"\n{poem}\n\"\"\"\n\n" +
            "Now create a detailed English image prompt that describes:\n" +
            "- The **visual style** (must be fairytale, painterly)\n" +
            "- The **main subject or symbolic objects**\n" +
            "- The **background and setting**\n" +
            "- The **composition** (where elements are placed; avoid center)\n" +
            "- **Lighting and color palette**\n" +
            "- The **emotional tone** reflected in the imagery\n\n" +
            "Return only the final English image prompt. Do not include any explanations, within 1500 characters.";


        ChatRequest requestData = new ChatRequest
        {
            model = "gpt-4",
            temperature = 0.5f,
            max_tokens = 300,
            messages = new ChatMessage[]
            {
                new ChatMessage { role = "system", content = "You are a poetic prompt generator for AI artwork." },
                new ChatMessage { role = "user", content = promptRequest }
            }
        };

        string json = JsonUtility.ToJson(requestData);
        UnityWebRequest request = UnityWebRequest.PostWwwForm("https://api.openai.com/v1/chat/completions", "POST");
        byte[] body = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + openAiApiKey);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("OpenAI 프롬프트 생성 실패: " + request.error);
            yield break;
        }

        generatedPrompt = JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text).choices[0].message.content.Trim();
        Debug.Log("생성된 프롬프트:\n" + generatedPrompt);

        string sourceImageUrl = "https://ltveoyyzdpzngwzihlrt.supabase.co/storage/v1/object/public/prompt/imagePrompt.png";
        yield return this.TryStartCoroutine(UploadImageToLeonardo(sourceImageUrl, (imageId) =>
        {
            if (!string.IsNullOrEmpty(imageId))
                this.TryStartCoroutine(GenerateImage(generatedPrompt, imageId));
            else
                Debug.LogError("imageId를 얻지 못해 이미지 생성을 중단합니다.");
        }));
    }

        // 클래스 내부 어디든(필드들 아래) 넣어라.
    static void LogReq(string tag, UnityWebRequest r)
    {
        Debug.Log($"[{tag}] code={r.responseCode}, result={r.result}, err={r.error}\nbody={r.downloadHandler?.text}");
    }


    // IEnumerator UploadImageToLeonardo(string imageUrl, Action<string> onSuccess)
    // {
    //     string initUrl = "https://cloud.leonardo.ai/api/rest/v1/init-image";
    //     UnityWebRequest initReq = UnityWebRequest.PostWwwForm(initUrl, "POST");
    //     byte[] initBody = Encoding.UTF8.GetBytes("{\"extension\":\"jpg\"}");
    //     initReq.uploadHandler = new UploadHandlerRaw(initBody);
    //     initReq.downloadHandler = new DownloadHandlerBuffer();
    //     initReq.SetRequestHeader("Authorization", "Bearer " + leonardoApiKey);
    //     initReq.SetRequestHeader("Content-Type", "application/json");

    //     yield return initReq.SendWebRequest();

    //     Debug.Log($"[LEO_INIT] code={initReq.responseCode}, err={initReq.error}\nbody={initReq.downloadHandler?.text}");

    //     if (initReq.result != UnityWebRequest.Result.Success)
    //     {
    //         Debug.LogError("Leonardo presigned 요청 실패: " + initReq.downloadHandler.text);
    //         onSuccess(null);
    //         yield break;
    //     }

    //     JSONNode root = JSON.Parse(initReq.downloadHandler.text);
    //     var uploadInfo = root["uploadInitImage"];
    //     string uploadUrl = uploadInfo["url"];
    //     string imageId = uploadInfo["id"];

    //     string rawFieldsString = uploadInfo["fields"];
    //     JSONNode fields = JSON.Parse(rawFieldsString);



    //     UnityWebRequest download = UnityWebRequest.Get(imageUrl);
    //     yield return download.SendWebRequest();

    //     if (download.result != UnityWebRequest.Result.Success)
    //     {
    //         Debug.LogError("이미지 다운로드 실패: " + download.error);
    //         onSuccess(null);
    //         yield break;
    //     }

    //     byte[] imageBytes = download.downloadHandler.data;

    //     WWWForm form = new WWWForm();
    //     form.AddField("key", fields["key"].Value);
    //     form.AddField("bucket", fields["bucket"].Value);
    //     form.AddField("policy", fields["Policy"].Value);
    //     form.AddField("x-amz-algorithm", fields["X-Amz-Algorithm"].Value);
    //     form.AddField("x-amz-credential", fields["X-Amz-Credential"].Value);
    //     form.AddField("x-amz-date", fields["X-Amz-Date"].Value);
    //     form.AddField("x-amz-security-token", fields["X-Amz-Security-Token"].Value);
    //     form.AddField("x-amz-signature", fields["X-Amz-Signature"].Value);
    //     form.AddField("Content-Type", fields["Content-Type"].Value);


    //     form.AddBinaryData("file", imageBytes, "image.jpg", "image/jpeg");

    //     UnityWebRequest upload = UnityWebRequest.Post(uploadUrl, form);
    //     yield return upload.SendWebRequest();

    //     if (upload.result != UnityWebRequest.Result.Success)
    //     {
    //         Debug.LogError("이미지 업로드 실패: " + upload.error);
    //         onSuccess(null);
    //         yield break;
    //     }

    //     Debug.Log("✅ Leonardo 업로드 완료: imageId = " + imageId);
    //     onSuccess(imageId);
    // }

    IEnumerator UploadImageToLeonardo(string imageUrl, Action<string> onSuccess)
{
    // 1) presigned init-image (JSON POST)
    var initReq = new UnityWebRequest("https://cloud.leonardo.ai/api/rest/v1/init-image", "POST");
    var initBody = Encoding.UTF8.GetBytes("{\"extension\":\"png\"}");
    initReq.uploadHandler   = new UploadHandlerRaw(initBody);
    initReq.downloadHandler = new DownloadHandlerBuffer();
    initReq.SetRequestHeader("Authorization", "Bearer " + leonardoApiKey);
    initReq.SetRequestHeader("Content-Type", "application/json");

    yield return initReq.SendWebRequest();
    LogReq("LEO_INIT", initReq);

    if (initReq.result != UnityWebRequest.Result.Success)
    {
        Debug.LogError("init-image 실패 → imageId를 못 받음(키/쿼터/네트워크).");
        onSuccess?.Invoke(null);
        yield break;
    }

    // 2) 응답 파싱
    JSONNode root;
    try { root = SimpleJSON.JSON.Parse(initReq.downloadHandler.text); }
    catch { Debug.LogError("init-image 응답 JSON 파싱 실패"); onSuccess?.Invoke(null); yield break; }

    var uploadInfo = root["uploadInitImage"];
    if (uploadInfo == null || uploadInfo.IsNull) uploadInfo = root["initImage"]; // 혹시 스키마 변경 대비

    string uploadUrl = uploadInfo?["url"]?.Value;
    string imageId   = uploadInfo?["id"]?.Value;

    // fields: 문자열(JSON) 또는 객체로 올 수 있음 → 안전 처리
    JSONNode fieldsNode = uploadInfo["fields"];
    if (fieldsNode != null && fieldsNode.IsString) fieldsNode = JSON.Parse(fieldsNode.Value);

    if (string.IsNullOrEmpty(uploadUrl) || string.IsNullOrEmpty(imageId) || fieldsNode == null || fieldsNode.IsNull)
    {
        Debug.LogError("init-image 응답에 url/id/fields가 없음.");
        onSuccess?.Invoke(null);
        yield break;
    }
    Debug.Log($"[LEO_INIT] imageId={imageId}");

    // 3) 원본(소스) 이미지 다운로드
    var dl = UnityWebRequest.Get(imageUrl);
    yield return dl.SendWebRequest();
    LogReq("SRC_DOWNLOAD", dl);

    if (dl.result != UnityWebRequest.Result.Success ||
        dl.responseCode != 200 ||
        dl.downloadHandler.data == null ||
        dl.downloadHandler.data.Length == 0)
    {
        Debug.LogError("소스 이미지 다운로드 실패(권한/경로/signed URL 확인).");
        onSuccess?.Invoke(null);
        yield break;
    }
    var imageBytes = dl.downloadHandler.data;

    // 4) S3 업로드: presigned fields를 '그대로' 주입
    //    - key 정제(선행 '/' 제거, trim)
    string rawKey = fieldsNode["key"]?.Value ?? "";
    string useKey = rawKey.Trim();
    if (useKey.StartsWith("/")) useKey = useKey.Substring(1);
    Debug.Log($"[LEO] uploadUrl={uploadUrl}");
    Debug.Log($"[LEO] rawKey='{rawKey}' → useKey='{useKey}'");

    var form = new WWWForm();

    // key는 항상 첫 번째에, 정제된 값으로
    form.AddField("key", useKey);

    // 나머지 필드들은 응답 키/값을 '그대로'(대소문자 보존). bucket은 보통 제외(호스트에 포함)
    foreach (var kv in fieldsNode)
    {
        var k = kv.Key;
        if (string.Equals(k, "key", StringComparison.OrdinalIgnoreCase)) continue;
        if (string.Equals(k, "bucket", StringComparison.OrdinalIgnoreCase)) continue; // 안전하게 제외
        form.AddField(k, kv.Value.Value);
    }

    // 파일 파트(콘텐츠 타입은 fields와 일치: 대부분 image/jpeg)
    form.AddBinaryData("file", imageBytes, "image.png", "image/jpeg");

    var upload = UnityWebRequest.Post(uploadUrl, form);
    upload.chunkedTransfer = false;   // S3 안정화
    upload.useHttpContinue = false;   // S3 안정화

    yield return upload.SendWebRequest();
    LogReq("LEO_S3_UPLOAD", upload);

    if (upload.result != UnityWebRequest.Result.Success)
    {
        Debug.LogError("초기 이미지 S3 업로드 실패(정책/서명/옵션). imageId는 받았지만 업로드 단계에서 중단됨.");
        onSuccess?.Invoke(null);
        yield break;
    }

    Debug.Log("✅ Leonardo 업로드 완료: imageId = " + imageId);
    onSuccess?.Invoke(imageId);
}


    IEnumerator GenerateImage(string prompt, string imageId)
    {
        string postUrl = "https://cloud.leonardo.ai/api/rest/v1/generations";

        string jsonBody = $@"{{
            ""alchemy"": true,
            ""height"": 768,
            ""width"": 576,
            ""modelId"": ""{modelId}"",
            ""num_images"": 1,
            ""prompt"": ""{EscapeForJson(prompt)}"",
            ""imagePrompts"": [""{imageId}""]
        }}";

        UnityWebRequest postRequest = UnityWebRequest.PostWwwForm(postUrl, "POST");
        postRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
        postRequest.downloadHandler = new DownloadHandlerBuffer();
        postRequest.SetRequestHeader("Authorization", "Bearer " + leonardoApiKey);
        postRequest.SetRequestHeader("accept", "application/json");
        postRequest.SetRequestHeader("content-type", "application/json");

        yield return postRequest.SendWebRequest();

        if (postRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("POST 실패: " + postRequest.downloadHandler.text);
            yield break;
        }

        string generationId = ExtractGenerationId(postRequest.downloadHandler.text);
        string getUrl = $"https://cloud.leonardo.ai/api/rest/v1/generations/{generationId}";

        string imageUrl = null;
        int maxAttempts = 30; // 최대 30초까지 대기
        float pollingInterval = 1f;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            UnityWebRequest getRequest = UnityWebRequest.Get(getUrl);
            getRequest.SetRequestHeader("Authorization", "Bearer " + leonardoApiKey);
            getRequest.SetRequestHeader("accept", "application/json");

            yield return getRequest.SendWebRequest();

            if (getRequest.result == UnityWebRequest.Result.Success)
            {
                imageUrl = ExtractImageUrl(getRequest.downloadHandler.text);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    Debug.Log("✅ 이미지 생성 완료: " + imageUrl);
                    break;
                }
            }
            else
            {
                Debug.LogWarning("GET 요청 실패, 다시 시도 중: " + getRequest.error);
            }

            yield return new WaitForSeconds(pollingInterval);
        }

        if (string.IsNullOrEmpty(imageUrl))
        {
            Debug.LogError("⚠️ 이미지 생성 실패 또는 제한 시간 초과");
            LoadingManager.Instance.SetLoading(false); // 로딩 끝
            Debug.Log("로딩 끝");
            yield break;
        }
        else
        {
            if (resultPanel != null)
                resultPanel.SetActive(true);

            if (themePanel != null)
                themePanel.SetActive(false);

            this.TryStartCoroutine(LoadImage(imageUrl));

            captureNUploadManager.StartCountdown();

        }

        LoadingManager.Instance.SetLoading(false); // 로딩 끝
        Debug.Log("로딩 끝");

        
    }


    IEnumerator LoadImage(string imageUrl)
    {
        UnityWebRequest textureRequest = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return textureRequest.SendWebRequest();

        if (textureRequest.result == UnityWebRequest.Result.Success)
        {
            resultImage.texture = DownloadHandlerTexture.GetContent(textureRequest);
        }
        else
        {
            Debug.LogError("이미지 불러오기 실패: " + textureRequest.error);
        }
    }

    string EscapeForJson(string s)
    {
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");
    }

    string ExtractGenerationId(string json) => JsonUtility.FromJson<GenerationResponse>(json)?.sdGenerationJob?.generationId;


    string ExtractImageUrl(string json)
    {
        try
        {
            var data = JsonUtility.FromJson<LeonardoStatusResponse>(json);
            if (data != null && data.generations_by_pk != null && 
                data.generations_by_pk.generated_images != null &&
                data.generations_by_pk.generated_images.Length > 0)
            {
                return data.generations_by_pk.generated_images[0].url;
            }
            else
            {
                Debug.LogWarning("⛔️ 이미지 URL 없음 — 아직 생성 안 됐거나 결과 비어 있음");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("❌ 이미지 URL 추출 중 예외 발생: " + e.Message);
            return null;
        }
    }
}