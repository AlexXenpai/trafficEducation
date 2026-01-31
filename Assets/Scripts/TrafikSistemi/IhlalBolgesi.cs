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

    [Header("Yön Kontrolü")]
    [Tooltip("Eğer aktifse, sadece bölge ile aynı yönde giden araçları durdurur/cezalandırır.")]
    public bool yonKontroluYap = true;
    [Tooltip("Kabul edilebilir açı farkı (1: Tam Aynı, 0: Dik, -1: Ters). Örn: 0.2 = ~78 derece")]
    [Range(-1f, 1f)]
    public float aciLimiti = 0.2f;

    // Spam önleme
    private bool arabaCezaKesildi = false;
    private bool yayaCezaKesildi = false;

    private void OnTriggerEnter(Collider other)
    {
        // Yön kontrolü
        if (yonKontroluYap && !YonUygunMu(other.transform))
        {
            return;
        }

        // 1) OYUNCU ARABASI (PlayerCar tag'i)
        if (other.CompareTag("PlayerCar"))
        {
            KontrolEtAraba();
        }

        // 2) OYUNCU YAYASI
        // Yaya kırmızı ışık cezası PedestrianPenaltySystem tarafından yönetiliyor.

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
        // Yön kontrolü (Stay'de de gerekebilir eğer araç içinde dönerse)
        if (yonKontroluYap && !YonUygunMu(other.transform))
        {
            // Eğer yönü artık uymuyorsa (döndüyse) ve AI ise serbest bırak
             if (other.CompareTag("AI_Araba"))
            {
                var aiCar = other.GetComponent<AICar>();
                if (aiCar != null) aiCar.TrafikIsigiDurumu(false);
            }
            return;
        }

        // Araba ışıkta beklerken sürekli kontrol et
        if (other.CompareTag("PlayerCar") && !arabaCezaKesildi)
        {
            KontrolEtAraba();
        }
        
        // AI Arabası için Yeşil Işık Kontrolü
        // TrafikIsigi.cs artık objeyi disable etmediği için burası çalışacak.
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

    private void OnTriggerExit(Collider other)
    {
        // Oyuncu arabası çıkınca tekrar ceza kesilebilsin
        if (other.CompareTag("PlayerCar"))
        {
            arabaCezaKesildi = false;
        }

        // Oyuncu yayası için burada ceza yönetimi yok.

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
    
    private bool YonUygunMu(Transform aracTransform)
    {
        // Aracın ileri vektörü ile bölgenin ileri vektörü arasındaki açı
        // Dot product: 1 = aynı yön, 0 = dik, -1 = ters
        float dot = Vector3.Dot(transform.forward, aracTransform.forward);
        return dot > aciLimiti;
    }
    
    private void OnDrawGizmos()
    {
        if (yonKontroluYap)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * 2f);
        }
    }
}
