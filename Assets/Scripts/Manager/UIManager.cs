using UnityEngine;
using TMPro; // TextMeshPro kullanmak i�in �art
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Elemanlar�")]
    public TextMeshProUGUI puanText;
    public TextMeshProUGUI cezaUyariText;

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

    // Script aktif oldu�unda dinlemeye ba�la
    private void OnEnable()
    {
        // GameManager'�n olaylar�na abone oluyoruz
        GameManager.OnPuanDegisti += PuanGuncelle;
        GameManager.OnCezaYendi += CezayiGoster;
    }

    // Script pasif oldu�unda (veya obje yok oldu�unda) dinlemeyi b�rak
    // BUNU YAPMAZSAN HAFIZA SIZINTISI (MEMORY LEAK) OLUR. �OK �NEML�.
    private void OnDisable()
    {
        GameManager.OnPuanDegisti -= PuanGuncelle;
        GameManager.OnCezaYendi -= CezayiGoster;
    }

    void Start()
    {
        // Oyun ba�lar ba�lamaz mevcut puan� ekrana yaz
        PuanGuncelle(GameManager.Instance.toplamPuan);
        cezaUyariText.text = ""; // Ba�lang��ta uyar� olmas�n

        HideModeInfoImmediate();
    }

    // GameManager "OnPuanDegisti" diye ba��r�nca bu �al��acak
    void PuanGuncelle(int yeniPuan)
    {
        puanText.text = "Puan: " + yeniPuan.ToString();
    }

    // GameManager "OnCezaYendi" diye ba��r�nca bu �al��acak
    void CezayiGoster(string sebep)
    {
        // �nce durdur ki �st �ste binmesin
        StopAllCoroutines();
        // Uyar�y� g�sterip 2 saniye sonra gizleyen coroutine'i ba�lat
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
}