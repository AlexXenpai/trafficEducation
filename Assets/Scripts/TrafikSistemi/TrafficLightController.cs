using UnityEngine;

public class TrafficLightController : MonoBehaviour
{
    public enum LightState
    {
        Red,
        Green
    }

    [Header("Current State")]
    public LightState currentState = LightState.Red;

    [Header("Light Objects")]
    public GameObject redLight;
    public GameObject greenLight;

    private void Start()
    {
        ApplyState();
    }

    private void OnValidate()
    {
        ApplyState();
    }

    public bool IsRed()
    {
        return currentState == LightState.Red;
    }

    public bool IsGreen()
    {
        return currentState == LightState.Green;
    }

    public void SetState(LightState newState)
    {
        currentState = newState;
        ApplyState();
    }

    private void ApplyState()
    {
        if (redLight != null)
            redLight.SetActive(currentState == LightState.Red);

        if (greenLight != null)
            greenLight.SetActive(currentState == LightState.Green);
    }
}
