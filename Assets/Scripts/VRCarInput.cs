using UnityEngine;
using UnityEngine.InputSystem;

public class VRCarInput : MonoBehaviour
{
    // X = direksiyon, Y = gaz/fren
    public InputActionProperty move;

    public float steer;     // -1..+1
    public float throttle;  // -1..+1

    void OnEnable()  { move.action.Enable(); }
    void OnDisable() { move.action.Disable(); }

    void Update()
    {
        Vector2 v = move.action.ReadValue<Vector2>();
        steer = v.x;
        throttle = v.y;
    }
}
