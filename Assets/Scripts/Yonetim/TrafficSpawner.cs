using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficSpawner : MonoBehaviour
{
    public GameObject[] Cars;

    [Header("Spawn Settings")]
    [Range(0, 100)]
    public int spawnPercentage = 30; // Varsayýlan %30 doluluk oraný. Bunu Inspector'dan ayarla.

    void Start()
    {
        spawn();
    }

    void spawn()
    {
        // Arabalar listesi boþsa hata vermesin, direkt çýksýn.
        if (Cars == null || Cars.Length == 0)
        {
            Debug.LogError("SpawnCars: Araba listesi boþ! Inspector'dan araba prefablarýný ekle.");
            return;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            // ZAR ATMA ANI:
            // 0 ile 100 arasýnda rastgele sayý tut. Eðer belirlenen yüzdeden büyükse, bu noktayý pas geç.
            if (Random.Range(0, 101) > spawnPercentage)
                continue;

            // Rastgele bir araba seç
            int ram = Random.Range(0, Cars.Length);

            // Arabayý oluþturmadan önce referanslarý hazýrla
            // Not: Instantiate etmeden önce orijinal prefab üzerinde deðiþiklik yapamayýz, 
            // bu yüzden önce objeyi yaratýp sonra ayar yapýyoruz.
            GameObject newCar = Instantiate(Cars[ram], transform.GetChild(i).position, transform.GetChild(i).rotation);

            AICar carAI = newCar.GetComponent<AICar>();

            if (carAI != null)
            {
                carAI.currentTrafficRoute = this.gameObject;
                carAI.currentWapointNumber = i;
            }
            else
            {
                Debug.LogWarning("Spawnlanan arabada 'Car_AI' scripti yok! Araba aptal gibi duracak.");
            }
        }
    }
}