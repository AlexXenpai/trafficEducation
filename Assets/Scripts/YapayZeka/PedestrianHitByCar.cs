using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NPC yayaların arabaya çarpınca düşmesi ve fırlaması için script.
/// </summary>
public class PedestrianHitByCar : MonoBehaviour
{
    [Header("Fırlatma Ayarları")]
    public float hitForce = 15f;        // Yatay fırlatma kuvveti
    public float upwardForce = 8f;      // Yukarı fırlatma kuvveti
    public float torqueForce = 5f;      // Dönme kuvveti
    
    private Rigidbody rb;
    private NavMeshAgent agent;
    private Animator animator;
    private PedestrianAI pedestrianAI;
    private CapsuleCollider capsuleCollider;
    
    private bool isHit = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        pedestrianAI = GetComponent<PedestrianAI>();
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Zaten vurulduysa tekrar işleme
        if (isHit) return;

        // Sadece oyuncu arabası veya AI arabası çarparsa
        if (collision.gameObject.CompareTag("PlayerCar") || 
            collision.gameObject.CompareTag("Car") || 
            collision.gameObject.CompareTag("AI_Araba"))
        {
            HitByVehicle(collision);
        }
    }

    void HitByVehicle(Collision collision)
    {
        isHit = true;
        
        // 1. NavMeshAgent'ı kapat (yoksa yaya havada asılı kalır)
        if (agent != null)
        {
            agent.enabled = false;
        }
        
        // 2. PedestrianAI'ı kapat
        if (pedestrianAI != null)
        {
            pedestrianAI.enabled = false;
        }
        
        // 3. Animator'ı kapat (ragdoll efekti için)
        if (animator != null)
        {
            animator.enabled = false;
        }
        
        // 4. Rigidbody'yi aktif et
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            
            // Rotasyon kısıtlamalarını kaldır (ragdoll gibi dönsün)
            rb.constraints = RigidbodyConstraints.None;
            
            // 5. Fırlatma yönünü hesapla
            Vector3 hitDirection = (transform.position - collision.transform.position).normalized;
            hitDirection.y = 0; // Yatay düzlemde
            
            // Arabanın hızına göre kuvvet ayarla
            Rigidbody carRb = collision.gameObject.GetComponent<Rigidbody>();
            float carSpeed = carRb != null ? carRb.linearVelocity.magnitude : 10f;
            float speedMultiplier = Mathf.Clamp(carSpeed / 10f, 0.5f, 2f);
            
            // 6. Kuvvetleri uygula
            Vector3 force = hitDirection * hitForce * speedMultiplier;
            force.y = upwardForce * speedMultiplier; // Yukarı fırlat
            
            rb.AddForce(force, ForceMode.Impulse);
            
            // 7. Rastgele dönme ekle (daha gerçekçi)
            Vector3 randomTorque = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            ) * torqueForce;
            
            rb.AddTorque(randomTorque, ForceMode.Impulse);
        }
        
        // 8. Collider'ı trigger olmaktan çıkar (fizik etkileşimi için)
        if (capsuleCollider != null)
        {
            capsuleCollider.isTrigger = false;
        }
        
        Debug.Log($"Yaya vuruldu: {gameObject.name}");
    }
    
    /// <summary>
    /// Yayayı sıfırla (respawn için kullanılabilir)
    /// </summary>
    public void ResetPedestrian()
    {
        isHit = false;
        
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        if (agent != null)
        {
            agent.enabled = true;
        }
        
        if (pedestrianAI != null)
        {
            pedestrianAI.enabled = true;
        }
        
        if (animator != null)
        {
            animator.enabled = true;
        }
    }
}
