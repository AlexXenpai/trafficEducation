using UnityEngine;

/// <summary>
/// Kırmızı ışık ihlal bölgesi.
/// Hem araba hem de yaya için çalışır.
/// </summary>
public class IhlalBolgesi : MonoBehaviour
{
    public TrafikIsigi bagliTrafikIsigi; // Hangi ışığı denetliyoruz?

    [Header("Araba Ceza Ayarları")]
    public int arabaCezaPuani = 30;
    public string arabaCezaMesaji = "Kırmızı Işıkta Geçtiniz!";

    [Header("Yaya Ceza Ayarları")]
    public int yayaCezaPuani = 15;
    public string yayaCezaMesaji = "Kırmızı Işıkta Karşıya Geçtiniz!";

    // Spam önleme
    private bool arabaCezaKesildi = false;
    private bool yayaCezaKesildi = false;

    private void OnTriggerEnter(Collider other)
    {
        // 1) OYUNCU ARABASI (PlayerCar tag'i)
        if (other.CompareTag("PlayerCar"))
        {
            KontrolEtAraba();
        }

        // 2) OYUNCU YAYASI
        if (IsPlayerPedestrian(other))
        {
            KontrolEtYaya();
        }

        // 3) YAPAY ZEKA ARABASI - Durdur
        if (other.CompareTag("AI_Araba"))
        {
            var aiCar = other.GetComponent<AICar>();
            if (aiCar != null && bagliTrafikIsigi != null)
            {
                bool durmali = bagliTrafikIsigi.suankiDurum == TrafikIsigi.IsikDurumu.Kirmizi ||
                               bagliTrafikIsigi.suankiDurum == TrafikIsigi.IsikDurumu.Sari;
                aiCar.TrafikIsigiDurumu(durmali);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Yaya ışıkta beklerken sürekli kontrol et
        if (IsPlayerPedestrian(other) && !yayaCezaKesildi)
        {
            KontrolEtYaya();
        }
        
        // Araba ışıkta beklerken sürekli kontrol et
        if (other.CompareTag("PlayerCar") && !arabaCezaKesildi)
        {
            KontrolEtAraba();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Oyuncu arabası çıkınca tekrar ceza kesilebilsin
        if (other.CompareTag("PlayerCar"))
        {
            arabaCezaKesildi = false;
        }

        // Oyuncu yayası çıkınca tekrar ceza kesilebilsin
        if (IsPlayerPedestrian(other))
        {
            yayaCezaKesildi = false;
        }

        // AI arabası çıkınca serbest bırak
        if (other.CompareTag("AI_Araba"))
        {
            var aiCar = other.GetComponent<AICar>();
            if (aiCar != null)
            {
                aiCar.TrafikIsigiDurumu(false);
            }
        }
    }

    private bool IsPlayerPedestrian(Collider other)
    {
        var controller = other.GetComponent<PedestrianController>();
        if (controller != null) return true;
        
        controller = other.GetComponentInParent<PedestrianController>();
        if (controller != null) return true;
        
        return false;
    }

    private void KontrolEtAraba()
    {
        if (bagliTrafikIsigi == null) return;

        if (bagliTrafikIsigi.suankiDurum == TrafikIsigi.IsikDurumu.Kirmizi)
        {
            if (arabaCezaKesildi) return;
            arabaCezaKesildi = true;

            Debug.Log("CEZA! " + arabaCezaMesaji + " -" + arabaCezaPuani);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.CezaVer(arabaCezaPuani, arabaCezaMesaji);
            }
        }
    }

    private void KontrolEtYaya()
    {
        if (bagliTrafikIsigi == null) return;

        // Yaya için: Yaya ışığı kırmızıysa (araç ışığı yeşilse) ceza ver
        // NOT: Genellikle yaya ışığı araç ışığının tersidir
        // Araç ışığı YEŞİL = Yaya ışığı KIRMIZI (yaya geçmemeli)
        // Araç ışığı KIRMIZI = Yaya ışığı YEŞİL (yaya geçebilir)
        
        if (bagliTrafikIsigi.suankiDurum == TrafikIsigi.IsikDurumu.Yesil ||
            bagliTrafikIsigi.suankiDurum == TrafikIsigi.IsikDurumu.Sari)
        {
            // Araç ışığı yeşil veya sarı = Yaya geçmemeli
            if (yayaCezaKesildi) return;
            yayaCezaKesildi = true;

            Debug.Log("CEZA! " + yayaCezaMesaji + " -" + yayaCezaPuani);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.CezaVer(yayaCezaPuani, yayaCezaMesaji);
            }
        }
    }
}
