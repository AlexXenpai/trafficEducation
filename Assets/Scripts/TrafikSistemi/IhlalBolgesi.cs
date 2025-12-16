using UnityEngine;

public class IhlalBolgesi : MonoBehaviour
{
    public TrafikIsigi bagliTrafikIsigi; // Hangi isigi denetliyoruz?

    [Header("Ceza Ayarlari")]
    public int cezaPuani = 20;
    public string cezaMesaji = "Kirmizi Isik Ihlali";

    private bool cezaKesildi = false;

    // Arabanin etiketi (Tag) mutlaka "Player" olmali.
    private void OnTriggerEnter(Collider other)
    {
        // 1) OYUNCU ISE KONTROL ET
        if (other.CompareTag("Player"))
        {
            KontrolEt();
        }

        // 2) YAPAY ZEKA ISE DURDUR
        if (other.CompareTag("AI_Araba"))
        {
            AICar yapayZeka = other.GetComponent<AICar>();
            if (yapayZeka != null)
            {
                // Isik Kirmizi/Sari ise DUR
                if (bagliTrafikIsigi != null &&
                    bagliTrafikIsigi.suankiDurum != TrafikIsigi.IsikDurumu.Yesil)
                {
                    yapayZeka.TrafikIsigiDurumu(true);
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("AI_Araba"))
        {
            AICar yapayZeka = other.GetComponent<AICar>();
            if (yapayZeka != null && bagliTrafikIsigi != null)
            {
                if (bagliTrafikIsigi.suankiDurum == TrafikIsigi.IsikDurumu.Yesil)
                    yapayZeka.TrafikIsigiDurumu(false); // devam
                else
                    yapayZeka.TrafikIsigiDurumu(true);  // dur
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("AI_Araba"))
        {
            AICar yapayZeka = other.GetComponent<AICar>();
            if (yapayZeka != null)
            {
                yapayZeka.TrafikIsigiDurumu(false);
            }
        }

        // Oyuncu kutudan cikinca tekrar ceza kesilebilsin istiyorsan:
        if (other.CompareTag("Player"))
        {
            cezaKesildi = false;
        }
    }

    private void KontrolEt()
    {
        if (bagliTrafikIsigi == null) return;

        if (bagliTrafikIsigi.suankiDurum == TrafikIsigi.IsikDurumu.Kirmizi)
        {
            if (cezaKesildi) return; // spam engelle
            cezaKesildi = true;

            // 1) CEZA UYARISI
            Debug.Log("CEZA! " + cezaMesaji + " -" + cezaPuani);

            // 2) PUAN DUSUR
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CezaVer(cezaPuani, cezaMesaji);
            }
        }
        else
        {
            Debug.Log("Guvenli gecis.");
        }
    }
}
