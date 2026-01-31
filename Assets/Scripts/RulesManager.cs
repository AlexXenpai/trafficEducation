using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RulesManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject rulesPanel;
    public TextMeshProUGUI rulesTitleText;
    public TextMeshProUGUI rulesContentText;
    public Button closeButton;

    [Header("Rules Content")]
    [TextArea(5, 10)]
    public string carModeRules = 
        "ARABA MODU KURALLARI:\n\n" +
        "1. Kırmızı Işık İhlali: -30 Puan\n" +
        "2. Hız Sınırını Aşma: -10 Puan (Saniye Başına)\n" +
        "3. Ters Şeritte Gitme: -50 Puan\n" +
        "4. Yayaya Çarpma: -20 Puan\n" +
        "5. Diğer Araçlara Çarpma: -10 Puan\n" +
        "6. Sinyal Vermeden Dönüş: -15 Puan\n\n" +
        "Hedef: Trafik kurallarına uyarak en yüksek puanı topla!";

    [TextArea(5, 10)]
    public string pedestrianModeRules = 
        "YAYA MODU KURALLARI:\n\n" +
        "1. Kırmızı Işıkta Geçme: -15 Puan\n" +
        "2. Yaya Geçidi Dışından Geçme: -10 Puan\n" +
        "3. Araçlara Çarpma: -20 Puan\n\n" +
        "Hedef: Güvenli bir şekilde karşıdan karşıya geç ve şehri keşfet!";

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseRules);
        }
        
        // Hide initially, will be shown by Mode Selection
        if (rulesPanel != null) rulesPanel.SetActive(false);
    }

    public void ShowRules(bool isCarMode)
    {
        if (rulesPanel != null)
        {
            rulesPanel.SetActive(true);
            
            if (rulesTitleText != null)
                rulesTitleText.text = isCarMode ? "TRAFİK KURALLARI (ARABA)" : "TRAFİK KURALLARI (YAYA)";
            
            if (rulesContentText != null)
                rulesContentText.text = isCarMode ? carModeRules : pedestrianModeRules;
                
            // Pause game while reading rules? Optional.
            // Time.timeScale = 0; 
        }
    }

    public void CloseRules()
    {
        if (rulesPanel != null)
        {
            rulesPanel.SetActive(false);
            Time.timeScale = 1;
        }
    }
}
