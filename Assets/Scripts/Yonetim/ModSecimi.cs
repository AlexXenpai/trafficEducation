using UnityEngine;

public class ModSecimi : MonoBehaviour
{
    [Header("Oyuncular")]
    public GameObject arabaOyuncusu;
    public GameObject yayaOyuncusu;

    [Header("UI")]
    public GameObject girisPaneli;
    
    [Header("XR")]
    public GameObject xrRig;
    public GameObject speedPanel;
    public GameObject rulesPanel;
    private KameraTakip kameraTakip;

    void Start()
    {
        // XR Rig'i bul
        if (xrRig == null)
        {
            xrRig = GameObject.Find("XR Origin (XR Rig)");
        }

        // KameraTakip scriptini bul veya ekle
        if (xrRig != null)
        {
            kameraTakip = xrRig.GetComponent<KameraTakip>();
            if (kameraTakip == null)
            {
                kameraTakip = xrRig.AddComponent<KameraTakip>();
            }

            // Hedefleri ata
            if (arabaOyuncusu != null)
                kameraTakip.carTarget = arabaOyuncusu.transform;
            if (yayaOyuncusu != null)
                kameraTakip.pedestrianTarget = yayaOyuncusu.transform;
        }

        // Başlangıç ayarları
        Time.timeScale = 0;
        if (girisPaneli != null) girisPaneli.SetActive(true);
        if (arabaOyuncusu != null) arabaOyuncusu.SetActive(false);
        if (yayaOyuncusu != null) yayaOyuncusu.SetActive(false);
    }

    public void ArabaModunuSec()
    {
        Debug.Log("=== ARABA MODU SEÇİLDİ ===");
        speedPanel.SetActive(true);
        if (arabaOyuncusu != null) arabaOyuncusu.SetActive(true);
        if (yayaOyuncusu != null) yayaOyuncusu.SetActive(false);

        if (kameraTakip != null)
        {
            kameraTakip.SetMode(true); // Araba modu
        }

        // Kuralları Göster
        ShowRules(true);
    }

    public void YayaModunuSec()
    {
        Debug.Log("=== YAYA MODU SEÇİLDİ ===");
        speedPanel.SetActive(false);

        if (arabaOyuncusu != null) arabaOyuncusu.SetActive(false);
        if (yayaOyuncusu != null) yayaOyuncusu.SetActive(true);

        if (kameraTakip != null)
        {
            kameraTakip.SetMode(false); // Yaya modu
        }

        // Kuralları Göster
        ShowRules(false);
    }

    void ShowRules(bool isCarMode)
    {
        if (girisPaneli != null) girisPaneli.SetActive(false);

        RulesManager rules = FindObjectOfType<RulesManager>();
        if (rules != null)
        {
            rules.ShowRules(isCarMode);
            // Oyun kurallar kapatılınca başlayacak (RulesManager içindeki CloseRules fonksiyonu Time.timeScale = 1 yapmalı veya burası yönetmeli)
            // Şimdilik RulesManager'da timeScale yönetimi yok, o yüzden burada başlatmıyoruz.
            // RulesManager'daki CloseButton'a OyunuBaslat fonksiyonunu bağlamamız gerekebilir veya RulesManager kendisi başlatır.

            // En temizi: RulesManager'a bir callback vermek veya RulesManager'ın oyunu başlatmasını sağlamak.
            // Basitlik için RulesManager'ın Close fonksiyonunda Time.timeScale = 1 yapmasını sağlayalım.
        }
        else
        {
            OyunuBaslat();
        }
    }

    void OyunuBaslat()
    {
        if (girisPaneli != null) girisPaneli.SetActive(false);
        Time.timeScale = 1;
    }
}
