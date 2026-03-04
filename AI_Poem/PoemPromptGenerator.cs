using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;

public class PoemPromptGenerator : MonoBehaviour
{
    public string apiKey; // OpenAI API Key

    public TMP_InputField userInput;
    public Button generateOptionsButton;



    public GameObject emotionPanel; // Emotion Panel 오브젝트
    public Button[] optionButtons; // 8개의 버튼으로 가정 -> Emotions

    [HideInInspector] public string selectedEmotion;


    public GameObject symbolPanel;
    //public Button[] symbolButtons;
    public BookSymbolObject[] bookSymbolObjects;
    public TMP_InputField customSymbolInput;
    public Button confirmSymbolButton;


    [HideInInspector] public string selectedSymbol;
    


    public GameObject themePanel;
    public TMP_Text themeText;
    public Button regenerateButton;
    public Button finishButton;
    [HideInInspector] public string theme;


    public GameObject resultPanel;
    public TMP_Text resultTitleText;
    public TMP_Text resultBodyText;
    [HideInInspector] public string finalPoem;
    public event Action<string> OnPoemGenerated;



    [HideInInspector] public string message;

    public CaptureNUploadManager captureScript;

    void Start()
    {


        // InputManager에서 메시지 받아오기
        message = InputManager.Instance.GetUserInput();
        if (!string.IsNullOrEmpty(message))
        {
            Debug.Log("받은 메시지: " + message);

            // InputField에 자동으로 입력
            if (userInput != null)
                userInput.text = message;

            // 감정 키워드 바로 생성
            this.TryStartCoroutine(GetEmotionKeywords(message));
        }
        else
        {
            Debug.LogWarning("InputManager에서 메시지를 불러올 수 없습니다.");
        }

        // Emotion Panel 처음엔 비활성화
        if (emotionPanel != null)
            emotionPanel.SetActive(false);

        // 버튼 수동 클릭도 가능하게
        generateOptionsButton.onClick.AddListener(() =>
        {
            string input = userInput.text;
            if (!string.IsNullOrEmpty(input))
            {
                Debug.Log("사용자 입력 감정 키워드 생성 시작");
                this.TryStartCoroutine(GetEmotionKeywords(input));
            }
        });

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int capturedIndex = i;
            optionButtons[i].onClick.AddListener(() => SelectEmotionFromButton(capturedIndex));
        }

        // for (int i = 0; i < symbolButtons.Length; i++)
        // {
        //     int capturedIndex = i;
        //     symbolButtons[i].onClick.AddListener(() => SelectSymbolFromButton(capturedIndex));
        // }

        confirmSymbolButton.onClick.AddListener(ConfirmCustomSymbol);

        regenerateButton.onClick.AddListener(() =>
        {

            if (!string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(selectedEmotion) && !string.IsNullOrEmpty(selectedSymbol))
            {
                Debug.Log("주제문 재생성 시작");
                this.TryStartCoroutine(GeneratePoemTheme(message, selectedEmotion, selectedSymbol));
            }
            else
            {
                Debug.LogWarning("메시지, 정서, 상징 중 하나 이상이 비어 있어 주제문을 재생성할 수 없습니다.");
            }
        });

