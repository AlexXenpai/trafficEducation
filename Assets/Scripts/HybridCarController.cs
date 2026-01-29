using UnityEngine;
using UnityEngine.InputSystem;

public class HybridCarController : MonoBehaviour
{
    [Header("Wheel Colliders")]
    public WheelCollider frontLeft;
    public WheelCollider frontRight;
    public WheelCollider rearLeft;
    public WheelCollider rearRight;

    [Header("Motor Settings")]
    public float motorForce = 1500f;
    public float maxSteerAngle = 30f;
    
    [Header("Stability Settings")]
    public float centerOfMassYOffset = -0.5f; // Ağırlık merkezini aşağı çek
    public bool preventRollover = true; // Takla atmayı önle
    public float maxRollAngle = 45f; // Maksimum yatma açısı
    public float stabilizationForce = 5000f; // Dengeleme kuvveti
    
    [Header("XR Input")]
    public InputActionProperty xrMoveInput;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (rb != null)
        {
            // Ağırlık merkezini aşağı çek - takla atmayı zorlaştırır
            rb.centerOfMass = new Vector3(0, centerOfMassYOffset, 0);
            
            // Angular drag'ı artır - ani dönüşlerde daha stabil
            rb.angularDamping = 2f;
        }
    }

    private void OnEnable()
    {
        if (xrMoveInput.action != null) xrMoveInput.action.Enable();
    }

    private void OnDisable()
    {
        if (xrMoveInput.action != null) xrMoveInput.action.Disable();
    }

    void FixedUpdate()
    {
        float v = 0;
        float h = 0;

        // Keyboard Input
        try
        {
            v = Input.GetAxis("Vertical");
            h = Input.GetAxis("Horizontal");
        }
        catch (System.Exception)
        {
            // Fallback if Legacy Input is disabled
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) v = 1;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) v = -1;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) h = -1;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h = 1;
            }
        }

        // XR Input Override
        if (xrMoveInput.action != null)
        {
            Vector2 xrVal = xrMoveInput.action.ReadValue<Vector2>();
            if (xrVal.sqrMagnitude > 0.1f)
            {
                h = xrVal.x;
                v = xrVal.y;
            }
        }

        float steer = h * maxSteerAngle;
        float motor = v * motorForce;

        if (frontLeft) { frontLeft.steerAngle = steer; frontLeft.motorTorque = motor; }
        if (frontRight) { frontRight.steerAngle = steer; frontRight.motorTorque = motor; }
        if (rearLeft) { rearLeft.motorTorque = motor; }
        if (rearRight) { rearRight.motorTorque = motor; }

        // Takla atmayı önle
        if (preventRollover)
        {
            PreventRollover();
        }
    }

    /// <summary>
    /// Arabanın takla atmasını önler
    /// </summary>
    void PreventRollover()
    {
        if (rb == null) return;

        // Mevcut rotasyonu al
        Vector3 currentRotation = transform.eulerAngles;
        
        // Açıları -180 ile 180 arasına normalize et
        float xAngle = NormalizeAngle(currentRotation.x);
        float zAngle = NormalizeAngle(currentRotation.z);

        // Eğer araba çok yattıysa düzelt
        bool needsStabilization = Mathf.Abs(xAngle) > maxRollAngle || Mathf.Abs(zAngle) > maxRollAngle;

        if (needsStabilization)
        {
            // Dengeleme kuvveti uygula
            Vector3 stabilizationTorque = Vector3.zero;

            if (Mathf.Abs(xAngle) > maxRollAngle)
            {
                stabilizationTorque.x = -xAngle * stabilizationForce * Time.fixedDeltaTime;
            }

            if (Mathf.Abs(zAngle) > maxRollAngle)
            {
                stabilizationTorque.z = -zAngle * stabilizationForce * Time.fixedDeltaTime;
            }

            rb.AddRelativeTorque(stabilizationTorque, ForceMode.Force);
        }

        // Aşırı yatma durumunda zorla düzelt
        if (Mathf.Abs(xAngle) > 60f || Mathf.Abs(zAngle) > 60f)
        {
            // Rotasyonu yumuşak bir şekilde düzelt
            Quaternion targetRotation = Quaternion.Euler(0, currentRotation.y, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 3f);
            
            // Angular velocity'yi azalt
            rb.angularVelocity = rb.angularVelocity * 0.9f;
        }
    }

    /// <summary>
    /// Açıyı -180 ile 180 arasına normalize eder
    /// </summary>
    float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }
}
