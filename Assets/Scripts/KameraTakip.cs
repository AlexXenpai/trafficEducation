using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

/// <summary>
/// XR Origin'i (kamera sistemini) araba veya yaya moduna göre konumlandırır.
/// Bu script XR Origin objesine eklenir.
/// </summary>
public class KameraTakip : MonoBehaviour
{
    [Header("Hedefler")]
    public Transform carTarget;
    public Transform pedestrianTarget;

    [Header("Araba Modu Ayarları")]
    public Vector3 carOffset = new Vector3(0, 2.5f, -8f);

    [Tooltip("Arabanın pitch/roll zıplamasını kameraya yansıtmamak için offset sadece YAW (yatay yön) ile uygulanır.")]
    public bool useYawOnlyForCarOffset = true;

    [Range(0.01f, 1f)]
    public float positionSmoothTime = 0.15f;
    [Range(0.01f, 1f)]
    public float rotationSmoothTime = 0.1f;

    [Header("Yaya Modu Ayarları")]
    public Vector3 pedestrianOffset = new Vector3(0, 1.6f, 0); // Göz seviyesi
    public bool lockYawToPedestrian = false; // Yayanın yönüne kilitle

    [Header("Durum")]
    public bool isCarMode = true;

    // SmoothDamp için velocity değişkenleri
    private Vector3 positionVelocity = Vector3.zero;
    private float rotationVelocity = 0f;

    private bool initialized = false;
    
    // Locomotion bileşenleri (araba modunda devre dışı bırakılacak)
    private ActionBasedContinuousMoveProvider moveProvider;
    private ActionBasedSnapTurnProvider snapTurnProvider;
    private ActionBasedContinuousTurnProvider continuousTurnProvider;
    private CharacterController characterController;
    private GravityProvider gravityProvider;
    private XRLocomotionBootstrap locomotionBootstrap;
    
    // Yeni locomotion sistemi bileşenleri
    private ContinuousMoveProvider continuousMoveProvider;
    private SnapTurnProvider newSnapTurnProvider;
    private ContinuousTurnProvider newContinuousTurnProvider;

    void Start()
    {
        // Locomotion bileşenlerini bul (eski sistem)
        moveProvider = GetComponent<ActionBasedContinuousMoveProvider>();
        snapTurnProvider = GetComponent<ActionBasedSnapTurnProvider>();
        continuousTurnProvider = GetComponent<ActionBasedContinuousTurnProvider>();
        characterController = GetComponent<CharacterController>();
        locomotionBootstrap = GetComponent<XRLocomotionBootstrap>();
        
        // Child'larda ara
        gravityProvider = GetComponentInChildren<GravityProvider>();
        
        // Yeni locomotion sistemi (Locomotion child'ında)
        continuousMoveProvider = GetComponentInChildren<ContinuousMoveProvider>();
        newSnapTurnProvider = GetComponentInChildren<SnapTurnProvider>();
        newContinuousTurnProvider = GetComponentInChildren<ContinuousTurnProvider>();
        
        if (carTarget != null)
        {
            SetCarMode();
        }
    }

    void LateUpdate()
    {
        if (isCarMode)
        {
            if (carTarget == null) return;
            FollowCar();
        }
        else
        {
            if (pedestrianTarget == null) return;
            FollowPedestrian();
        }
    }

    void FollowCar()
    {
        Vector3 targetPosition;

        if (useYawOnlyForCarOffset)
        {
            Vector3 flatForward = carTarget.forward;
            flatForward.y = 0f;
            if (flatForward.sqrMagnitude < 0.0001f)
                flatForward = Vector3.forward;

            Quaternion yawRotation = Quaternion.LookRotation(flatForward.normalized, Vector3.up);
            targetPosition = carTarget.position + (yawRotation * carOffset);
        }
        else
        {
            // Eski davranış: offset'i aracın tam rotasyonuyla uygular (pitch/roll zıplatabilir)
            targetPosition = carTarget.position + carTarget.TransformDirection(carOffset);
        }

        if (!initialized)
        {
            transform.position = targetPosition;

            Vector3 lookDir = carTarget.position - transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(lookDir);
            }

            initialized = true;
            positionVelocity = Vector3.zero;
            return;
        }

