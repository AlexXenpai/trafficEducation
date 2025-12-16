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

    //[Header("Y r me Kontrol  (BUNLARI YEN  EKLED K)")]
    // Karakterin y r mesini sa layan script
    //public ActionBasedContinuousMoveProvider moveProvider;
    // Karakterin d nmesini sa layan script
    //public ActionBasedContinuousTurnProvider turnProvider;

    void Start()
    {
        if (xrRig == null) xrRig = GameObject.FindGameObjectWithTag("Player");

        // Ba lang  ta zaman dursun
        Time.timeScale = 0;
        girisPaneli.SetActive(true);
        arabaOyuncusu.SetActive(false);
        yayaOyuncusu.SetActive(false);
    }

    public void ArabaModunuSec()
    {
        Debug.Log("Araba Modu: Y r me KAPATILIYOR.");

        arabaOyuncusu.SetActive(true);

        // XR Rig'i arabaya sabitle
        /*xrRig.transform.SetParent(arabaOyuncusu.transform);
        xrRig.transform.localPosition = new Vector3(0, 1.0f, -0.2f); // Koltuk ayar  (Deneyerek bul)
        xrRig.transform.localRotation = Quaternion.identity;
        */
        // KR T K HAMLE: Karakterin y r mesini ve d nmesini kapat yoruz
        //if (moveProvider != null) moveProvider.enabled = false;
        //if (turnProvider != null) turnProvider.enabled = false;

        OyunuBaslat();
    }

    public void YayaModunuSec()
    {
        Debug.Log("Yaya Modu: Y r me A ILIYOR.");

        yayaOyuncusu.SetActive(true);

        // XR Rig'i yaya g vdesine sabitle
        // XR Origin'i yayaya götür
if (xrRig != null && yayaOyuncusu != null)
{
    xrRig.transform.position = yayaOyuncusu.transform.position + new Vector3(0, 1.6f, 0);
    xrRig.transform.rotation = yayaOyuncusu.transform.rotation;
}

        
        // KR T K HAMLE: Karakter art k y r yebilir
        //if (moveProvider != null) moveProvider.enabled = true;
        //if (turnProvider != null) turnProvider.enabled = true;

        OyunuBaslat();
    }

    void OyunuBaslat()
    {
        girisPaneli.SetActive(false);
        Time.timeScale = 1;
    }
}