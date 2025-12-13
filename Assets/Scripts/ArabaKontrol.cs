using UnityEngine;
using UnityEngine.InputSystem; // New Input System kullanacağız

public class ArabaKontrol : MonoBehaviour
{
    [Header("Ayarlar")]
    public float motorForce = 1500f;
    public float breakForce = 3000f;
    public float maxSteerAngle = 30f;

    [Header("Wheel Colliders")]
    public WheelCollider frontLeftCollider;
    public WheelCollider frontRightCollider;
    public WheelCollider rearLeftCollider;
    public WheelCollider rearRightCollider;

    [Header("Wheel Transforms (Görsellik İçin)")]
    public Transform frontLeftTransform;
    public Transform frontRightTransform;
    public Transform rearLeftTransform;
    public Transform rearRightTransform;

    // Input Değerleri
    private float currentSteerAngle;
    private float currentbreakForce;
    private Vector2 inputVector;

    // XR Input Referansı (Bunu editörden bağlayacaksın)
    public InputActionProperty steeringInput;

    private void FixedUpdate()
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }

    private void GetInput()
    {
        // 1. Önce XR Kontrolcüsünden veriyi okumayı dene
        Vector2 xrInput = Vector2.zero;

        // Action null değilse ve aktifse oku
        if (steeringInput.action != null && steeringInput.action.enabled)
        {
            xrInput = steeringInput.action.ReadValue<Vector2>();
        }

        // 2. Eğer XR'dan veri gelmiyorsa (veya kask yoksa), Klavyeye (WASD) bak
        if (xrInput == Vector2.zero)
        {
            xrInput.x = Input.GetAxis("Horizontal"); // A-D veya Sol-Sağ ok
            xrInput.y = Input.GetAxis("Vertical");   // W-S veya Yukarı-Aşağı ok
        }

        // Sonucu genel değişkene ata
        inputVector = xrInput;
    }

    private void HandleMotor()
    {
        // Y ekseni (ileri/geri) gaz ve freni kontrol eder
        float acceleration = inputVector.y * motorForce;

        // Basit bir 4x4 mantığı
        frontLeftCollider.motorTorque = acceleration;
        frontRightCollider.motorTorque = acceleration;
        rearLeftCollider.motorTorque = acceleration;
        rearRightCollider.motorTorque = acceleration;

        // Eğer input yoksa otomatik fren (sürtünme simülasyonu)
        currentbreakForce = inputVector.y == 0 ? 100f : 0f;
        ApplyBrake();
    }

    private void ApplyBrake()
    {
        frontLeftCollider.brakeTorque = currentbreakForce;
        frontRightCollider.brakeTorque = currentbreakForce;
        rearLeftCollider.brakeTorque = currentbreakForce;
        rearRightCollider.brakeTorque = currentbreakForce;
    }

    private void HandleSteering()
    {
        // X ekseni (sağ/sol) direksiyonu kontrol eder
        currentSteerAngle = maxSteerAngle * inputVector.x;
        frontLeftCollider.steerAngle = currentSteerAngle;
        frontRightCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftCollider, frontLeftTransform);
        UpdateSingleWheel(frontRightCollider, frontRightTransform);
        UpdateSingleWheel(rearLeftCollider, rearLeftTransform);
        UpdateSingleWheel(rearRightCollider, rearRightTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }
}