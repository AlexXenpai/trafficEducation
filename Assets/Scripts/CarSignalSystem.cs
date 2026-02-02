using UnityEngine;
using System.Collections;

public class CarSignalSystem : MonoBehaviour
{
    [Header("Signal Lights")]
    public Renderer frontLeftLight;
    public Renderer frontRightLight;
    public Renderer rearLeftLight;
    public Renderer rearRightLight;

    [Header("Settings")]
    public float blinkInterval = 0.4f;
    public Material offMaterial;
    public Material onMaterial;

    private bool isLeftSignalOn = false;
    private bool isRightSignalOn = false;
    private Coroutine blinkCoroutine;

    // Public properties to check state
    public bool IsLeftSignalOn => isLeftSignalOn;
    public bool IsRightSignalOn => isRightSignalOn;

    private void Start()
    {
        // Ensure lights are off at start
        TurnOffAllLights();
    }

    public void ToggleLeftSignal()
    {
        if (isLeftSignalOn)
        {
            StopSignal();
        }
        else
        {
            isLeftSignalOn = true;
            isRightSignalOn = false; // Turn off right if it was on
            StartBlinking();
        }
    }

    public void ToggleRightSignal()
    {
        Debug.Log("ToggleRightSignal called");
        if (isRightSignalOn)
        {
            StopSignal();
        }
        else
        {
            isRightSignalOn = true;
            isLeftSignalOn = false; // Turn off left if it was on
            StartBlinking();
        }
        Debug.Log("ToggleRightSignal finished");
    }

    public void StopSignal()
    {
        isLeftSignalOn = false;
        isRightSignalOn = false;
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        TurnOffAllLights();
    }

    private void StartBlinking()
    {
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        bool isOn = true;
        while (isLeftSignalOn || isRightSignalOn)
        {
            SetLights(isOn);
            yield return new WaitForSeconds(blinkInterval);
            isOn = !isOn;
        }
        TurnOffAllLights();
    }

    private void SetLights(bool active)
    {
        Material mat = active ? onMaterial : offMaterial;

        if (isLeftSignalOn)
        {
            if (frontLeftLight) frontLeftLight.material = mat;
            if (rearLeftLight) rearLeftLight.material = mat;
            
            // Ensure right side is off
            if (frontRightLight) frontRightLight.material = offMaterial;
            if (rearRightLight) rearRightLight.material = offMaterial;
        }
        else if (isRightSignalOn)
        {
            if (frontRightLight) frontRightLight.material = mat;
            if (rearRightLight) rearRightLight.material = mat;

            // Ensure left side is off
            if (frontLeftLight) frontLeftLight.material = offMaterial;
            if (rearLeftLight) rearLeftLight.material = offMaterial;
        }
    }

    private void TurnOffAllLights()
    {
        if (offMaterial == null) return;

        if (frontLeftLight) frontLeftLight.material = offMaterial;
        if (frontRightLight) frontRightLight.material = offMaterial;
        if (rearLeftLight) rearLeftLight.material = offMaterial;
        if (rearRightLight) rearRightLight.material = offMaterial;
    }
}
