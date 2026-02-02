using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SaveMainScene : MonoBehaviour
{
    [MenuItem("Tools/Save Main Scene")]
    public static void Execute()
    {
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), "Assets/Scenes/Main Scene.unity");
        Debug.Log("Main Scene saved to Assets/Scenes/Main Scene.unity");
    }
}
