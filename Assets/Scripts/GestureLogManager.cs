using UnityEngine;
using UnityEngine.UI; // ScrollRectを使うために必要
using TMPro;
using System.Collections; // コルーチン（1フレーム待つ処理）を使うために必要

public class GestureLogManager : MonoBehaviour
{
    [Header("ログを追加する場所")]
    [SerializeField] private Transform contentTransform;

    [Header("生成するテキストのプレハブ")]
    [SerializeField] private GameObject logTextPrefab;

    [Header("スクロールビュー本体")]
    [SerializeField] private ScrollRect scrollRect; // 自動スクロール用にここを追加！

    // ジェスチャー認識時にこの関数を呼ぶ
    public void AddLog(string message)
    {
        // プレハブをContentの子オブジェクトとして生成
        GameObject newLog = Instantiate(logTextPrefab, contentTransform);

        // 生成したプレハブのTextMeshProコンポーネントを取得して、文字を書き換える
        TextMeshProUGUI textComp = newLog.GetComponent<TextMeshProUGUI>();
        if (textComp != null)
        {
            textComp.text = message;
        }

        // ログを追加した後に、一番下までスクロールさせる処理を呼ぶ
        StartCoroutine(ScrollToBottom());
    }

    // 自動スクロール用の処理
    private IEnumerator ScrollToBottom()
    {
        // UIの縦幅（Content Size Fitter）の再計算が終わるまで1フレームだけ待つ
        yield return null;

        // スクロール位置を一番下（0.0f）に設定する
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}