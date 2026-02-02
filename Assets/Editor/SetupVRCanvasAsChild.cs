using UnityEngine;
using UnityEditor;
using Unity.XR.CoreUtils;

public class SetupVRCanvasAsChild : MonoBehaviour
{
    [MenuItem("Tools/Setup VR Canvas As Camera Child")]
    public static void Execute()
    {
        // Find Canvas
        Canvas canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }

        // Find XR Origin and Camera
        XROrigin xrOrigin = Object.FindFirstObjectByType<XROrigin>();
        if (xrOrigin == null || xrOrigin.Camera == null)
        {
            Debug.LogError("XR Origin or Camera not found!");
            return;
        }

        Transform xrCamera = xrOrigin.Camera.transform;
        
        // Remove VRCanvasFollower if exists (we don't need it anymore)
        var follower = canvas.GetComponent<VRCanvasFollower>();
        if (follower != null)
        {
            Object.DestroyImmediate(follower);
            Debug.Log("Removed VRCanvasFollower component");
        }

        // Parent Canvas to XR Camera
        canvas.transform.SetParent(xrCamera, false);
        
        // Set local position (in front of camera)
        canvas.transform.localPosition = new Vector3(0, -0.2f, 2f); // 2m önde, biraz aşağıda
        canvas.transform.localRotation = Quaternion.identity;
        canvas.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f); // VR için uygun boyut
        
        // Update Canvas settings
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = xrOrigin.Camera;
        
        // Update RectTransform
        RectTransform rectTransform = canvas.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(1200, 800);
        
        EditorUtility.SetDirty(canvas.gameObject);
        
        Debug.Log("Canvas is now child of XR Camera!");
        Debug.Log($"Canvas parent: {canvas.transform.parent.name}");
        Debug.Log($"Canvas local position: {canvas.transform.localPosition}");
        Debug.Log($"Canvas local scale: {canvas.transform.localScale}");
    }
}
