// Dat file nay vao thu muc Assets/Editor.
// Menu: EchoLab -> 3 - Gan Mass Tuner
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class EchoLabMassSetup
{
    [MenuItem("EchoLab/3 - Gan Mass Tuner (Player + Crate + UI)")]
    public static void SetupMassTuner()
    {
        GameObject player = GameObject.FindWithTag("Player");
        GameObject canvas = GameObject.Find("GameCanvas");

        if (player == null || canvas == null)
        {
            EditorUtility.DisplayDialog("EchoLab",
                "Khong tim thay Player (tag Player) hoac GameCanvas. Hay chay menu 2 truoc.",
                "OK");
            return;
        }

        // ---- Nhan hien khoi luong (giua duoi man hinh) ----
        TMP_Text label;
        Transform existing = canvas.transform.Find("MassLabel");
        if (existing != null)
        {
            label = existing.GetComponent<TMP_Text>();
        }
        else
        {
            GameObject go = new GameObject("MassLabel", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            Undo.RegisterCreatedObjectUndo(go, "Create MassLabel");

            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 80f);
            rt.sizeDelta = new Vector2(1000f, 70f);

            TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
            t.fontSize = 36f;
            t.alignment = TextAlignmentOptions.Center;
            t.color = new Color(0.5f, 1f, 1f);
            t.raycastTarget = false;
            t.text = "";
            label = t;
        }

        // ---- Tam ngam o giua man hinh ----
        if (canvas.transform.Find("Crosshair") == null)
        {
            GameObject ch = new GameObject("Crosshair", typeof(RectTransform), typeof(Image));
            ch.transform.SetParent(canvas.transform, false);
            Undo.RegisterCreatedObjectUndo(ch, "Create Crosshair");

            RectTransform rt = (RectTransform)ch.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(8f, 8f);

            Image img = ch.GetComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.8f);
            img.raycastTarget = false;
        }

        // ---- Gan MassTuner vao Player ----
        MassTuner tuner = player.GetComponent<MassTuner>();
        if (tuner == null) tuner = Undo.AddComponent<MassTuner>(player);
        tuner.cam = player.GetComponentInChildren<Camera>();
        tuner.label = label;
        EditorUtility.SetDirty(tuner);

        // ---- Vat nang hon 6kg thi khong nhac duoc ----
        GrabSystem grab = player.GetComponent<GrabSystem>();
        if (grab != null)
        {
            grab.maxGrabMass = 6f;
            EditorUtility.SetDirty(grab);
        }

        // ---- Gan TunableBody vao hop ----
        GameObject crate = GameObject.Find("crate");
        if (crate == null) crate = GameObject.Find("Crate");

        if (crate != null)
        {
            TunableBody tb = crate.GetComponent<TunableBody>();
            if (tb == null) tb = Undo.AddComponent<TunableBody>(crate);
            tb.minMass = 0.5f;
            tb.maxMass = 10f;
            EditorUtility.SetDirty(tb);
        }
        else
        {
            Debug.LogWarning("EchoLab: khong tim thay doi tuong 'crate'. Hay tu them TunableBody vao hop.");
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("EchoLab: da gan Mass Tuner (MassTuner, TunableBody, MassLabel, Crosshair).");
    }
}