using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using Unity.XR.CoreUtils;

public class SceneSetup
{
    public static void Execute()
    {
        // 1. GameManager bul
        GameManager gm = GameObject.FindObjectOfType<GameManager>();
        if (gm == null)
        {
            Debug.LogError("GameManager not found!");
            return;
        }

        // 2. Sahne objelerini bul
        GameObject playerCar = GameObject.Find("PlayerCar");
        GameObject pedestrian = GameObject.Find("pedestrian");
        GameObject entryPanel = GameObject.Find("Canvas/GirişPaneli");

        // XR Origin'i component üzerinden bul (EN DOĞRUSU)
        XROrigin xrOriginComp = GameObject.FindObjectOfType<XROrigin>();
        GameObject xrOrigin = xrOriginComp != null ? xrOriginComp.gameObject : null;

        // 3. GameManager referanslarını ata
        gm.playerCar = playerCar;
        gm.pedestrian = pedestrian;
        gm.xrOrigin = xrOrigin;
        gm.entryPanel = entryPanel;

        EditorUtility.SetDirty(gm);

        // 4. Pedestrian setup (SADE – kontrol GameManager’da)
        if (pedestrian != null)
        {
            // Yere gömülmesin diye hafif yukarı al
            pedestrian.transform.position = new Vector3(
                pedestrian.transform.position.x,
                1.0f,
                pedestrian.transform.position.z
            );

            PedestrianController pc = pedestrian.GetComponent<PedestrianController>();
            if (pc == null)
                pc = pedestrian.AddComponent<PedestrianController>();

            // Mod seçimine kadar kapalı
            pc.enabled = false;

            EditorUtility.SetDirty(pc);
        }

        // 5. UI Button bağlantıları
        if (entryPanel != null)
        {
            Transform carBtnTr = entryPanel.transform.Find("Araba Modu Seç");
            if (carBtnTr != null)
            {
                Button btn = carBtnTr.GetComponent<Button>();
                if (btn != null)
                {
                    UnityEditor.Events.UnityEventTools.RemovePersistentListener(
                        btn.onClick, gm.StartCarMode);
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(
                        btn.onClick, gm.StartCarMode);
                }
            }

            Transform pedBtnTr = entryPanel.transform.Find("Yaya Modu Seç");
            if (pedBtnTr != null)
            {
                Button btn = pedBtnTr.GetComponent<Button>();
                if (btn != null)
                {
                    UnityEditor.Events.UnityEventTools.RemovePersistentListener(
                        btn.onClick, gm.StartPedestrianMode);
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(
                        btn.onClick, gm.StartPedestrianMode);
                }
            }
        }

        Debug.Log("Scene Setup Updated (GameManager + XR Origin uyumlu)!");
    }
}
