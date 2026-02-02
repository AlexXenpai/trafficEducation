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
    
    private bool leftSignalCallbackRegistered = false;
    private bool rightSignalCallbackRegistered = false;

    void OnEnable()  
    { 
        // Callback'leri bir frame sonra register et (CarInputBootstrap'ın action'ları ataması için bekle)
        StartCoroutine(RegisterCallbacksDelayed());
    }
    
    System.Collections.IEnumerator RegisterCallbacksDelayed()
    {
        // Bir frame bekle
        yield return null;
        
        if (move.action != null) move.action.Enable(); 
        
        // Sol sinyal callback
        if (leftSignalAction.action != null)
        {
            leftSignalAction.action.Enable();
            if (!leftSignalCallbackRegistered)
            {
                leftSignalAction.action.performed += OnLeftSignalPerformed;
                leftSignalCallbackRegistered = true;
                Debug.Log($"VRCarInput: Sol sinyal callback registered. Binding: {leftSignalAction.action.bindings[0].path}");
            }
        }
        else
        {
            Debug.LogWarning("VRCarInput: leftSignalAction.action is null!");
        }
        
        // Sağ sinyal callback
        if (rightSignalAction.action != null)
        {
            rightSignalAction.action.Enable();
            if (!rightSignalCallbackRegistered)
            {
                rightSignalAction.action.performed += OnRightSignalPerformed;
                rightSignalCallbackRegistered = true;
                Debug.Log($"VRCarInput: Sağ sinyal callback registered. Binding: {rightSignalAction.action.bindings[0].path}");
            }
        }
        else
        {
            Debug.LogWarning("VRCarInput: rightSignalAction.action is null!");
        }
    }
    
    void OnDisable() 
    { 
        if (move.action != null) move.action.Disable(); 
        
        if (leftSignalAction.action != null)
        {
            leftSignalAction.action.Disable();
            if (leftSignalCallbackRegistered)
            {
                leftSignalAction.action.performed -= OnLeftSignalPerformed;
                leftSignalCallbackRegistered = false;
            }
        }
        
        if (rightSignalAction.action != null)
        {
            rightSignalAction.action.Disable();
            if (rightSignalCallbackRegistered)
            {
                rightSignalAction.action.performed -= OnRightSignalPerformed;
                rightSignalCallbackRegistered = false;
            }
        }
    }
    
    private void OnLeftSignalPerformed(InputAction.CallbackContext ctx)
    {
        Debug.Log("Left signal button pressed");
        if (signalSystem != null) 
        {
            signalSystem.ToggleLeftSignal();
        }
    }
    
    private void OnRightSignalPerformed(InputAction.CallbackContext ctx)
    {
        Debug.Log("Right signal button pressed");
        if (signalSystem != null) 
        {
            signalSystem.ToggleRightSignal();
        }
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
        
        // Signal inputs artık callback ile işleniyor
    }
}
