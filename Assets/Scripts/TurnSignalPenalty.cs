using UnityEngine;

public class TurnSignalPenalty : MonoBehaviour
{
    [Header("References")]
    public CarSignalSystem signalSystem;
    public Transform carTransform;

    [Header("Settings")]
    public float turnThresholdAngle = 30f; // Degrees to consider a turn
    public float checkInterval = 0.1f; // How often to check rotation delta
    public int penaltyAmount = 10;
    public string penaltyMessage = "Sinyal Vermeden Dönüş!";

    private float lastCheckTime;
    private float lastPenaltyTime;
    private float accumulatedRotationY = 0f;
    private float lastYRotation;

    private void Start()
    {
        if (carTransform == null) carTransform = transform;
        if (signalSystem == null) signalSystem = GetComponent<CarSignalSystem>();

        lastYRotation = carTransform.eulerAngles.y;
        lastCheckTime = Time.time;
    }

    private void Update()
    {
        if (Time.time - lastCheckTime > checkInterval)
        {
            CheckTurn();
            lastCheckTime = Time.time;
        }
    }

    private void CheckTurn()
    {
        float currentY = carTransform.eulerAngles.y;
        float delta = Mathf.DeltaAngle(lastYRotation, currentY);
        lastYRotation = currentY;

        // Accumulate rotation
        accumulatedRotationY += delta;

        // Decay accumulation over time (simulating "straightening out" or time passing)
        // If we don't turn enough quickly, it fades away
        accumulatedRotationY = Mathf.Lerp(accumulatedRotationY, 0, Time.deltaTime * 2f);

        // Check thresholds
        if (Mathf.Abs(accumulatedRotationY) > turnThresholdAngle)
        {
            // Turning Right (Positive rotation)
            if (accumulatedRotationY > 0)
            {
                if (signalSystem != null && !signalSystem.IsRightSignalOn)
                {
                    ApplyPenalty("Sağa Sinyal Vermeden Dönüş!");
                }
            }
            // Turning Left (Negative rotation)
            else
            {
                if (signalSystem != null && !signalSystem.IsLeftSignalOn)
                {
                    ApplyPenalty("Sola Sinyal Vermeden Dönüş!");
                }
            }
            
            // Reset accumulation after detection to prevent spamming
            accumulatedRotationY = 0;
        }
    }

    private void ApplyPenalty(string message)
    {
        // Cooldown to prevent multiple penalties for the same turn
        if (Time.time - lastPenaltyTime < 5f) return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CezaVer(penaltyAmount, message);
            lastPenaltyTime = Time.time;
        }
    }
}
