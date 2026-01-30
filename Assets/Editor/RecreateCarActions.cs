#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public static class RecreateCarActions
{
    [MenuItem("Tools/Setup/Recreate Car Actions")]
    public static void Execute()
    {
        string path = "Assets/Input/CarActions.inputactions";
        
        // Create asset
        var asset = ScriptableObject.CreateInstance<InputActionAsset>();
        asset.name = "CarActions";
        
        // Create Map
        var map = asset.AddActionMap("CarControls");
        
        // Create Actions
        var move = map.AddAction("Move", type: InputActionType.Value);
        move.expectedControlType = "Vector2";
        
        var leftSignal = map.AddAction("LeftSignal", type: InputActionType.Button);
        var rightSignal = map.AddAction("RightSignal", type: InputActionType.Button);
        
        // Add Bindings
        move.AddBinding("<XRController>{RightHand}/thumbstick");
        move.AddBinding("<Gamepad>/leftStick");
        
        // Composite
        move.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        
        leftSignal.AddBinding("<XRController>{LeftHand}/primaryButton");
        leftSignal.AddBinding("<Keyboard>/q");
        
        rightSignal.AddBinding("<XRController>{RightHand}/primaryButton");
        rightSignal.AddBinding("<Keyboard>/e");
        
        // Save
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("CarActions re-created at " + path);
    }
}
#endif
