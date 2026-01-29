using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AutoFindTrafficLightFineZone : MonoBehaviour
{
    [Header("Auto-find settings")]
    public float searchRadius = 30f;
    public LayerMask trafficLightLayer = ~0; // istersen sadece "TrafficLight" layer seçersin

    [Header("Player")]
    public string playerTag = "Player";

    [Header("Fine")]
    public int fineAmount = 500;
    public float cooldownSeconds = 2f;

    [Header("Debug")]
    public bool drawGizmos = true;

    private TrafficLightController foundLight;
    private float lastFineTime = -999f;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void Start()
    {
        FindNearestTrafficLight();
        if (foundLight == null)
            Debug.LogWarning($"[{name}] Yakında TrafficLightController bulamadım. Radius={searchRadius}", this);
    }

    public void FindNearestTrafficLight()
    {
        foundLight = null;

        // Radius içindeki collider'ları tara
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, trafficLightLayer, QueryTriggerInteraction.Ignore);

        float bestDistSqr = float.PositiveInfinity;

        foreach (var h in hits)
        {
            // Parent/child fark etmesin diye: ışık controller nerede ise bul
            var tl = h.GetComponentInParent<TrafficLightController>();
            if (tl == null) continue;

            float dSqr = (tl.transform.position - transform.position).sqrMagnitude;
            if (dSqr < bestDistSqr)
            {
                bestDistSqr = dSqr;
                foundLight = tl;
            }
        }

        if (foundLight != null)
            Debug.Log($"[{name}] En yakın ışık: {foundLight.name}", this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        // ışık bulunmadıysa bir daha dene (sahne geç yüklenirse falan)
        if (foundLight == null) FindNearestTrafficLight();
        if (foundLight == null) return;

        if (Time.time - lastFineTime < cooldownSeconds) return;

        if (foundLight.IsRed())
        {
            lastFineTime = Time.time;
            Debug.Log($"CEZA! Kırmızı ışıkta yaya geçti: {fineAmount}₺ (Işık: {foundLight.name})");
            // burada kendi puan/para sistemine bağlarsın
        }
        else
        {
            Debug.Log($"Serbest (yeşil). (Işık: {foundLight.name})");
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}
