using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Unity.XR.CoreUtils;

public class FixVRCanvasSetup : MonoBehaviour
{
    [MenuItem("Tools/Fix VR Canvas Setup")]
    public static void Execute()
    {
        // Find Canvas
        Canvas canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }

        // Find XR Origin
        XROrigin xrOrigin = Object.FindFirstObjectByType<XROrigin>();
        if (xrOrigin == null)
        {
            Debug.LogError("XR Origin not found!");
            return;
        }

        // Find Main Camera in XR Origin
        Camera xrCamera = xrOrigin.Camera;
        if (xrCamera == null)
        {
            Debug.LogError("XR Camera not found!");
            return;
        }

        // 1. Change Canvas to World Space
        canvas.renderMode = RenderMode.WorldSpace;
        
        // 2. Set Canvas position in front of camera (2.5 meters away)
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        
        // Position canvas in front of XR Origin at a comfortable distance
        canvas.transform.SetParent(null); // Unparent first
        canvas.transform.position = new Vector3(0, 1.5f, 3f); // 3 meters in front, at eye level
        canvas.transform.rotation = Quaternion.identity;
        
        // 3. Scale canvas appropriately for VR (1 unit = 1 meter)
        // For World Space canvas, we need a small scale since sizeDelta is in pixels
        // A comfortable UI in VR is about 1-2 meters wide at 2-3 meters distance
        // With sizeDelta of ~1154 pixels, we want it to be ~1.5 meters wide
        float desiredWidthInMeters = 1.5f;
        float pixelWidth = canvasRect.sizeDelta.x;
        float scale = desiredWidthInMeters / pixelWidth;
        canvas.transform.localScale = new Vector3(scale, scale, scale);
        
        Debug.Log($"Canvas sizeDelta: {canvasRect.sizeDelta}");
        Debug.Log($"Calculated scale: {scale}");
        
        // 4. Set world camera
        canvas.worldCamera = xrCamera;
        
        // 5. Configure EventSystem for XR
        EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
        if (eventSystem != null)
        {
            // Configure InputSystemUIInputModule
            InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (inputModule != null)
            {
                inputModule.xrTrackingOrigin = xrOrigin.transform;
                Debug.Log("InputSystemUIInputModule configured with XR Tracking Origin");
            }
            
            // Add XRUIInputModule if not present
            var xrUIInputModule = eventSystem.GetComponent<UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule>();
            if (xrUIInputModule == null)
            {
                xrUIInputModule = eventSystem.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule>();
                Debug.Log("Added XRUIInputModule to EventSystem");
            }
            
            // Disable standard InputSystemUIInputModule if XRUIInputModule is present
            if (inputModule != null && xrUIInputModule != null)
            {
                inputModule.enabled = false;
                Debug.Log("Disabled InputSystemUIInputModule (XRUIInputModule will handle input)");
            }
        }
        
        // 6. Ensure TrackedDeviceGraphicRaycaster is present
        var trackedRaycaster = canvas.GetComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();
        if (trackedRaycaster == null)
        {
            trackedRaycaster = canvas.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();
            Debug.Log("Added TrackedDeviceGraphicRaycaster to Canvas");
        }
        trackedRaycaster.enabled = true;
        
        // 7. Disable standard GraphicRaycaster (XR uses TrackedDeviceGraphicRaycaster)
        GraphicRaycaster standardRaycaster = canvas.GetComponent<GraphicRaycaster>();
        if (standardRaycaster != null)
        {
            standardRaycaster.enabled = false;
            Debug.Log("Disabled standard GraphicRaycaster");
        }
        
        // Mark scene as dirty
        EditorUtility.SetDirty(canvas.gameObject);
        EditorUtility.SetDirty(eventSystem.gameObject);
        
        Debug.Log("VR Canvas setup completed successfully!");
        Debug.Log($"Canvas position: {canvas.transform.position}");
        Debug.Log($"Canvas scale: {canvas.transform.localScale}");
    }
}