        // Pozisyon: SmoothDamp
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            targetPosition, 
            ref positionVelocity, 
            positionSmoothTime
        );

        // Rotasyon: Arabaya bak
        Vector3 lookDirection = carTarget.position - transform.position;
        lookDirection.y = 0;
        
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            
            float currentAngle = transform.eulerAngles.y;
            float targetAngle = targetRotation.eulerAngles.y;
            
            float smoothedAngle = Mathf.SmoothDampAngle(
                currentAngle, 
                targetAngle, 
                ref rotationVelocity, 
                rotationSmoothTime
            );
            
            transform.rotation = Quaternion.Euler(0, smoothedAngle, 0);
        }
    }

    void FollowPedestrian()
    {
        // Yaya modunda: XR Origin'i yayanın pozisyonuna taşı
        // Offset ile göz seviyesine getir
        Vector3 targetPosition = pedestrianTarget.position + pedestrianOffset;
        
        // Direkt pozisyon ata (1. şahıs için smooth gerekmiyor)
        transform.position = targetPosition;
        
        // Rotasyonu serbest bırak - VR'da kullanıcı kafasını çevirebilir
        // Sadece istenirse yayanın yönüne kilitle
        if (lockYawToPedestrian)
        {
            transform.rotation = Quaternion.Euler(0, pedestrianTarget.eulerAngles.y, 0);
        }
    }

    public void SetCarMode()
    {
        isCarMode = true;
        initialized = false;
        positionVelocity = Vector3.zero;
        rotationVelocity = 0f;
        
        // Araba modunda locomotion'ı devre dışı bırak
        DisableLocomotion();
        
        Debug.Log("Kamera: Araba Moduna geçildi");
    }

    public void SetPedestrianMode()
    {
        isCarMode = false;
        initialized = false;
        
        // Yaya modunda locomotion'ı etkinleştir
        EnableLocomotion();
        
        // Yaya moduna geçerken hemen konumlan
        if (pedestrianTarget != null)
        {
            transform.position = pedestrianTarget.position + pedestrianOffset;
        }
        
        Debug.Log("Kamera: Yaya Moduna geçildi");
    }
    
    private void DisableLocomotion()
    {
        // Eski sistem - Move provider'ı devre dışı bırak
        if (moveProvider != null)
            moveProvider.enabled = false;
        
        // Eski sistem - Snap turn provider'ı devre dışı bırak
        if (snapTurnProvider != null)
            snapTurnProvider.enabled = false;
            
        // Eski sistem - Continuous turn provider'ı devre dışı bırak
        if (continuousTurnProvider != null)
            continuousTurnProvider.enabled = false;
        
        // Character controller'ı devre dışı bırak
        if (characterController != null)
            characterController.enabled = false;
        
        // Gravity provider'ı devre dışı bırak
        if (gravityProvider != null)
            gravityProvider.enabled = false;
            
        // Locomotion bootstrap'ı devre dışı bırak
        if (locomotionBootstrap != null)
            locomotionBootstrap.enabled = false;
            
        // Yeni sistem - Move provider
        if (continuousMoveProvider != null)
            continuousMoveProvider.enabled = false;
            
        // Yeni sistem - Snap turn
        if (newSnapTurnProvider != null)
            newSnapTurnProvider.enabled = false;
            
        // Yeni sistem - Continuous turn
        if (newContinuousTurnProvider != null)
            newContinuousTurnProvider.enabled = false;
        
        Debug.Log("Locomotion devre dışı bırakıldı (Araba modu)");
    }
    
    private void EnableLocomotion()
    {
        // Eski sistem - Move provider'ı etkinleştir
        if (moveProvider != null)
            moveProvider.enabled = true;
        
        // Yaya modunda SNAP TURN KAPALI - sadece smooth/continuous turn
        if (snapTurnProvider != null)
            snapTurnProvider.enabled = false;
            
        // Yaya modunda CONTINUOUS TURN AÇIK - smooth dönüş
        if (continuousTurnProvider != null)
            continuousTurnProvider.enabled = true;
        
        // Character controller'ı etkinleştir
        if (characterController != null)
            characterController.enabled = true;
        
        // Gravity provider'ı etkinleştir
        if (gravityProvider != null)
            gravityProvider.enabled = true;
            
        // Locomotion bootstrap'ı etkinleştir
        if (locomotionBootstrap != null)
            locomotionBootstrap.enabled = true;
            
        // Yeni sistem - Move provider
        if (continuousMoveProvider != null)
            continuousMoveProvider.enabled = true;
            
        // Yeni sistem - Snap turn KAPALI
        if (newSnapTurnProvider != null)
            newSnapTurnProvider.enabled = false;
            
        // Yeni sistem - Continuous turn AÇIK
        if (newContinuousTurnProvider != null)
            newContinuousTurnProvider.enabled = true;
        
        Debug.Log("Locomotion etkinleştirildi (Yaya modu - Smooth Turn)");
    }

    public void SetMode(bool carMode)
    {
        if (carMode)
            SetCarMode();
        else
            SetPedestrianMode();
    }

    public void SetTargets(Transform car, Transform pedestrian)
    {
        carTarget = car;
        pedestrianTarget = pedestrian;
    }
}
