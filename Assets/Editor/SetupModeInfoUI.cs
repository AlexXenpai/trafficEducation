#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public static class SetupModeInfoUI
{
    [MenuItem("Tools/Setup/Mode Info UI")]
    public static void Execute()
    {
        var panel = GameObject.Find("Canvas/ModeInfoPanel");
        if (panel == null)
        {
            Debug.LogError("ModeInfoPanel bulunamadı: Canvas/ModeInfoPanel");
            return;
        }

        // Ensure CanvasGroup
        var cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        // Ensure Image
        var img = panel.GetComponent<Image>();
        if (img != null)
        {
            img.raycastTarget = false;
            img.color = new Color(0f, 0f, 0f, 0.65f);
        }

        // RectTransform settings (top center)
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -20f);
        rt.sizeDelta = new Vector2(760f, 140f);

        var textGo = GameObject.Find("Canvas/ModeInfoPanel/ModeInfoText");
        if (textGo == null)
        {
            Debug.LogError("ModeInfoText bulunamadı: Canvas/ModeInfoPanel/ModeInfoText");
            return;
        }

        // Eğer eski UGUI Text varsa kaldırıp TMP ekle
        var legacyText = textGo.GetComponent<UnityEngine.UI.Text>();
        if (legacyText != null)
        {
            Object.DestroyImmediate(legacyText, true);
        }

        var tmp = textGo.GetComponent<TextMeshProUGUI>();
        if (tmp == null)
        {
            tmp = textGo.AddComponent<TextMeshProUGUI>();
        }

        tmp.raycastTarget = false;
        tmp.text = "";
        tmp.fontSize = 26;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        var textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.pivot = new Vector2(0.5f, 0.5f);
        textRt.anchoredPosition = Vector2.zero;
        textRt.sizeDelta = Vector2.zero;
        textRt.offsetMin = new Vector2(20f, 10f);
        textRt.offsetMax = new Vector2(-20f, -10f);

        EditorUtility.SetDirty(panel);
        EditorUtility.SetDirty(textGo);

        Debug.Log("ModeInfo UI ayarlandı (CanvasGroup + RectTransform + TMP). Panel başlangıçta gizli (alpha=0)." );
    }
}
#endif
