using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem.UI;

/// <summary>
/// InputSystemUIInputModule'un XR tracking origin referansını runtime'da otomatik bağlar.
/// Bazı projelerde bu alan Inspector'da görünse bile tool ile set edilemeyebiliyor.
/// </summary>
[DefaultExecutionOrder(-100)]
public class XRUIBootstrap : MonoBehaviour
{
    public Transform xrTrackingOrigin;

    void Awake()
    {
        if (xrTrackingOrigin == null)
        {
            var xr = GameObject.Find("XR Origin (XR Rig)");
            if (xr != null) xrTrackingOrigin = xr.transform;
        }

        var module = GetComponent<InputSystemUIInputModule>();
        if (module == null || xrTrackingOrigin == null) return;

        // Try set via reflection (field names differ between Input System versions)
        var t = typeof(InputSystemUIInputModule);

        // property (if exists)
        var p = t.GetProperty("xRTrackingOrigin", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.CanWrite)
        {
            p.SetValue(module, xrTrackingOrigin);
            return;
        }

        // common backing fields
        foreach (var fieldName in new[] { "m_XRTrackingOrigin", "m_XrTrackingOrigin", "m_XRTrackingOriginTransform", "m_XrTrackingOriginTransform" })
        {
            var f = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (f != null && f.FieldType == typeof(Transform))
            {
                f.SetValue(module, xrTrackingOrigin);
                return;
            }
        }
    }
}
