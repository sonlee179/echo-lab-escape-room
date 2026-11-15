// Dat file nay vao thu muc Assets/Editor.
// Menu: EchoLab -> 4 - Gan Time Echo
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class EchoLabEchoSetup
{
    [MenuItem("EchoLab/4 - Gan Time Echo (Ghost + Player + UI)")]
    public static void SetupEcho()
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

        // ---- Layer Player va Ghost ----
        int playerLayer = EnsureLayer("Player");
        int ghostLayer = EnsureLayer("Ghost");

        foreach (Transform t in player.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = playerLayer;

        // ---- Prefab Ghost ----
        GameObject ghostPrefab = BuildGhostPrefab(ghostLayer);

        // ---- Nhan EchoLabel (goc tren trai, duoi KeyText) ----
        TMP_Text label;
        Transform existing = canvas.transform.Find("EchoLabel");
        if (existing != null)
        {
            label = existing.GetComponent<TMP_Text>();
        }
        else
        {
            GameObject go = new GameObject("EchoLabel", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            Undo.RegisterCreatedObjectUndo(go, "Create EchoLabel");

            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(30f, -90f);
            rt.sizeDelta = new Vector2(800f, 60f);

            TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
            t.fontSize = 32f;
            t.alignment = TextAlignmentOptions.Left;
            t.color = new Color(1f, 0.5f, 0.5f);
            t.raycastTarget = false;
            t.text = "";
            label = t;
        }

        // ---- Gan EchoRecorder vao Player ----
        EchoRecorder rec = player.GetComponent<EchoRecorder>();
        if (rec == null) rec = Undo.AddComponent<EchoRecorder>(player);
        rec.ghostPrefab = ghostPrefab;
        rec.label = label;
        rec.maxTime = 10f;
        EditorUtility.SetDirty(rec);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("EchoLab",
            "Da gan Time Echo xong!\n\nBam Play, nhan Q de ghi 10 giay, Q lan nua de dung va tao bong ma.",
            "OK");
    }

    // =====================================================================
    static GameObject BuildGhostPrefab(int ghostLayer)
    {
        const string prefabDir = "Assets/prefabs";
        if (!AssetDatabase.IsValidFolder(prefabDir))
            AssetDatabase.CreateFolder("Assets", "prefabs");

        string prefabPath = prefabDir + "/Ghost.prefab";

        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        g.name = "Ghost";
        g.layer = ghostLayer;

        Rigidbody rb = g.AddComponent<Rigidbody>();
        rb.mass = 5f;
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        g.GetComponent<Renderer>().sharedMaterial = MakeGhostMaterial();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(g, prefabPath);
        Object.DestroyImmediate(g);
        return prefab;
    }

    static Material MakeGhostMaterial()
    {
        const string dir = "Assets/AutoMaterials";
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets", "AutoMaterials");

        string path = dir + "/GhostMat.mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        Material m = new Material(shader);
        Color c = new Color(0.2f, 0.9f, 1f, 0.4f);

        m.color = c;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);

        // Che do trong suot (URP Lit)
        if (m.HasProperty("_Surface"))
        {
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetOverrideTag("RenderType", "Transparent");
            m.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = (int)RenderQueue.Transparent;
            m.SetShaderPassEnabled("ShadowCaster", false);
        }

        // Phat sang
        if (m.HasProperty("_EmissionColor"))
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", new Color(0.2f, 0.9f, 1f) * 1.5f);
        }

        AssetDatabase.CreateAsset(m, path);
        return m;
    }

    static int EnsureLayer(string layerName)
    {
        int idx = LayerMask.NameToLayer(layerName);
        if (idx != -1) return idx;

        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        for (int i = 8; i < 32; i++)
        {
            SerializedProperty sp = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(sp.stringValue))
            {
                sp.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                return i;
            }
        }
        return 0;
    }
}