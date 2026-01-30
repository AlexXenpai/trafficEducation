using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Araba modundayken (PlayerCar aktifken) araç NavMesh "Road" alanının dışına çıkarsa ceza verir.
/// </summary>
public class CarOffroadPenalty : MonoBehaviour
{
    [Header("Ceza")]
    public int offroadPenalty = 10;
    public string offroadMessage = "yol dışına çıktınız 5 saniye içinde geri girin";

    [Header("Kontrol")]
    public float checkInterval = 0.25f;
    public float gracePeriodSeconds = 5.0f;
    public float sampleMaxDistance = 5.0f;

    int roadAreaIndex;
    float nextCheckTime;

    bool wasOffroad;
    float nextOffroadPenaltyTime;

    void Start()
    {
        roadAreaIndex = NavMesh.GetAreaFromName("Road");
        if (roadAreaIndex == -1)
            Debug.LogError("DİKKAT: Navigation Areas sekmesinde 'Road' tanımlı değil! (CarOffroadPenalty)");
    }

    void Update()
    {
        if (!isActiveAndEnabled) return;
        if (Time.time < nextCheckTime) return;
        nextCheckTime = Time.time + checkInterval;

        // Bu komponent PlayerCar üzerinde olmalı. Araba modu dışında zaten PlayerCar disable olduğu için çalışmaz.
        if (!gameObject.activeInHierarchy) return;

        // Ceza sistemi yoksa çık.
        if (GameManager.Instance == null) return;

        bool isOffroad = IsOffroad();

        if (!wasOffroad && isOffroad)
        {
            // Yol dışına ilk çıkış: anında ceza + sonraki ceza için 5 sn sayaç
            wasOffroad = true;
            GameManager.Instance.CezaVer(offroadPenalty, offroadMessage);
            nextOffroadPenaltyTime = Time.time + gracePeriodSeconds;
            return;
        }

        if (wasOffroad && !isOffroad)
        {
            // Yola geri girdi: sayaç sıfırla
            wasOffroad = false;
            return;
        }

        // Yol dışındayken her 5 saniyede bir tekrar ceza kes
        if (wasOffroad && isOffroad)
        {
            if (Time.time >= nextOffroadPenaltyTime)
            {
                GameManager.Instance.CezaVer(offroadPenalty, offroadMessage);
                nextOffroadPenaltyTime = Time.time + gracePeriodSeconds;
            }
        }
    }

    bool IsOffroad()
    {
        if (roadAreaIndex == -1) return false;

        if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit, sampleMaxDistance, NavMesh.AllAreas))
            return false;

        int areaIndex = GetAreaIndexFromMask(hit.mask);
        return areaIndex != roadAreaIndex;
    }

    static int GetAreaIndexFromMask(int mask)
    {
        for (int i = 0; i < 32; i++)
        {
            if ((mask & (1 << i)) != 0)
                return i;
        }

        return -1;
    }
}