        finishButton.onClick.AddListener(() =>
        {
            theme = themeText.text;

            if (!string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(selectedEmotion) &&
                !string.IsNullOrEmpty(selectedSymbol) && !string.IsNullOrEmpty(theme))
            {
                Debug.Log("시 생성 시작");
                Debug.Log("로딩 시작");

                if (themePanel != null)
                    themePanel.SetActive(false);
                LoadingManager.Instance.SetLoading(true); // 로딩 시작
                this.TryStartCoroutine(GenerateFinalPoem(message, selectedEmotion, selectedSymbol, theme));
                captureScript.StartCountdown();
            }
            else
            {
                Debug.LogWarning("정보 부족: 메시지 / 정서 / 상징 / 주제문");
            }
        });
    }


    IEnumerator GetEmotionKeywords(string message)
    {

        LoadingManager.Instance?.SetLoading(true); // 로딩 시작

        string systemPrompt =
        "You are an AI assistant that helps generate creative poetry based on user input.\n\n" +
        "Project context:\n" +
        "- The user provides a message on the theme: \"Things we must not forget.\"\n" +
        "- Based on this message, the user will go through the following steps:\n" +
        "  1. Choose one main emotion they want to express in the poem.\n" +
        "  2. Choose one symbolic image related to that emotion.\n" +
        "  3. Finally, a poem will be generated using the input message, the chosen emotion, and the chosen symbol.\n\n" +
        "Your current task:\n" +
        $"- You are given the user's message:\n\"{message}\"\n\n" +
        "- Based on this message, suggest 8 different emotions that could be expressed in a poem inspired by this message.\n" +
        "- Each emotion should be expressed in a single Korean word.\n" +
        "- The emotions should reflect different possible emotional perspectives — avoid repetition or overly similar meanings.\n" +
        "- Only return the 8 words in korean, each on a new line. No explanations.\n\n" +
        "Example output:\n감사\n분노\n그리움\n두려움\n...";

        ChatRequest requestData = new ChatRequest
        {
            model = "gpt-4",
            temperature = 0.8f,
            max_tokens = 200,
            messages = new ChatMessage[]
            {
                new ChatMessage { role = "system", content = "You are an AI assistant for creating poetry through emotions and symbolism." },
                new ChatMessage { role = "user", content = systemPrompt }
            }
        };

        string jsonData = JsonUtility.ToJson(requestData);
        UnityWebRequest request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("GPT API request failed: " + request.error);
            LoadingManager.Instance?.SetLoading(false); // 로딩 끝
            yield break;
        }

        ChatResponse response = JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);
        string content = response.choices[0].message.content;
        Debug.Log("GPT Emotion Keywords Response:\n" + content);

        string[] lines = content.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // 키워드 할당
        for (int i = 0; i < optionButtons.Length && i < lines.Length; i++)
        {
            optionButtons[i].GetComponentInChildren<TMP_Text>().text = lines[i].Trim();
        }

        // Emotion Panel 표시
        if (emotionPanel != null)
            emotionPanel.SetActive(true);

        LoadingManager.Instance?.SetLoading(false); // 로딩 끝

    }

    public string GetselectedEmotion()
    {
        return selectedEmotion;
    }

    public void SelectEmotionFromButton(int index)
    {
        if (index >= 0 && index < optionButtons.Length)
        {
            selectedEmotion = optionButtons[index].GetComponentInChildren<TMP_Text>().text;
            Debug.Log("선택된 정서: " + selectedEmotion);


            this.TryStartCoroutine(GetSymbolKeywords(InputManager.Instance.GetUserInput(), selectedEmotion));
        }

        if (emotionPanel != null)
            emotionPanel.SetActive(false);
    }

    IEnumerator GetSymbolKeywords(string message, string emotion)
    {
        LoadingManager.Instance?.SetLoading(true); // 로딩 시작

        string prompt =
            "You are a creative assistant that suggests symbolic imagery for poetry.\n\n" +
            $"The user provided the following message:\n\"{message}\"\n\n" +
            $"The user selected the main emotion:\n\"{emotion}\"\n\n" +
            "Your task is to suggest 7 different symbolic images that relate to both the message and the chosen emotion.\n\n" +
            "Guidelines:\n" +
            "- Each symbol should be a single word or short phrase in Korean.\n" +
            "- The 7 symbols should each convey a unique idea or metaphor — avoid redundancy.\n" +
            "- Symbols should evoke feeling, atmosphere, or metaphor rather than literal meaning.\n" +
            "- Do not include any explanation, numbering, or extra text.\n" +
            "- Return exactly 7 symbolic expressions, one per line, in Korean.\n\n" +
            "Example:\n달빛\n거울\n잿빛 구름\n…";

        ChatRequest requestData = new ChatRequest
        {
            model = "gpt-4",
            temperature = 0.9f,
            max_tokens = 150,
            messages = new ChatMessage[]
            {
                new ChatMessage { role = "system", content = "You are a creative assistant that helps generate symbolic imagery for poetry based on emotion and message." },
                new ChatMessage { role = "user", content = prompt }
            }
        };

        string jsonData = JsonUtility.ToJson(requestData);
        UnityWebRequest request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("GPT 상징 키워드 생성 실패: " + request.error);
            LoadingManager.Instance?.SetLoading(false); // 로딩 끝
            yield break;
        }

        ChatResponse response = JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);
        string content = response.choices[0].message.content;
        Debug.Log("GPT Symbol Keywords Response:\n" + content);

        string[] lines = content.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // for (int i = 0; i < symbolButtons.Length && i < lines.Length; i++)
        // {
        //     symbolButtons[i].GetComponentInChildren<TMP_Text>().text = lines[i].Trim();
        // }

        // symbolPanel.SetActive(true);
        // LoadingManager.Instance?.SetLoading(false); // 로딩 끝
        // ✅ 여기에서 책 오브젝트 텍스트에 할당
        for (int i = 0; i < bookSymbolObjects.Length && i < lines.Length; i++)
        {
            if (bookSymbolObjects[i] != null && bookSymbolObjects[i].linkedSymbolText != null)
            {
                bookSymbolObjects[i].linkedSymbolText.text = lines[i].Trim();
            }
        }
        LoadingManager.Instance?.SetLoading(false); // 로딩 끝
    }

    public void SelectSymbolFromButton(int index)
    {
        // if (index >= 0 && index < symbolButtons.Length)
        // {
        //     selectedSymbol = symbolButtons[index].GetComponentInChildren<TMP_Text>().text;
        //     // Debug.Log("선택된 상징: " + selectedSymbol);

        //     ConfirmCustomSymbol();
        // }
    }

    public void ConfirmCustomSymbol()
    {
        string custom = customSymbolInput.text;

        if (!string.IsNullOrEmpty(custom))
        {
            selectedSymbol = custom;
            Debug.Log("사용자 입력 상징 선택됨: " + selectedSymbol);
        }
        else if (!string.IsNullOrEmpty(selectedSymbol))
        {
            Debug.Log("버튼으로 상징 선택됨: " + selectedSymbol);
        }
        else
        {
            Debug.Log("상징이 선택되지 않았습니다.");
            return;
        }

        if (!string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(selectedEmotion))
        {
            if (symbolPanel != null)
                symbolPanel.SetActive(false);

            Debug.Log("정서 + 상징 기반 주제문 생성 시작");
            this.TryStartCoroutine(GeneratePoemTheme(message, selectedEmotion, selectedSymbol));
        }
        else
        {
            Debug.LogWarning("메시지 또는 정서가 비어 있음");
        }
    }

    IEnumerator GeneratePoemTheme(string message, string emotion, string symbol)
    {
        LoadingManager.Instance?.SetLoading(true); // 로딩 시작

        string prompt =
            "You are a Korean poetry assistant that creates the **main theme or concept** of a poem.\n\n" +
            $"User message: \"{message}\"\n" +
            $"Chosen emotion: \"{emotion}\"\n" +
            $"Symbolic image: \"{symbol}\"\n\n" +
            "Based on this information, generate a poetic and abstract theme (in Korean) that could guide the creation of a short poem.\n" +
            "The theme should:\n" +
            "- Be metaphorical, conceptual, or symbolic.\n" +
            "- Reflect the emotion and symbol deeply.\n" +
            "- Be one short Korean sentence (max 25 words).\n" +
            "- Do not include explanations, labels, or headers.\n\n" +
            "Respond with only the sentence.";

        ChatRequest requestData = new ChatRequest
        {
            model = "gpt-4",
            temperature = 0.7f,
            max_tokens = 150,
            messages = new ChatMessage[]
            {
                new ChatMessage { role = "system", content = "You are a Korean poetry assistant that generates themes for symbolic and emotional poems." },
                new ChatMessage { role = "user", content = prompt }
            }
        };

        string jsonData = JsonUtility.ToJson(requestData);
        UnityWebRequest request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("GPT 주제문 생성 실패: " + request.error);
            LoadingManager.Instance?.SetLoading(false); // 로딩 끝
            yield break;
        }

        ChatResponse response = JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);
        theme = response.choices[0].message.content.Trim();

        Debug.Log("생성된 시의 주제문: " + theme);

        if (themeText != null)
            themeText.text = theme;

        if (themePanel != null)
            themePanel.SetActive(true);

        LoadingManager.Instance?.SetLoading(false); // 로딩 끝

    }
    
    IEnumerator GenerateFinalPoem(string message, string emotion, string symbol, string theme)
    {
        string prompt =
            "You are a Korean poet AI. Based on the user's message, chosen emotion, symbolic image, and poetic theme, write a Korean poem with a suitable title.\n\n" +
            $"User message: \"{message}\"\n" +
            $"Emotion: \"{emotion}\"\n" +
            $"Symbolic image: \"{symbol}\"\n" +
            $"Theme: \"{theme}\"\n\n" +
            "Instructions:\n" +
            "- Write a short and meaningful Korean title (max 15 characters) on the **first line**.\n" +
            "- Write 4–6 lines of a poetic body under the title.\n" +
            "- Make the poem reflect the theme and emotion symbolically.\n" +
            "- Return only the poem (title + body). No labels, headers, or explanations.";


        ChatRequest requestData = new ChatRequest
        {
            model = "gpt-4",
            temperature = 0.9f,
            max_tokens = 150,
            messages = new ChatMessage[]
            {
                new ChatMessage { role = "system", content = "You are a Korean poet AI that writes symbolic, emotional poetry with titles." },
                new ChatMessage { role = "user", content = prompt }
            }
        };

        string jsonData = JsonUtility.ToJson(requestData);
        UnityWebRequest request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("GPT 시 생성 실패: " + request.error);
            LoadingManager.Instance.SetLoading(false);
            Debug.Log("로딩 끝");
            yield break;
        }

        string content = JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text).choices[0].message.content;
        finalPoem = content;
        OnPoemGenerated?.Invoke(finalPoem);
        Debug.Log("생성된 시:\n" + content);

        // 제목과 본문 분리
        string[] lines = content.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length > 1)
        {
            resultTitleText.text = lines[0].Trim().Replace("\"", "").Replace("“", "").Replace("”", "");
            resultBodyText.text = string.Join("\n", lines, 1, lines.Length - 1);
        }
        else
        {
            resultTitleText.text = "무제";
            resultBodyText.text = content.Trim();
        }


    }




}
