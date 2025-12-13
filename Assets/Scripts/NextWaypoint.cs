using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Next_Waypoint : MonoBehaviour
{
    [Tooltip("Buraya gidilecek diðer yollarýn ÝLK Waypoint objelerini sürükle.")]
    public GameObject[] Waypoints;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            AICar carAI = other.GetComponent<AICar>();

            // Güvenlik kontrolü: Script yoksa veya liste boþsa iptal et
            if (carAI == null || Waypoints.Length == 0) return;

            // 1. Rastgele bir hedef waypoint seç (Örn: Route_2'nin 3. waypointi)
            int randomIndex = Random.Range(0, Waypoints.Length);
            GameObject targetWaypoint = Waypoints[randomIndex];

            // 2. Rota Deðiþimi: Seçilen waypoint'in parent'ý yeni Rota (Route Holder) olur.
            carAI.currentTrafficRoute = targetWaypoint.transform.parent.gameObject;

            // 3. Ýndex Atamasý (SENÝN ÝSTEDÝÐÝN KISIM):
            // Seçilen waypoint, parent'ýnýn kaçýncý çocuðuysa (sibling index),
            // araba o numaralý hedefe kilitlenir.
            carAI.currentWapointNumber = targetWaypoint.transform.GetSiblingIndex();

            // 4. Eski hedefi temizle
            carAI.nextWaypoint = null;
        }
    }
}