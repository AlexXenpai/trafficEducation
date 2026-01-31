using UnityEngine;

public class SpeedLimitZone : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Speed limit in km/h")]
    public float speedLimit = 50f;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object is the player car
        // We look for the SpeedMonitor component
        SpeedMonitor monitor = other.GetComponentInParent<SpeedMonitor>();
        if (monitor != null)
        {
            monitor.SetCurrentLimit(speedLimit);
            Debug.Log($"Entered Speed Zone: {speedLimit} km/h");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        SpeedMonitor monitor = other.GetComponentInParent<SpeedMonitor>();
        if (monitor != null)
        {
            monitor.ClearLimit();
            Debug.Log("Exited Speed Zone");
        }
    }
}
