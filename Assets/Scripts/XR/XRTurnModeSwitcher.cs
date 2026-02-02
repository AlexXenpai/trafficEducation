using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Yaya modunda snap turn (45 derece) yerine smooth/continuous turn aktif eder.
/// Mod bilgisi için XR Origin üzerindeki KameraTakip.isCarMode kullanılır.
/// </summary>
[DefaultExecutionOrder(-20)]
public class XRTurnModeSwitcher : MonoBehaviour
{
    [Header("Input")]
    public string resourcesInputActionsName = "XRI Default Input Actions";

    [Header("Continuous Turn")]
    public float continuousTurnSpeed = 90f; // deg/sec

    [Header("References")]
    public KameraTakip kameraTakip;

    InputActionAsset inputAsset;

    ActionBasedSnapTurnProvider snapTurn;
    ActionBasedContinuousTurnProvider continuousTurn;

    bool lastCarMode;

    void Awake()
    {
        if (kameraTakip == null)
            kameraTakip = GetComponent<KameraTakip>();

        snapTurn = GetComponent<ActionBasedSnapTurnProvider>();
        continuousTurn = GetComponent<ActionBasedContinuousTurnProvider>();

        inputAsset = Resources.Load<InputActionAsset>(resourcesInputActionsName);
        if (inputAsset == null)
        {
            Debug.LogWarning($"XRTurnModeSwitcher: Resources'ta InputActionAsset bulunamadı: {resourcesInputActionsName}");
            return;
        }

        inputAsset.Enable();

        // Bind actions to both providers
        var leftTurn = inputAsset.FindAction("XRI Left Locomotion/Turn", true);
        var rightTurn = inputAsset.FindAction("XRI Right Locomotion/Turn", true);

        if (snapTurn != null)
        {
            SetInputActionProperty(snapTurn, new[] { "leftHandTurnAction", "m_LeftHandTurnAction" }, leftTurn);
            SetInputActionProperty(snapTurn, new[] { "rightHandTurnAction", "m_RightHandTurnAction" }, rightTurn);
        }

        if (continuousTurn != null)
        {
            SetInputActionProperty(continuousTurn, new[] { "leftHandTurnAction", "m_LeftHandTurnAction" }, leftTurn);
            SetInputActionProperty(continuousTurn, new[] { "rightHandTurnAction", "m_RightHandTurnAction" }, rightTurn);

            TrySetFloat(continuousTurn, new[] { "turnSpeed", "m_TurnSpeed" }, continuousTurnSpeed);
        }

        // Apply initial state
        lastCarMode = !IsCarMode();
        Apply();
    }

    void Update()
    {
        bool carMode = IsCarMode();
        if (carMode == lastCarMode) return;
        lastCarMode = carMode;
        Apply();
    }

    bool IsCarMode()
    {
        return kameraTakip != null && kameraTakip.isCarMode;
    }

    void Apply()
    {
        bool carMode = IsCarMode();

        // Araba modunda: her iki turn provider da kapalı (KameraTakip yönetiyor)
        // Yaya modunda: continuous turn açık, snap turn kapalı
        if (carMode)
        {
            // Araba modunda turn provider'lar kapalı - kamera arabayı takip ediyor
            if (snapTurn != null) snapTurn.enabled = false;
            if (continuousTurn != null) continuousTurn.enabled = false;
        }
        else
        {
            // Yaya modunda: continuous turn açık
            if (snapTurn != null) snapTurn.enabled = false;
            if (continuousTurn != null) continuousTurn.enabled = true;
        }
    }

    static void SetInputActionProperty(Component component, string[] candidateNames, InputAction action)
    {
        if (action == null) return;

        // XRI (Input System tabanlı) provider'lar UnityEngine.InputSystem.InputActionProperty kullanır.
        var prop = new InputActionProperty(action);

        var t = component.GetType();

        foreach (var name in candidateNames)
        {
            var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null && p.CanWrite && p.PropertyType == typeof(InputActionProperty))
            {
                p.SetValue(component, prop);
                return;
            }
        }

        foreach (var name in candidateNames)
        {
            var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null && f.FieldType == typeof(InputActionProperty))
            {
                f.SetValue(component, prop);
                return;
            }
        }
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
