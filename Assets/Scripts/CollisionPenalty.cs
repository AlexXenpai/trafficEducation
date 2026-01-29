using UnityEngine;

/// <summary>
/// Çarpışma ceza sistemi.
/// Hem araba modu hem de yaya modu için çalışır.
/// </summary>
public class CollisionPenalty : MonoBehaviour
{
    [Header("Araba Modu Cezaları")]
    public int buildingPenalty = 10;
    public int carPenalty = 10;
    public int pedestrianPenalty = 20; // Araba ile yayaya çarpma
    
    [Header("Yaya Modu Cezaları")]
    public int hitOtherPedestrianPenalty = 10; // Yaya ile yayaya çarpma
    
    [Header("Mesajlar")]
    public string buildingMessage = "Binaya Çarptınız!";
    public string carMessage = "Arabaya Çarptınız!";
    public string pedestrianMessage = "Yayaya Çarptınız!";
    public string hitOtherPedestrianMessage = "Başka bir yayaya çarptınız!";

    // Spam önleme için cooldown
    private float lastPenaltyTime = 0f;
    private float penaltyCooldown = 1f; // 1 saniye cooldown

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }

    private void HandleCollision(GameObject hitObject)
    {
        // Cooldown kontrolü - spam önleme
        if (Time.time - lastPenaltyTime < penaltyCooldown)
            return;

        // Kendimize çarpmayı yoksay
        if (hitObject == gameObject)
            return;
            
        // Çocuk objelere çarpmayı yoksay
        if (hitObject.transform.IsChildOf(transform))
            return;

        // Bu obje oyuncu yayası mı?
        bool isPlayerPedestrian = GetComponent<PedestrianController>() != null;
        
        // Bu obje oyuncu arabası mı?
        bool isPlayerCar = CompareTag("PlayerCar");

        // 1. BINA CARPMASI
        bool isBuilding = false;
        if (hitObject.transform.parent != null)
        {
            Transform parent = hitObject.transform.parent;
            if (parent.name == "Buildings")
            {
                isBuilding = true;
            }
            else if (parent.parent != null && parent.parent.name == "Buildings")
            {
                isBuilding = true;
            }
        }
        
        if (isBuilding)
        {
            ApplyPenalty(buildingPenalty, buildingMessage);
            return;
        }

        // 2. ARABA CARPMASI
        if (hitObject.CompareTag("Car") || hitObject.CompareTag("AI_Araba") || hitObject.CompareTag("PlayerCar"))
        {
            // Kendi arabamıza çarpmayı yoksay
            if (hitObject.CompareTag("PlayerCar") && isPlayerPedestrian)
            {
                // Oyuncu yayası kendi arabasına çarptı - ceza yok
                return;
            }
            
            ApplyPenalty(carPenalty, carMessage);
            return;
        }

        // 3. YAYA CARPMASI
        // AI yayası mı kontrol et
        bool isAIPedestrian = hitObject.CompareTag("Pedestrian") || 
                              hitObject.GetComponent<PedestrianAI>() != null;
        
        // Oyuncu yayası mı kontrol et (çarpılan obje)
        bool hitPlayerPedestrian = hitObject.GetComponent<PedestrianController>() != null;
        
        if (isAIPedestrian && !hitPlayerPedestrian)
        {
            if (isPlayerCar)
            {
                // Araba ile yayaya çarpma - 20 puan
                ApplyPenalty(pedestrianPenalty, pedestrianMessage);
            }
            else if (isPlayerPedestrian)
            {
                // Yaya ile yayaya çarpma - 10 puan
                ApplyPenalty(hitOtherPedestrianPenalty, hitOtherPedestrianMessage);
            }
        }
    }
    
    private void ApplyPenalty(int amount, string message)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CezaVer(amount, message);
            lastPenaltyTime = Time.time;
            Debug.Log($"CEZA: {message} (-{amount} puan)");
        }
    }
}
