using UnityEngine;

/// <summary>
/// Yaya geçiş bölgesi - SADECE TESPİT İÇİN.
/// Ceza verme işlemi PedestrianPenaltySystem tarafından yapılır.
/// </summary>
public class PedestrianCrossZone : MonoBehaviour
{
    public enum ZoneType
    {
        Crosswalk, // Yaya geçidi (güvenli alan)
        Road       // Yol (artık kullanılmıyor)
    }

    [Header("Zone Settings")]
    public ZoneType zoneType = ZoneType.Crosswalk;
    
    [Header("Traffic Light")]
    public TrafikIsigi bagliTrafikIsigi;
    public bool autoFindTrafficLight = true;
    public float trafficLightSearchRadius = 20f;

    // Bu zone içindeki AI yaya sayısı (trafik ışığı sistemi için)
    private int aiPedestrianCount = 0;

    public bool HasPedestrian()
    {
        return aiPedestrianCount > 0;
    }

    private void Start()
    {
        if (zoneType == ZoneType.Crosswalk && autoFindTrafficLight && bagliTrafikIsigi == null)
        {
            FindNearestTrafficLight();
        }
        
        if (CrosswalkManager.Instance != null)
            CrosswalkManager.Instance.Register(this);
    }
    
    private void FindNearestTrafficLight()
    {
        TrafikIsigi[] allLights = FindObjectsByType<TrafikIsigi>(FindObjectsSortMode.None);
        float closestDist = float.MaxValue;
        
        foreach (var light in allLights)
        {
            float dist = Vector3.Distance(transform.position, light.transform.position);
            if (dist < closestDist && dist < trafficLightSearchRadius)
            {
                closestDist = dist;
                bagliTrafikIsigi = light;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Sadece AI yayaları için sayaç (trafik ışığı sistemi)
        if (other.CompareTag("Pedestrian"))
        {
            // Oyuncu yayası değilse say
            if (other.GetComponent<PedestrianController>() == null &&
                other.GetComponentInParent<PedestrianController>() == null)
            {
                aiPedestrianCount++;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Pedestrian"))
        {
            if (other.GetComponent<PedestrianController>() == null &&
                other.GetComponentInParent<PedestrianController>() == null)
            {
                aiPedestrianCount = Mathf.Max(0, aiPedestrianCount - 1);
            }
        }
    }

    private void OnDestroy()
    {
        if (CrosswalkManager.Instance != null)
            CrosswalkManager.Instance.Unregister(this);
    }
}
