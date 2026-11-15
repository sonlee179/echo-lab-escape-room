// Dat file nay vao thu muc Assets/Editor.
// Menu: EchoLab -> 5 - Gan Gravity Flip
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class EchoLabGravitySetup
{
    [MenuItem("EchoLab/5 - Gan Gravity Flip (Player + Tam tran + UI)")]
    public static void SetupGravity()
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

        // ---- Nhan GravityLabel (goc tren trai, duoi EchoLabel) ----
        TMP_Text label;
        Transform existing = canvas.transform.Find("GravityLabel");
        if (existing != null)
        {
            label = existing.GetComponent<TMP_Text>();
        }
        else
        {
            GameObject go = new GameObject("GravityLabel", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            Undo.RegisterCreatedObjectUndo(go, "Create GravityLabel");

            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(30f, -150f);
            rt.sizeDelta = new Vector2(900f, 60f);

            TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
            t.fontSize = 32f;
            t.alignment = TextAlignmentOptions.Left;
            t.color = new Color(0.8f, 0.6f, 1f);
            t.raycastTarget = false;
            t.text = "";
            label = t;
        }

        // ---- Gan GravityFlip vao Player ----
        GravityFlip flip = player.GetComponent<GravityFlip>();
        if (flip == null) flip = Undo.AddComponent<GravityFlip>(player);
        flip.label = label;
        flip.cooldown = 3f;
        flip.roomLights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        EditorUtility.SetDirty(flip);

        // ---- Tam ap suat tren tran ----
        GameObject plate = GameObject.Find("PlateCeiling");
        if (plate == null)
        {
            plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "PlateCeiling";
            plate.transform.position = new Vector3(3f, 3.85f, -1f);
            plate.transform.localScale = new Vector3(2f, 0.1f, 2f);
            plate.GetComponent<Renderer>().sharedMaterial = MakeMat("PlateCeilingMat", new Color(0.9f, 0.1f, 0.1f));
            Undo.RegisterCreatedObjectUndo(plate, "Create PlateCeiling");
        }

        BoxCollider col = plate.GetComponent<BoxCollider>();
        if (col == null) col = Undo.AddComponent<BoxCollider>(plate);
        col.isTrigger = true;
        col.center = new Vector3(0f, -12f, 0f);   // vung cam bien huong XUONG duoi tran
        col.size = new Vector3(1f, 25f, 1f);

        PressurePlate pp = plate.GetComponent<PressurePlate>();
        if (pp == null) pp = Undo.AddComponent<PressurePlate>(plate);
        pp.requiredMass = 4f;
        pp.indicator = plate.GetComponent<Renderer>();
        EditorUtility.SetDirty(pp);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("EchoLab",
            "Da gan Gravity Flip xong!\n\nBam Play, nhan F de dao trong luc. Hop (neu khong cam tren tay) se bay len tam tran PlateCeiling.",
            "OK");
    }

    static Material MakeMat(string matName, Color color)
    {
        const string dir = "Assets/AutoMaterials";
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets", "AutoMaterials");

        string path = dir + "/" + matName + ".mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        Material m = new Material(shader);
        m.color = color;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
        AssetDatabase.CreateAsset(m, path);
        return m;
    }
}