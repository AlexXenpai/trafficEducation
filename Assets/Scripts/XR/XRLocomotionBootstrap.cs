using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// VR locomotion'u (joystick ile yürüyüş + snap turn) otomatik bağlar.
/// InputActionAsset'i Resources'tan yükler: "XRI Default Input Actions".
/// </summary>
public class XRLocomotionBootstrap : MonoBehaviour
{
    [Header("Input")]
    public string resourcesInputActionsName = "XRI Default Input Actions";

    [Header("Move")]
    public float moveSpeed = 2.0f;
    public bool enableStrafe = true;

    [Header("Turn")]
    public float snapTurnAngle = 45f;

    [Header("References")]
    public Transform forwardSource; // boşsa Main Camera kullanılır

    InputActionAsset inputAsset;

    void Awake()
    {
        // Input actions
        inputAsset = Resources.Load<InputActionAsset>(resourcesInputActionsName);
        if (inputAsset == null)
        {
            Debug.LogError($"XRLocomotionBootstrap: Resources'ta InputActionAsset bulunamadı: {resourcesInputActionsName}. 'Assets/Resources/{resourcesInputActionsName}.inputactions' olmalı.");
            return;
        }

        inputAsset.Enable();

        // Forward source
        if (forwardSource == null)
        {
            var cam = Camera.main;
            if (cam != null)
                forwardSource = cam.transform;
        }

        SetupMove();
        SetupSnapTurn();
    }

    void SetupMove()
    {
        var moveProvider = GetComponent<ActionBasedContinuousMoveProvider>();
        if (moveProvider == null)
        {
            Debug.LogWarning("XRLocomotionBootstrap: ActionBasedContinuousMoveProvider bulunamadı (XR Origin üzerinde)." );
            return;
        }

        moveProvider.enabled = true;
        moveProvider.moveSpeed = moveSpeed;
        moveProvider.enableStrafe = enableStrafe;

        if (forwardSource != null)
            moveProvider.forwardSource = forwardSource;

        // Bind actions (Left/Right)
        var leftMove = inputAsset.FindAction("XRI Left Locomotion/Move", true);
        var rightMove = inputAsset.FindAction("XRI Right Locomotion/Move", true);

        SetInputActionProperty(moveProvider, new[] { "leftHandMoveAction", "m_LeftHandMoveAction" }, leftMove);
        SetInputActionProperty(moveProvider, new[] { "rightHandMoveAction", "m_RightHandMoveAction" }, rightMove);
    }

    void SetupSnapTurn()
    {
        var snapTurn = GetComponent<ActionBasedSnapTurnProvider>();
        if (snapTurn == null)
            snapTurn = gameObject.AddComponent<ActionBasedSnapTurnProvider>();

        snapTurn.enabled = true;

        // Turn amount field/property name değişebiliyor, reflection ile set edelim.
        TrySetFloat(snapTurn, new[] { "turnAmount", "m_TurnAmount", "snapTurnAmount" }, snapTurnAngle);

        var leftTurn = inputAsset.FindAction("XRI Left Locomotion/Turn", true);
        var rightTurn = inputAsset.FindAction("XRI Right Locomotion/Turn", true);

        SetInputActionProperty(snapTurn, new[] { "leftHandTurnAction", "m_LeftHandTurnAction" }, leftTurn);
        SetInputActionProperty(snapTurn, new[] { "rightHandTurnAction", "m_RightHandTurnAction" }, rightTurn);
    }

    static void SetInputActionProperty(Component component, string[] candidateNames, InputAction action)
    {
        if (action == null) return;
        var prop = new InputActionProperty(action);

        var t = component.GetType();

        // Try properties
        foreach (var name in candidateNames)
        {
            var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null && p.CanWrite && p.PropertyType == typeof(InputActionProperty))
            {
                p.SetValue(component, prop);
                return;
            }
        }

        // Try fields
        foreach (var name in candidateNames)
        {
            var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null && f.FieldType == typeof(InputActionProperty))
            {
                f.SetValue(component, prop);
                return;
            }
        }

        Debug.LogWarning($"XRLocomotionBootstrap: {t.Name} üzerinde InputActionProperty alanı bulunamadı ({string.Join(",", candidateNames)})." );
    }

    static void TrySetFloat(Component component, string[] candidateNames, float value)
    {
        var t = component.GetType();

        foreach (var name in candidateNames)
        {
            var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null && p.CanWrite && p.PropertyType == typeof(float))
            {
                p.SetValue(component, value);
                return;
            }
        }

        foreach (var name in candidateNames)
        {
            var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null && f.FieldType == typeof(float))
            {
                f.SetValue(component, value);
                return;
            }
        }
    }
}
