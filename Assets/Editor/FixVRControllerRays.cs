using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class FixVRControllerRays : MonoBehaviour
{
    [MenuItem("Tools/Fix VR Controller Rays")]
    public static void Execute()
    {
        // Find XR Interaction Manager
        XRInteractionManager interactionManager = Object.FindFirstObjectByType<XRInteractionManager>();
        if (interactionManager == null)
        {
            Debug.LogError("XR Interaction Manager not found!");
            return;
        }
        Debug.Log($"Found XR Interaction Manager: {interactionManager.gameObject.name}");

        // Find all NearFarInteractors and connect them to the manager
        var nearFarInteractors = Object.FindObjectsByType<NearFarInteractor>(FindObjectsSortMode.None);
        foreach (var interactor in nearFarInteractors)
        {
            // Set interaction manager
            var serializedObject = new SerializedObject(interactor);
            var managerProp = serializedObject.FindProperty("m_InteractionManager");
            if (managerProp != null)
            {
                managerProp.objectReferenceValue = interactionManager;
                serializedObject.ApplyModifiedProperties();
                Debug.Log($"Connected {interactor.gameObject.name} to XR Interaction Manager");
            }
            
            // Enable the interactor
            interactor.enabled = true;
            
            // Find and enable LineRenderer in children
            var lineRenderers = interactor.GetComponentsInChildren<LineRenderer>(true);
            foreach (var lr in lineRenderers)
            {
                lr.enabled = true;
                Debug.Log($"Enabled LineRenderer on {lr.gameObject.name}");
            }
            
            EditorUtility.SetDirty(interactor);
        }

        // Find all XRInteractionGroups and connect them
        var interactionGroups = Object.FindObjectsByType<XRInteractionGroup>(FindObjectsSortMode.None);
        foreach (var group in interactionGroups)
        {
            var serializedObject = new SerializedObject(group);
            var managerProp = serializedObject.FindProperty("m_InteractionManager");
            if (managerProp != null)
            {
                managerProp.objectReferenceValue = interactionManager;
                serializedObject.ApplyModifiedProperties();
                Debug.Log($"Connected XRInteractionGroup {group.gameObject.name} to XR Interaction Manager");
            }
            EditorUtility.SetDirty(group);
        }

        // Find XRUIInputModule and configure it
        var xrUIInputModule = Object.FindFirstObjectByType<XRUIInputModule>();
        if (xrUIInputModule != null)
        {
            // XRUIInputModule should automatically find interactors, but let's make sure it's enabled
            xrUIInputModule.enabled = true;
            xrUIInputModule.enableXRInput = true;
            Debug.Log("XRUIInputModule configured and enabled");
            EditorUtility.SetDirty(xrUIInputModule);
        }
        else
        {
            Debug.LogWarning("XRUIInputModule not found! UI interaction may not work.");
        }

        // Find all XRRayInteractors (for teleport) and enable their line visuals too
        var rayInteractors = Object.FindObjectsByType<XRRayInteractor>(FindObjectsSortMode.None);
        foreach (var rayInteractor in rayInteractors)
        {
            var serializedObject = new SerializedObject(rayInteractor);
            var managerProp = serializedObject.FindProperty("m_InteractionManager");
            if (managerProp != null)
            {
                managerProp.objectReferenceValue = interactionManager;
                serializedObject.ApplyModifiedProperties();
            }
            
            // Enable line renderer
            var lr = rayInteractor.GetComponent<LineRenderer>();
            if (lr != null)
            {
                lr.enabled = true;
                Debug.Log($"Enabled LineRenderer on XRRayInteractor {rayInteractor.gameObject.name}");
            }
            
            EditorUtility.SetDirty(rayInteractor);
        }

        Debug.Log("VR Controller Rays setup completed!");
        Debug.Log($"Configured {nearFarInteractors.Length} NearFarInteractors");
        Debug.Log($"Configured {interactionGroups.Length} XRInteractionGroups");
        Debug.Log($"Configured {rayInteractors.Length} XRRayInteractors");
    }
}
