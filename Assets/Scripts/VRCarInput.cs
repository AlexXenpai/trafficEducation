using UnityEngine;
using UnityEngine.InputSystem;

public class VRCarInput : MonoBehaviour
{
    // X = direksiyon, Y = gaz/fren
    public InputActionProperty move;
    
    [Header("Turn Signals")]
    public InputActionProperty leftSignalAction;
    public InputActionProperty rightSignalAction;
    public CarSignalSystem signalSystem;

    public float steer;     // -1..+1
    public float throttle;  // -1..+1

    void OnEnable()  
    { 
        if (move.action != null) move.action.Enable(); 
        if (leftSignalAction.action != null) leftSignalAction.action.Enable();
        if (rightSignalAction.action != null) rightSignalAction.action.Enable();
    }
    
    void OnDisable() 
    { 
        if (move.action != null) move.action.Disable(); 
        if (leftSignalAction.action != null) leftSignalAction.action.Disable();
        if (rightSignalAction.action != null) rightSignalAction.action.Disable();
    }

    void Start()
    {
        if (signalSystem == null)
            signalSystem = GetComponent<CarSignalSystem>();
    }

    void Update()
    {
        if (move.action != null)
        {
            Vector2 v = move.action.ReadValue<Vector2>();
            steer = v.x;
            throttle = v.y;
        }

        // Check for signal inputs (Button presses)
        if (leftSignalAction.action != null && leftSignalAction.action.WasPressedThisFrame())
        {
            if (signalSystem != null) signalSystem.ToggleLeftSignal();
        }

        if (rightSignalAction.action != null && rightSignalAction.action.WasPressedThisFrame())
        {
            if (signalSystem != null) signalSystem.ToggleRightSignal();
        }
    }
}
