using UnityEngine;

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

    void Update()
    {
        if (Time.time < nextCheckTime) return;
        nextCheckTime = Time.time + checkInterval;
        
        CheckGround();
        CheckPenalties();
    }
    
    void CheckGround()
    {
        isOnRoad = false;
        nearestTrafficLight = null;
        
        // Yaya geçidi kontrolü
        Vector3 checkPos = transform.position + Vector3.up * 0.5f;
        Collider[] triggers = Physics.OverlapSphere(checkPos, 1.5f);
        bool isOnCrosswalk = false;
        
        foreach (var col in triggers)
        {
            PedestrianCrossZone crossZone = col.GetComponent<PedestrianCrossZone>();
            if (crossZone != null)
            {
                isOnCrosswalk = true;
                if (crossZone.bagliTrafikIsigi != null)
                {
                    nearestTrafficLight = crossZone.bagliTrafikIsigi;
                }
            }
        }
        
        // Yaya geçidindeyse yol kontrolü yapma
        if (isOnCrosswalk) return;
        
        // Zemin kontrolü - Raycast ile (QueryTriggerInteraction.Ignore ile trigger'ları atla)
        Vector3 rayStart = transform.position + Vector3.up * 1f;
        RaycastHit hit;
        
        if (Physics.Raycast(rayStart, Vector3.down, out hit, 3f, ~0, QueryTriggerInteraction.Ignore))
        {
            string hitName = hit.collider.gameObject.name.ToLower();
            string parentName = hit.collider.transform.parent != null 
                ? hit.collider.transform.parent.name.ToLower() : "";
            
            // YOL kontrolü
            if (hitName.Contains("road") && parentName.Contains("roads"))
            {
                isOnRoad = true;
            }
            
            if (showDebug)
            {
                bool onSidewalk = SidewalkTrigger.IsPlayerOnSidewalk();
                Debug.Log($"[Pedestrian] Kaldırım:{onSidewalk} Yol:{isOnRoad} | Hit:{hitName}");
            }
        }
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
