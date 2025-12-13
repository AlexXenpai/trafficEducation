using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; // BU SATIRI EKLEMEZSEN HATA ALIRSIN

public class ModSecimi : MonoBehaviour
{
    [Header("Oyuncular")]
    public GameObject arabaOyuncusu;
    public GameObject yayaOyuncusu;

    [Header("UI")]
    public GameObject girisPaneli;
    public GameObject xrRig;

    [Header("Yürüme Kontrolü (BUNLARI YENÝ EKLEDÝK)")]
    // Karakterin yürümesini saðlayan script
    public ActionBasedContinuousMoveProvider moveProvider;
    // Karakterin dönmesini saðlayan script
    public ActionBasedContinuousTurnProvider turnProvider;

    void Start()
    {
        if (xrRig == null) xrRig = GameObject.FindGameObjectWithTag("Player");

        // Baþlangýçta zaman dursun
        Time.timeScale = 0;
        girisPaneli.SetActive(true);
        arabaOyuncusu.SetActive(false);
        yayaOyuncusu.SetActive(false);
    }

    public void ArabaModunuSec()
    {
        Debug.Log("Araba Modu: Yürüme KAPATILIYOR.");

        arabaOyuncusu.SetActive(true);

        // XR Rig'i arabaya sabitle
        xrRig.transform.SetParent(arabaOyuncusu.transform);
        xrRig.transform.localPosition = new Vector3(0, 1.0f, -0.2f); // Koltuk ayarý (Deneyerek bul)
        xrRig.transform.localRotation = Quaternion.identity;

        // KRÝTÝK HAMLE: Karakterin yürümesini ve dönmesini kapatýyoruz
        if (moveProvider != null) moveProvider.enabled = false;
        if (turnProvider != null) turnProvider.enabled = false;

        OyunuBaslat();
    }

    public void YayaModunuSec()
    {
        Debug.Log("Yaya Modu: Yürüme AÇILIYOR.");

        yayaOyuncusu.SetActive(true);

        // XR Rig'i yaya gövdesine sabitle
        xrRig.transform.SetParent(yayaOyuncusu.transform);
        xrRig.transform.localPosition = new Vector3(0, 1.6f, 0);
        xrRig.transform.localRotation = Quaternion.identity;

        // KRÝTÝK HAMLE: Karakter artýk yürüyebilir
        if (moveProvider != null) moveProvider.enabled = true;
        if (turnProvider != null) turnProvider.enabled = true;

        OyunuBaslat();
    }

    void OyunuBaslat()
    {
        girisPaneli.SetActive(false);
        Time.timeScale = 1;
    }
}