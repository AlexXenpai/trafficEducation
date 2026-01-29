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

        if (arabaOyuncusu != null) arabaOyuncusu.SetActive(true);
        if (yayaOyuncusu != null) yayaOyuncusu.SetActive(false);

        if (kameraTakip != null)
        {
            kameraTakip.SetMode(true); // Araba modu
        }

        OyunuBaslat();
    }

    public void YayaModunuSec()
    {
        Debug.Log("=== YAYA MODU SEÇİLDİ ===");

        if (arabaOyuncusu != null) arabaOyuncusu.SetActive(false);
        if (yayaOyuncusu != null) yayaOyuncusu.SetActive(true);

        if (kameraTakip != null)
        {
            kameraTakip.SetMode(false); // Yaya modu
        }

        OyunuBaslat();
    }

    void OyunuBaslat()
    {
        if (girisPaneli != null) girisPaneli.SetActive(false);
        Time.timeScale = 1;
    }
}
