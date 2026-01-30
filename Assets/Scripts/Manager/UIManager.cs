using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Elemanlar")]
    public TextMeshProUGUI puanText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI cezaUyariText;

    [Header("Summary UI")]
    public GameObject summaryPanel;
    public TextMeshProUGUI summaryScoreText;
    public TextMeshProUGUI summaryMistakesText;

    [Header("Mod Bilgilendirme")]
    public CanvasGroup modeInfoGroup;
    public TextMeshProUGUI modeInfoText;
    public float modeInfoDuration = 6f;

    Coroutine modeInfoCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Script aktif oldugunda dinlemeye basla
    private void OnEnable()
    {
        // GameManager'in olaylarina abone oluyoruz
        GameManager.OnPuanDegisti += PuanGuncelle;
        GameManager.OnCezaYendi += CezayiGoster;
        GameManager.OnTimerTick += UpdateTimer;
        GameManager.OnGameSuccess += ShowSummary;
    }

    // Script pasif oldugunda (veya obje yok oldugunda) dinlemeyi birak
    private void OnDisable()
    {
        GameManager.OnPuanDegisti -= PuanGuncelle;
        GameManager.OnCezaYendi -= CezayiGoster;
        GameManager.OnTimerTick -= UpdateTimer;
        GameManager.OnGameSuccess -= ShowSummary;
    }

    void Start()
    {
        // Oyun baslar baslamaz mevcut puani ekrana yaz
        PuanGuncelle(GameManager.Instance.toplamPuan);
        cezaUyariText.text = ""; // Baslangicta uyari olmasin

        HideModeInfoImmediate();
    }

    // GameManager "OnPuanDegisti" diye bagirinca bu calisacak
    void PuanGuncelle(int yeniPuan)
    {
        puanText.text = "Puan: " + yeniPuan.ToString();
    }

    // GameManager "OnCezaYendi" diye bagirinca bu calisacak
    void CezayiGoster(string sebep)
    {
        // Once durdur ki ust uste binmesin
        StopAllCoroutines();
        // Uyariyi gosterip 2 saniye sonra gizleyen coroutine'i baslat
        StartCoroutine(UyariGosterGizle("CEZA! " + sebep));
    }

    IEnumerator UyariGosterGizle(string mesaj)
    {
        cezaUyariText.text = mesaj;
        cezaUyariText.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f); // 2 saniye bekle

        cezaUyariText.text = "";
        cezaUyariText.gameObject.SetActive(false);
    }

    public void ShowModeInfo(string message)
    {
        if (modeInfoGroup == null || modeInfoText == null) return;

        if (modeInfoCoroutine != null)
            StopCoroutine(modeInfoCoroutine);

        modeInfoText.text = message;
        modeInfoGroup.alpha = 1f;
        modeInfoGroup.interactable = false;
        modeInfoGroup.blocksRaycasts = false;

        modeInfoCoroutine = StartCoroutine(ModeInfoRoutine());
    }

    IEnumerator ModeInfoRoutine()
    {
        yield return new WaitForSeconds(modeInfoDuration);
        HideModeInfoImmediate();
        modeInfoCoroutine = null;
    }

    void HideModeInfoImmediate()
    {
        if (modeInfoGroup == null) return;
        modeInfoGroup.alpha = 0f;
        modeInfoGroup.interactable = false;
        modeInfoGroup.blocksRaycasts = false;
        if (modeInfoText != null) modeInfoText.text = "";
    }

    void UpdateTimer(float time)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60F);
            int seconds = Mathf.FloorToInt(time - minutes * 60);
            timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
        }
    }

    void ShowSummary(int score, List<string> mistakes)
    {
        if (summaryPanel != null)
        {
            summaryPanel.SetActive(true);
            if (summaryScoreText != null)
                summaryScoreText.text = "Puan: " + score;
            
            if (summaryMistakesText != null)
            {
                if (mistakes.Count == 0)
                {
                    summaryMistakesText.text = "Hata Yok! Tebrikler.";
                }
                else
                {
                    summaryMistakesText.text = string.Join("\n", mistakes);
                }
            }
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
