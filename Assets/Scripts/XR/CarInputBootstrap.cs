using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Inspector'da boş kalmış InputActionProperty alanlarını runtime'da doldurur.
/// PlayerCar üzerinde VRCarInput ve HybridCarController ile beraber kullanılır.
/// </summary>
[DefaultExecutionOrder(-50)]
public class CarInputBootstrap : MonoBehaviour
{
    [Header("Bindings")]
    public string xrThumbstickBinding = "<XRController>{RightHand}/thumbstick";
    public string gamepadBinding = "<Gamepad>/leftStick";

    [Header("Signals")]
    public string leftSignalBinding = "<XRController>{LeftHand}/primaryButton";
    public string rightSignalBinding = "<XRController>{RightHand}/primaryButton";
    public string leftSignalKeyboardFallback = "<Keyboard>/q";
    public string rightSignalKeyboardFallback = "<Keyboard>/e";

    InputAction moveAction;
    InputAction leftSignalAction;
    InputAction rightSignalAction;

    void Awake()
    {
        var vr = GetComponent<VRCarInput>();
        var hybrid = GetComponent<HybridCarController>();

        // Create shared move action
        moveAction = new InputAction("CarMove", InputActionType.Value, expectedControlType: "Vector2");
        if (!string.IsNullOrWhiteSpace(xrThumbstickBinding)) moveAction.AddBinding(xrThumbstickBinding);
        if (!string.IsNullOrWhiteSpace(gamepadBinding)) moveAction.AddBinding(gamepadBinding);

        // Create signal actions
        leftSignalAction = new InputAction("LeftSignal", InputActionType.Button);
        if (!string.IsNullOrWhiteSpace(leftSignalBinding)) leftSignalAction.AddBinding(leftSignalBinding);
        if (!string.IsNullOrWhiteSpace(leftSignalKeyboardFallback)) leftSignalAction.AddBinding(leftSignalKeyboardFallback);

        rightSignalAction = new InputAction("RightSignal", InputActionType.Button);
        if (!string.IsNullOrWhiteSpace(rightSignalBinding)) rightSignalAction.AddBinding(rightSignalBinding);
        if (!string.IsNullOrWhiteSpace(rightSignalKeyboardFallback)) rightSignalAction.AddBinding(rightSignalKeyboardFallback);

        // Assign
        if (vr != null)
        {
            vr.move = new InputActionProperty(moveAction);
            vr.leftSignalAction = new InputActionProperty(leftSignalAction);
            vr.rightSignalAction = new InputActionProperty(rightSignalAction);
        }

        if (hybrid != null)
        {
            hybrid.xrMoveInput = new InputActionProperty(moveAction);
        }

        // Enable (VRCarInput/HybridCarController da enable ediyor ama garanti olsun)
        moveAction.Enable();
        leftSignalAction.Enable();
        rightSignalAction.Enable();
    }

    void OnDestroy()
    {
        moveAction?.Dispose();
        leftSignalAction?.Dispose();
        rightSignalAction?.Dispose();
    }
}
