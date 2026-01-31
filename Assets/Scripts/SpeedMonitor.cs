using UnityEngine;
using TMPro;
using System.Collections;

public class SpeedMonitor : MonoBehaviour
{
    [Header("Settings")]
    public float defaultSpeedLimit = 50f;
    public float penaltyCheckInterval = 1.0f; // Check every second
    public float penaltyMultiplier = 0.5f; // Penalty = ExcessSpeed * Multiplier
    public float speedTolerance = 5f; // Buffer before penalty

    [Header("UI References")]
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI limitText;
    public GameObject warningIcon;

    private float currentSpeedLimit;
    private float currentSpeed;
    private Rigidbody rb;
    private float lastPenaltyTime;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentSpeedLimit = defaultSpeedLimit;
        
        if (warningIcon != null) warningIcon.SetActive(false);
        UpdateUI();
    }

    private void Update()
    {
        CalculateSpeed();
        CheckSpeedLimit();
        UpdateUI();
    }

    private void CalculateSpeed()
    {
        if (rb != null)
        {
            // Convert m/s to km/h
            currentSpeed = rb.linearVelocity.magnitude * 3.6f;
        }
    }

    private void CheckSpeedLimit()
    {
        float excessSpeed = currentSpeed - currentSpeedLimit;

        if (excessSpeed > speedTolerance)
        {
            // Speeding
            if (warningIcon != null) warningIcon.SetActive(true);

            if (Time.time - lastPenaltyTime > penaltyCheckInterval)
            {
                ApplyPenalty(excessSpeed);
                lastPenaltyTime = Time.time;
            }
        }
        else
        {
            // Safe
            if (warningIcon != null) warningIcon.SetActive(false);
        }
    }

    private void ApplyPenalty(float excess)
    {
        int penalty = Mathf.RoundToInt(excess * penaltyMultiplier);
        if (penalty < 1) penalty = 1;

        string message = $"Hız Limiti Aşıldı! ({Mathf.RoundToInt(currentSpeed)}/{currentSpeedLimit})";
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CezaVer(penalty, message);
        }
    }

    private void UpdateUI()
    {
        if (speedText != null)
        {
            speedText.text = Mathf.RoundToInt(currentSpeed).ToString();
            
            // Change color if speeding
            if (currentSpeed > currentSpeedLimit)
                speedText.color = Color.red;
            else
                speedText.color = Color.white;
        }

        if (limitText != null)
        {
            limitText.text = Mathf.RoundToInt(currentSpeedLimit).ToString();
        }
    }

    public void SetCurrentLimit(float limit)
    {
        currentSpeedLimit = limit;
    }

    public void ClearLimit()
    {
        currentSpeedLimit = defaultSpeedLimit;
    }
}
