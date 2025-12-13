using UnityEditor;
using UnityEngine;

public class ObjectReplacer2 : EditorWindow
{
    [SerializeField] private GameObject newPrefab;
    [SerializeField] private Vector3 rotationOffset = new Vector3(0, 90, 0); // Varsayýlan olarak senin istediðin 90'ý koydum

    [MenuItem("Tools/Object Replacer")]
    static void Init()
    {
        ObjectReplacer window = (ObjectReplacer)GetWindow(typeof(ObjectReplacer));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Object Replacer Tool", EditorStyles.boldLabel);

        newPrefab = (GameObject)EditorGUILayout.ObjectField("New Prefab", newPrefab, typeof(GameObject), false);

        // UI'ya rotasyon ayarý ekledik. Artýk editörden elle girebilirsin.
        rotationOffset = EditorGUILayout.Vector3Field("Rotation Offset", rotationOffset);

        EditorGUILayout.Space(); // Biraz boþluk býrakalým, her þey dip dibe olmasýn

        if (GUILayout.Button("Replace Selected Objects"))
        {
            ReplaceSelected();
        }
    }

    void ReplaceSelected()
    {
        if (newPrefab == null)
        {
            Debug.LogError("Hata: Önce yeni bir Prefab seçmelisin!");
            return;
        }

        GameObject[] selectedObjects = Selection.gameObjects;

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Replace Objects with Rotation");
        var undoGroupIndex = Undo.GetCurrentGroup();

        foreach (GameObject oldObj in selectedObjects)
        {
            GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(newPrefab);

            newObj.transform.position = oldObj.transform.position;

            // DÝKKAT: Rotasyon matematiði burada.
            // Eski rotasyonu alýp, belirlediðimiz ofset ile ÇARPIYORUZ.
            // Quaternion'larda toplama olmaz, çarpma iþlemi döndürme (rotate) demektir.
            newObj.transform.rotation = oldObj.transform.rotation * Quaternion.Euler(rotationOffset);

            newObj.transform.localScale = oldObj.transform.localScale;
            newObj.transform.parent = oldObj.transform.parent;

            Undo.RegisterCreatedObjectUndo(newObj, "Created New Object");
            Undo.DestroyObjectImmediate(oldObj);
        }

        Undo.CollapseUndoOperations(undoGroupIndex);
        Debug.Log($"{selectedObjects.Length} obje deðiþtirildi ve döndürüldü.");
    }
}