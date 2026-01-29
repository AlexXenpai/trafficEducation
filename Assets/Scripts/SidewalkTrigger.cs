using UnityEngine;

/// <summary>
/// Kaldırım güvenli bölge trigger'ı.
/// Yaya bu trigger içindeyken asla yol cezası almaz.
/// </summary>
public class SidewalkTrigger : MonoBehaviour
{
    // Static sayaç - kaç kaldırım trigger'ı içindeyiz
    private static int playerInsideCount = 0;
    
    public static bool IsPlayerOnSidewalk()
    {
        return playerInsideCount > 0;
    }
    
    private bool playerInside = false;

    private void OnTriggerEnter(Collider other)
    {
        // Oyuncu yayası mı kontrol et
        if (IsPlayerPedestrian(other) && !playerInside)
        {
            playerInside = true;
            playerInsideCount++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsPlayerPedestrian(other) && playerInside)
        {
            playerInside = false;
            playerInsideCount = Mathf.Max(0, playerInsideCount - 1);
        }
    }
    
    private bool IsPlayerPedestrian(Collider other)
    {
        if (other.GetComponent<PedestrianController>() != null) return true;
        if (other.GetComponentInParent<PedestrianController>() != null) return true;
        if (other.GetComponent<PedestrianPenaltySystem>() != null) return true;
        if (other.GetComponentInParent<PedestrianPenaltySystem>() != null) return true;
        return false;
    }
    
    private void OnDisable()
    {
        if (playerInside)
        {
            playerInsideCount = Mathf.Max(0, playerInsideCount - 1);
            playerInside = false;
        }
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        playerInsideCount = 0;
    }
}
