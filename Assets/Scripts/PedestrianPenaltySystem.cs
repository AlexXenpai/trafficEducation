using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI; // NavMesh için gerekli
/// <summary>
/// Yaya ceza sistemi.
/// SidewalkTrigger içindeyse ASLA ceza vermez.
/// </summary>
public class PedestrianPenaltySystem : MonoBehaviour
{
    [Header("Ceza Ayarları")]
    public int roadPenalty = 20;
    public int redLightPenalty = 15;
    public float penaltyCooldown = 5f;
    
    [Header("Debug")]
    public bool showDebug = false;
    
    // Durum
    private float lastRoadPenaltyTime = -100f;
    private float lastRedLightPenaltyTime = -100f;
    private float checkInterval = 0.5f;
    private float nextCheckTime = 0f;
    
    // Yolda mı?
    private bool isOnRoad = false;
    
    // En yakın trafik ışığı
    private TrafikIsigi nearestTrafficLight = null;
    private int roadAreaIndex;
    private int sidewalkAreaIndex;

    void Start()
    {
        // Area indexlerini isimle alıyoruz (Hata payı sıfır)
        roadAreaIndex = NavMesh.GetAreaFromName("Road");
        sidewalkAreaIndex = NavMesh.GetAreaFromName("Sidewalk");

        if (roadAreaIndex == -1 || sidewalkAreaIndex == -1)
        {
            Debug.LogError("DİKKAT: Navigation Areas sekmesinde 'Road' veya 'Sidewalk' tanımlı değil!");
        }
    }
    void Update()
    {
        if (Time.time < nextCheckTime) return;
        nextCheckTime = Time.time + checkInterval;
        
        CheckGround();
        CheckPenalties();
    }

    void CheckGround()
    {
        // Her kontrolde durumları temizle ki eski verilerle ceza kesilmesin
        isOnRoad = false;
        nearestTrafficLight = null;

        // VR için daha güvenli bir sorgu noktası (yere yakın)
        Vector3 checkPos = transform.position;

        NavMeshHit navHit;
        // 5.0f yapıyoruz ki VR yüksekliği sorun çıkarmasın
        if (NavMesh.SamplePosition(checkPos, out navHit, 5.0f, NavMesh.AllAreas))
        {
            int currentAreaIndex = GetAreaIndexFromMask(navHit.mask);

            if (currentAreaIndex == roadAreaIndex)
            {
                // ÖNEMLİ: Burada yaya geçidi kontrolünü de iç içe yapmalısın.
                // Çünkü hem yolda hem yaya geçidinde olabilirsin.
                bool onCrosswalk = CheckCrosswalk();

                if (!onCrosswalk)
                {
                    isOnRoad = true;
                    if (showDebug) Debug.Log("<color=red>Yol Cezası Aktif</color>");
                }
            }
            else if (currentAreaIndex == sidewalkAreaIndex)
            {
                isOnRoad = false;
                if (showDebug) Debug.Log("<color=green>Kaldırım Güvenli</color>");
            }
        }
    }

    // Yaya geçidi kontrolünü ayır ki kafan karışmasın
    bool CheckCrosswalk()
    {
        Collider[] triggers = Physics.OverlapSphere(transform.position, 1.5f);
        foreach (var col in triggers)
        {
            PedestrianCrossZone crossZone = col.GetComponent<PedestrianCrossZone>();
            if (crossZone != null)
            {
                nearestTrafficLight = crossZone.bagliTrafikIsigi;
                return true;
            }
        }
        return false;
    }

    // Bitmask'ı düz Index'e çeviren yardımcı fonksiyon
    int GetAreaIndexFromMask(int mask)
    {
        for (int i = 0; i < 32; i++)
        {
            if ((mask & (1 << i)) != 0) return i;
        }
        return -1;
    }

    void CheckPenalties()
    {
        // ÖNCELİK 1: Kaldırım trigger'ı içindeyse ASLA ceza verme
        if (SidewalkTrigger.IsPlayerOnSidewalk())
        {
            return; // Güvenli bölge - hiçbir ceza yok
        }
        
        // ÖNCELİK 2: Yaya geçidindeyse sadece kırmızı ışık kontrolü
        if (nearestTrafficLight != null)
        {
            CheckRedLightPenalty();
            return;
        }
        
        // ÖNCELİK 3: Yoldaysa ceza ver
        if (isOnRoad)
        {
            if (Time.time - lastRoadPenaltyTime >= penaltyCooldown)
            {
                GiveRoadPenalty();
            }
        }
    }
    
    void CheckRedLightPenalty()
    {
        if (nearestTrafficLight == null)
        {
            FindNearestTrafficLight();
        }
        
        if (nearestTrafficLight == null) return;
        
        if (nearestTrafficLight.suankiDurum == TrafikIsigi.IsikDurumu.Yesil ||
            nearestTrafficLight.suankiDurum == TrafikIsigi.IsikDurumu.Sari)
        {
            if (Time.time - lastRedLightPenaltyTime >= penaltyCooldown)
            {
                GiveRedLightPenalty();
            }
        }
    }
    
    void FindNearestTrafficLight()
    {
        TrafikIsigi[] allLights = FindObjectsByType<TrafikIsigi>(FindObjectsSortMode.None);
        float closestDist = float.MaxValue;
        
        foreach (var light in allLights)
        {
            float dist = Vector3.Distance(transform.position, light.transform.position);
            if (dist < closestDist && dist < 25f)
            {
                closestDist = dist;
                nearestTrafficLight = light;
            }
        }
    }
    
    void GiveRoadPenalty()
    {
        lastRoadPenaltyTime = Time.time;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CezaVer(roadPenalty, "Yaya geçidi dışından karşıya geçildi!");
            Debug.Log($"CEZA: Yaya geçidi dışından geçiş! (-{roadPenalty} puan)");
        }
    }
    
    void GiveRedLightPenalty()
    {
        lastRedLightPenaltyTime = Time.time;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CezaVer(redLightPenalty, "Kırmızı ışıkta karşıya geçtiniz!");
            Debug.Log($"CEZA: Kırmızı ışıkta geçiş! (-{redLightPenalty} puan)");
        }
    }
}
