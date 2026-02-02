using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class FixVRRaysComplete : MonoBehaviour
{
    [MenuItem("Tools/Fix VR Rays Complete")]
    public static void Execute()
    {
        int fixCount = 0;
        
        // 1. Fix all CurveVisualControllers - make rays always visible
        var curveVisuals = Object.FindObjectsByType<CurveVisualController>(FindObjectsSortMode.None);
        foreach (var cv in curveVisuals)
        {
            var so = new SerializedObject(cv);
            
            // Make line extend even without hit
            var extendProp = so.FindProperty("m_ExtendLineToEmptyHit");
            if (extendProp != null)
            {
                extendProp.boolValue = true;
                Debug.Log($"Set extendLineToEmptyHit=true on {cv.gameObject.name}");
            }
            
            // Set resting line length longer
            var restingLengthProp = so.FindProperty("m_RestingVisualLineLength");
            if (restingLengthProp != null)
            {
                restingLengthProp.floatValue = 5f; // 5 meters when not hitting anything
            }
            
            // Set line dynamics to always show
            var dynamicsModeProp = so.FindProperty("m_LineDynamicsMode");
            if (dynamicsModeProp != null)
            {
                dynamicsModeProp.enumValueIndex = 0; // None - always show full line
            }
            
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(cv);
            fixCount++;
        }
        Debug.Log($"Fixed {curveVisuals.Length} CurveVisualControllers");
        
        // 2. Enable all LineRenderers
        var lineRenderers = Object.FindObjectsByType<LineRenderer>(FindObjectsSortMode.None);
        foreach (var lr in lineRenderers)
        {
            if (!lr.enabled)
            {
                lr.enabled = true;
                EditorUtility.SetDirty(lr);
                Debug.Log($"Enabled LineRenderer on {lr.gameObject.name}");
                fixCount++;
            }
            
            // Set line width if too thin
            if (lr.startWidth < 0.005f)
            {
                lr.startWidth = 0.01f;
                lr.endWidth = 0.005f;
                EditorUtility.SetDirty(lr);
            }
        }
        
        // 3. Make sure NearFarInteractors have UI interaction enabled
        var nearFarInteractors = Object.FindObjectsByType<NearFarInteractor>(FindObjectsSortMode.None);
        foreach (var nfi in nearFarInteractors)
        {
            var so = new SerializedObject(nfi);
            
            var uiProp = so.FindProperty("m_EnableUIInteraction");
            if (uiProp != null && !uiProp.boolValue)
            {
                uiProp.boolValue = true;
                so.ApplyModifiedProperties();
                Debug.Log($"Enabled UI interaction on {nfi.gameObject.name}");
                fixCount++;
            }
            
            EditorUtility.SetDirty(nfi);
        }
        
        // 4. Configure XRUIInputModule
        var xrUIModule = Object.FindFirstObjectByType<XRUIInputModule>();
        if (xrUIModule != null)
        {
            var so = new SerializedObject(xrUIModule);
            
            // Enable XR input
            var enableXRProp = so.FindProperty("m_EnableXRInput");
            if (enableXRProp != null)
            {
                enableXRProp.boolValue = true;
            }
            
            // Enable builtin actions as fallback
            var fallbackProp = so.FindProperty("m_EnableBuiltinActionsAsFallback");
            if (fallbackProp != null)
            {
                fallbackProp.boolValue = true;
            }
            
            so.ApplyModifiedProperties();
            xrUIModule.enabled = true;
            EditorUtility.SetDirty(xrUIModule);
            Debug.Log("Configured XRUIInputModule");
            fixCount++;
        }
        
        // 5. Make sure TrackedDeviceGraphicRaycaster is enabled on Canvas
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var canvas in canvases)
        {
            var trackedRaycaster = canvas.GetComponent<TrackedDeviceGraphicRaycaster>();
            if (trackedRaycaster != null && !trackedRaycaster.enabled)
            {
                trackedRaycaster.enabled = true;
                EditorUtility.SetDirty(trackedRaycaster);
                Debug.Log($"Enabled TrackedDeviceGraphicRaycaster on {canvas.gameObject.name}");
                fixCount++;
            }
        }
        
        // 6. Check XR Interaction Manager exists and is enabled
        var xrManager = Object.FindFirstObjectByType<XRInteractionManager>();
        if (xrManager != null)
        {
            xrManager.enabled = true;
            EditorUtility.SetDirty(xrManager);
            Debug.Log($"XR Interaction Manager is active: {xrManager.gameObject.name}");
        }
        else
        {
            Debug.LogError("XR Interaction Manager not found!");
        }
        
        Debug.Log($"VR Rays fix complete! Applied {fixCount} fixes.");
    }
}
