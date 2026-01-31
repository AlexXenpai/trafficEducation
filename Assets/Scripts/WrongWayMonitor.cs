using UnityEngine;
using System.Collections.Generic;

public class WrongWayMonitor : MonoBehaviour
{
    [Header("Settings")]
    public Transform routesParent;
    public float checkInterval = 0.2f;
    public float laneWidthThreshold = 5.0f; // Distance to consider being "in" a lane
    public int penaltyPoints = 50;
    public string penaltyMessage = "Ters Şeritte Gidiyorsunuz!";
    public float penaltyCooldown = 3.0f; // Seconds between penalties

    private List<Transform> allWaypoints = new List<Transform>();
    private float lastCheckTime;
    private float lastPenaltyTime;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Auto-find Routes if not assigned
        if (routesParent == null)
        {
            GameObject routesObj = GameObject.Find("Routes");
            if (routesObj != null) routesParent = routesObj.transform;
        }

        if (routesParent != null)
        {
            // Collect all waypoints
            foreach (Transform route in routesParent)
            {
                foreach (Transform wp in route)
                {
                    allWaypoints.Add(wp);
                }
            }
        }
        else
        {
            Debug.LogWarning("WrongWayMonitor: 'Routes' parent not found. Wrong way detection disabled.");
        }
    }

    void Update()
    {
        if (Time.time - lastCheckTime > checkInterval)
        {
            CheckWrongWay();
            lastCheckTime = Time.time;
        }
    }

    void CheckWrongWay()
    {
        if (allWaypoints.Count == 0) return;
        if (rb != null && rb.linearVelocity.magnitude < 1.0f) return;

        Transform bestWaypoint = null;
        float minWeight = float.MaxValue;
        Vector3 currentPos = transform.position;

        foreach (Transform wp in allWaypoints)
        {
            float dstSqr = Vector3.SqrMagnitude(wp.position - currentPos);

            // Sadece belli bir mesafe içindeki noktaları değerlendir
            if (dstSqr > (laneWidthThreshold * laneWidthThreshold)) continue;

            Vector3 wpDirection = GetLaneDirection(wp);
            float dot = Vector3.Dot(transform.forward, wpDirection);

            // AĞIRLIKLANDIRMA: Eğer nokta bizim yönümüze tersse, onu "uzakmış gibi" gösteriyoruz.
            // Böylece yan şeritteki (ters yönlü) nokta daha yakın olsa bile seçilmiyor.
            float weight = dstSqr;
            if (dot < 0) weight *= 5.0f; // Ters yönlü noktaları 5 kat daha "uzak" say.

            if (weight < minWeight)
            {
                minWeight = weight;
                bestWaypoint = wp;
            }
        }

        if (bestWaypoint != null)
        {
            Vector3 finalDirection = GetLaneDirection(bestWaypoint);
            float finalDot = Vector3.Dot(transform.forward, finalDirection);

            // -0.7 kullanarak toleransı biraz daha artırıyoruz (virajlar için)
            if (finalDot < -0.7f)
            {
                if (Time.time - lastPenaltyTime > penaltyCooldown)
                {
                    GameManager.Instance?.CezaVer(penaltyPoints, penaltyMessage);
                    lastPenaltyTime = Time.time;
                }
            }
        }
    }

    Vector3 GetLaneDirection(Transform wp)
    {
        // Based on AICar logic: Cars move from Child[i] to Child[i-1]
        // So flow direction is: Child[i-1].position - Child[i].position
        
        int index = wp.GetSiblingIndex();
        Transform parent = wp.parent;
        
        if (index > 0)
        {
            // Normal case: Direction towards previous child
            return (parent.GetChild(index - 1).position - wp.position).normalized;
        }
        else if (parent.childCount > 1)
        {
            // End of path (Index 0): Use direction from (1 -> 0)
            return (wp.position - parent.GetChild(1).position).normalized;
        }
        
        return wp.forward; // Fallback
    }
}
