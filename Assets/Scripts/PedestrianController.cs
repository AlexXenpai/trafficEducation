using UnityEngine;

/// <summary>
/// VR uyumlu yaya kontrol scripti.
/// - Baktığın yöne doğru döner ve yürür
/// - Hareket ederken yürüme animasyonu oynar
/// - VR kafa takibi ile senkronize çalışır
/// </summary>
public class PedestrianController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 10f;
    
    [Header("Referanslar")]
    public Transform cameraTransform; // VR kamera (Main Camera)
    
    private Rigidbody rb;
    private Animator animator;
    private bool isMoving = false;
    
    // Animator parametreleri
    private static readonly int SpeedParam = Animator.StringToHash("Speed");
    private static readonly int IsWalkingParam = Animator.StringToHash("IsWalking");
    private static readonly int WalkParam = Animator.StringToHash("Walk");

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        
        // Rigidbody ayarları
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        
        // Kamerayı otomatik bul
        if (cameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                cameraTransform = mainCam.transform;
            }
        }
    }

    // VR Input
    public UnityEngine.InputSystem.InputActionProperty moveInput;

    void OnEnable()
    {
        if (moveInput.action != null) moveInput.action.Enable();
    }

    void OnDisable()
    {
        if (moveInput.action != null) moveInput.action.Disable();
    }

    void Update()
    {
        // Sadece aktifken çalış
        if (!gameObject.activeInHierarchy) return;
        
        Vector2 input = Vector2.zero;
        if (moveInput.action != null)
        {
            input = moveInput.action.ReadValue<Vector2>();
        }
        else
        {
            // Fallback to legacy input if action is not set
            input.x = Input.GetAxis("Horizontal");
            input.y = Input.GetAxis("Vertical");
        }
        
        isMoving = input.sqrMagnitude > 0.01f;
        
        // Animasyonu güncelle
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        if (!gameObject.activeInHierarchy) return;
        if (rb == null) return;
        
        // 1. Hareket (Joystick)
        Vector2 input = Vector2.zero;
        if (moveInput.action != null)
        {
            input = moveInput.action.ReadValue<Vector2>();
        }
        else
        {
            input.x = Input.GetAxis("Horizontal");
            input.y = Input.GetAxis("Vertical");
        }
        
        Vector3 inputDirection = new Vector3(input.x, 0f, input.y);
        
        if (inputDirection.sqrMagnitude > 0.01f)
        {
            // Kameranın baktığı yöne göre hareket yönünü hesapla
            Vector3 moveDirection = GetMoveDirection(inputDirection);
            
            // Karakteri hareket yönüne döndür (Sadece hareket ederken)
            // RotateTowardsDirection(moveDirection); // Bunu iptal ediyoruz, kafa nereye bakarsa oraya dönsün
            
            // Hareketi uygula
            Vector3 movement = moveDirection * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + movement);
        }

        // 2. Rotasyon (Kafa Takibi)
        // Karakterin vücudunu kameranın baktığı yöne (Y ekseninde) döndür
        if (cameraTransform != null)
        {
            Vector3 lookDir = cameraTransform.forward;
            lookDir.y = 0; // Sadece yatay düzlemde dön
            if (lookDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                // Yumuşak dönüş
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
            }
        }
    }
    
    /// <summary>
    /// Kameranın baktığı yöne göre hareket yönünü hesaplar
    /// </summary>
    Vector3 GetMoveDirection(Vector3 input)
    {
        if (cameraTransform == null)
        {
            return input.normalized;
        }
        
        // Kameranın forward ve right vektörlerini al (Y eksenini sıfırla)
        Vector3 camForward = cameraTransform.forward;
        camForward.y = 0;
        camForward.Normalize();
        
        Vector3 camRight = cameraTransform.right;
        camRight.y = 0;
        camRight.Normalize();
        
        // Girişe göre hareket yönünü hesapla
        Vector3 moveDirection = (camForward * input.z + camRight * input.x).normalized;
        
        return moveDirection;
    }
    
    /// <summary>
    /// Karakteri belirtilen yöne yumuşak bir şekilde döndürür
    /// </summary>
    void RotateTowardsDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.01f) return;
        
        // Hedef rotasyonu hesapla
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        
        // Yumuşak dönüş
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            targetRotation, 
            rotationSpeed * Time.fixedDeltaTime
        );
    }
    
    /// <summary>
    /// Animator'ı günceller - yürüme/durma animasyonları
    /// </summary>
    void UpdateAnimation()
    {
        if (animator == null) return;
        
        // Farklı parametre isimlerini dene (Animator Controller'a bağlı)
        // Speed parametresi (float)
        if (HasParameter(SpeedParam))
        {
            animator.SetFloat(SpeedParam, isMoving ? 1f : 0f);
        }
        
        // IsWalking parametresi (bool)
        if (HasParameter(IsWalkingParam))
        {
            animator.SetBool(IsWalkingParam, isMoving);
        }
        
        // Walk parametresi (bool)
        if (HasParameter(WalkParam))
        {
            animator.SetBool(WalkParam, isMoving);
        }
    }
    
    /// <summary>
    /// Animator'da belirtilen parametre var mı kontrol eder
    /// </summary>
    bool HasParameter(int paramHash)
    {
        if (animator == null) return false;
        
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.nameHash == paramHash)
                return true;
        }
        return false;
    }
    
    /// <summary>
    /// Kamera referansını ayarlar (ModSecimi tarafından çağrılabilir)
    /// </summary>
    public void SetCameraReference(Transform cam)
    {
        cameraTransform = cam;
    }
}
