#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public static class SetupGameOverUI
{
    [MenuItem("Tools/Setup/Game Over UI")]
    public static void Execute()
    {
        var panel = GameObject.Find("Canvas/GameOverPanel");
        if (panel == null)
        {
            Debug.LogError("GameOverPanel bulunamadı: Canvas/GameOverPanel");
            return;
        }

        // Panel: full screen overlay
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;

        // Background
        var img = panel.GetComponent<Image>();
        if (img != null)
        {
            img.color = new Color(0f, 0f, 0f, 0.75f);
            img.raycastTarget = true;
        }

        // CanvasGroup for show/hide
        var cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        var textGo = GameObject.Find("Canvas/GameOverPanel/GameOverText");
        if (textGo == null)
        {
            Debug.LogError("GameOverText bulunamadı: Canvas/GameOverPanel/GameOverText");
            return;
        }

        // Replace legacy Text with TMP
        var legacyText = textGo.GetComponent<UnityEngine.UI.Text>();
        if (legacyText != null)
            Object.DestroyImmediate(legacyText, true);

        var tmp = textGo.GetComponent<TextMeshProUGUI>();
        if (tmp == null) tmp = textGo.AddComponent<TextMeshProUGUI>();

        tmp.text = "Simülasyon başarısız!\nTekrar deneyin.";
        tmp.fontSize = 48;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        tmp.enableWordWrapping = true;

        var trt = textGo.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.5f, 0.5f);
        trt.anchorMax = new Vector2(0.5f, 0.5f);
        trt.pivot = new Vector2(0.5f, 0.5f);
        trt.anchoredPosition = Vector2.zero;
        trt.sizeDelta = new Vector2(800f, 250f);

        EditorUtility.SetDirty(panel);
        EditorUtility.SetDirty(textGo);

        Debug.Log("GameOver UI ayarlandı (CanvasGroup + TMP). Panel başlangıçta gizli (alpha=0)." );
    }
}
#endif
