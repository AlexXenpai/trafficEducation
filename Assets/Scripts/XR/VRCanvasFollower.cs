using UnityEngine;

/// <summary>
/// VR'da Canvas'ın kullanıcının önünde kalmasını sağlar.
/// Canvas her zaman kullanıcının baktığı yönde, belirli bir mesafede durur.
/// </summary>
public class VRCanvasFollower : MonoBehaviour
{
    [Header("Takip Ayarları")]
    [Tooltip("Takip edilecek kamera (boş bırakılırsa otomatik bulunur)")]
    public Transform targetCamera;
    
    [Tooltip("Kameradan uzaklık (metre)")]
    public float distanceFromCamera = 2.5f;
    
    [Tooltip("Kameradan yükseklik ofseti (metre)")]
    public float heightOffset = 0f;
    
    [Tooltip("Pozisyon takip hızı")]
    public float positionSmoothSpeed = 5f;
    
    [Tooltip("Rotasyon takip hızı")]
    public float rotationSmoothSpeed = 5f;
    
    [Header("Takip Modu")]
    [Tooltip("Sadece yatay rotasyonu takip et (baş eğmelerini yoksay)")]
    public bool horizontalOnly = true;
    
    [Tooltip("Sadece belirli açı farkında güncelle (derece)")]
    public float updateAngleThreshold = 30f;
    
    [Tooltip("Anlık takip (smooth yok)")]
    public bool instantFollow = false;
    
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float lastUpdateAngle;
    
    void Start()
    {
        // Kamerayı otomatik bul
        if (targetCamera == null)
        {
            // XR Origin'deki Main Camera'yı bul
            var xrOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin != null && xrOrigin.Camera != null)
            {
                targetCamera = xrOrigin.Camera.transform;
                Debug.Log($"VRCanvasFollower: XR Camera bulundu - {targetCamera.name}");
            }
            else
            {
                // Fallback: Main Camera tag'li kamerayı bul
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    targetCamera = mainCam.transform;
                    Debug.Log($"VRCanvasFollower: Main Camera bulundu - {targetCamera.name}");
                }
            }
        }
        
        if (targetCamera == null)
        {
            Debug.LogError("VRCanvasFollower: Kamera bulunamadı!");
            enabled = false;
            return;
        }
        
        // Başlangıç pozisyonunu ayarla
        UpdateTargetTransform();
        if (instantFollow)
        {
            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }
    }
    
    void LateUpdate()
    {
        if (targetCamera == null) return;
        
        // Açı farkını kontrol et
        float currentAngle = horizontalOnly ? targetCamera.eulerAngles.y : targetCamera.eulerAngles.y;
        float angleDiff = Mathf.Abs(Mathf.DeltaAngle(lastUpdateAngle, currentAngle));
        
        // Eşik değerini aştıysa veya her zaman güncelle
        if (angleDiff > updateAngleThreshold || updateAngleThreshold <= 0)
        {
            UpdateTargetTransform();
            lastUpdateAngle = currentAngle;
        }
        
        // Pozisyon ve rotasyonu güncelle
        if (instantFollow)
        {
            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionSmoothSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothSpeed);
        }
    }
    
    void UpdateTargetTransform()
    {
        if (targetCamera == null) return;
        
        // Kameranın forward yönünü al
        Vector3 forward = targetCamera.forward;
        
        if (horizontalOnly)
        {
            // Sadece yatay düzlemde (Y ekseninde dönüş)
            forward.y = 0;
            forward.Normalize();
            
            if (forward.sqrMagnitude < 0.001f)
            {
                forward = Vector3.forward;
            }
        }
        
        // Hedef pozisyonu hesapla
        targetPosition = targetCamera.position + forward * distanceFromCamera;
        targetPosition.y = targetCamera.position.y + heightOffset;
        
        // Hedef rotasyonu hesapla (kameraya baksın)
        Vector3 lookDirection = targetPosition - targetCamera.position;
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            targetRotation = Quaternion.LookRotation(lookDirection);
        }
    }
    
    /// <summary>
    /// Canvas'ı anında kullanıcının önüne getir
    /// </summary>
    public void SnapToFront()
    {
        UpdateTargetTransform();
        transform.position = targetPosition;
        transform.rotation = targetRotation;
        lastUpdateAngle = horizontalOnly ? targetCamera.eulerAngles.y : targetCamera.eulerAngles.y;
    }
}
