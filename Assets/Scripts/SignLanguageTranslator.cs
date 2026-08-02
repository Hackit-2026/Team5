using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SignLanguageTranslator : MonoBehaviour
{
    [Header("API設定")]
    [SerializeField] private string apiKey = "sk-ここにあなたのAPIキーを貼り付けます";
    private string apiUrl = "https://api.openai.com/v1/chat/completions";

    [Header("UI設定")]
    [SerializeField] private TextMeshProUGUI currentWordsUI; // 現在認識している単語の羅列（私 学校 など）を表示

    [Header("ログ管理スクリプトへの参照")]
    // 翻訳結果をScrollViewに追加するために、GestureLogManagerを繋ぎます
    [SerializeField] private GestureLogManager logManager;

    // 認識した単語を貯めておくリスト
    private List<string> recognizedWords = new List<string>();

    // ジェスチャーを認識したときに呼ばれる関数（各ジェスチャーのイベントに設定する）
    public void AddWord(string word)
    {
        recognizedWords.Add(word);
        UpdateCurrentWordsUI();
    }

    // 現在の単語の羅列をUIに表示
    private void UpdateCurrentWordsUI()
    {
        if (currentWordsUI != null)
        {
            currentWordsUI.text = "認識中: " + string.Join(" ", recognizedWords);
        }
    }

    // 「翻訳実行」の合図で呼ばれる関数（翻訳ジェスチャーのイベントに設定する）
    public void Translate()
    {
        if (recognizedWords.Count == 0) return;

        string wordsStr = string.Join(" ", recognizedWords);

        // とりあえずログに「翻訳中...」と出す（任意）
        if (logManager != null)
        {
            logManager.AddLog("AIで翻訳中...");
        }

        // AIにリクエストを送信
        StartCoroutine(SendRequestToAI(wordsStr));
    }

    // 単語リストをリセットする関数
    public void ClearWords()
    {
        recognizedWords.Clear();
        UpdateCurrentWordsUI();
    }

    // --- AIと通信する処理 ---
    private IEnumerator SendRequestToAI(string words)
    {
        // プロンプト（指示）
        string systemPrompt = "あなたは手話の翻訳者です。入力された単語の羅列を、自然な日本語の会話文に変換して出力してください。出力は変換後の文章のみにしてください。";

        string jsonPayload = $@"{{
            ""model"": ""gpt-4o-mini"",
            ""messages"": [
                {{""role"": ""system"", ""content"": ""{systemPrompt}""}},
                {{""role"": ""user"", ""content"": ""{words}""}}
            ]
        }}";

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("エラー: " + request.error);
                if (logManager != null) logManager.AddLog("通信エラーが発生しました");
            }
            else
            {
                // 返ってきた文章を抜き出す
                string responseText = request.downloadHandler.text;
                string translatedSentence = ExtractContentFromJson(responseText);

                // ★ここでログマネージャーを呼んで、ScrollViewにAIの翻訳結果を追加する！
                if (logManager != null)
                {
                    logManager.AddLog(translatedSentence);
                }

                // 翻訳が終わったので、単語リスト（認識中: 私 学校... の表示）をリセットする
                ClearWords();
            }
        }
    }

    private string ExtractContentFromJson(string json)
    {
        try
        {
            string searchStr = "\"content\": \"";
            int startIndex = json.IndexOf(searchStr) + searchStr.Length;
            int endIndex = json.IndexOf("\"", startIndex);
            string result = json.Substring(startIndex, endIndex - startIndex);
            result = result.Replace("\\n", "\n").Replace("\\\"", "\"");
            return result;
        }
        catch
        {
            return "翻訳の解析に失敗しました";
        }
    }
}