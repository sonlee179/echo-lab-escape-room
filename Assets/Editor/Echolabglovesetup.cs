// Dat file nay vao thu muc Assets/Editor.
// Menu: EchoLab -> 6 - Gan Gang tay, Nhat ky, Thong bao
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class EchoLabGloveSetup
{
    [MenuItem("EchoLab/6 - Gan Gang tay, Nhat ky, Thong bao")]
    public static void SetupGloves()
    {
        GameObject player = GameObject.FindWithTag("Player");
        GameObject canvas = GameObject.Find("GameCanvas");
        GameManager gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();

        if (player == null || canvas == null || gm == null)
        {
            EditorUtility.DisplayDialog("EchoLab",
                "Thieu Player, GameCanvas hoac GameManager. Hay chay menu 2 truoc.",
                "OK");
            return;
        }

        // ---- Chu thong bao (giua tren man hinh, duoi dong ho) ----
        TMP_Text msg;
        Transform existing = canvas.transform.Find("MessageText");
        if (existing != null)
        {
            msg = existing.GetComponent<TMP_Text>();
        }
        else
        {
            GameObject go = new GameObject("MessageText", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            Undo.RegisterCreatedObjectUndo(go, "Create MessageText");

            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -140f);
            rt.sizeDelta = new Vector2(1400f, 140f);

            TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
            t.fontSize = 38f;
            t.alignment = TextAlignmentOptions.Top;
            t.color = new Color(1f, 0.9f, 0.3f);
            t.raycastTarget = false;
            t.text = "";
            msg = t;
        }

        gm.messageText = msg;
        EditorUtility.SetDirty(gm);

        // ---- Tat che do thu (phai nhat gang tay moi co ky nang) ----
        MassTuner mt = player.GetComponent<MassTuner>();
        if (mt != null) { mt.debugUnlock = false; EditorUtility.SetDirty(mt); }

        EchoRecorder er = player.GetComponent<EchoRecorder>();
        if (er != null) { er.debugUnlock = false; EditorUtility.SetDirty(er); }

        GravityFlip gf = player.GetComponent<GravityFlip>();
        if (gf != null) { gf.debugUnlock = false; EditorUtility.SetDirty(gf); }

        // ---- 3 gang tay mau (dat tam trong phong 1, sau nay keo sang cac phong) ----
        MakePickup("Glove_Mass", new Vector3(-4f, 0.9f, -4.5f), new Color(0.2f, 0.8f, 1f),
            Ability.MassTuner,
            "GLOVE 1: MASS TUNER. Look at an object and scroll the mouse wheel to change its mass.");

        MakePickup("Glove_Echo", new Vector3(0f, 0.9f, -4.5f), new Color(1f, 0.5f, 0.2f),
            Ability.TimeEcho,
            "GLOVE 2: TIME ECHO. Press Q to record yourself. Your past can stand in your place.");

        MakePickup("Glove_Gravity", new Vector3(4f, 0.9f, -4.5f), new Color(0.7f, 0.4f, 1f),
            Ability.GravityFlip,
            "GLOVE 3: GRAVITY FLIP. Press F. Objects fall upward, but you do not.");

        // ---- May tinh nhat ky ----
        if (GameObject.Find("LogTerminal1") == null)
        {
            GameObject term = GameObject.CreatePrimitive(PrimitiveType.Cube);
            term.name = "LogTerminal1";
            term.transform.position = new Vector3(5.6f, 1f, -2f);
            term.transform.localScale = new Vector3(0.3f, 1.2f, 0.9f);
            term.GetComponent<Renderer>().sharedMaterial = MakeMat("TerminalMat", new Color(0.1f, 0.9f, 0.5f));
            Undo.RegisterCreatedObjectUndo(term, "Create LogTerminal1");

            LogTerminal lt = term.AddComponent<LogTerminal>();
            lt.log = "LOG 01: Dr. Hao built the Echo machine to copy himself through time. It worked. The lab is collapsing.";
            lt.seconds = 7f;
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("EchoLab",
            "Xong! Da tao MessageText, 3 gang tay (Glove_Mass, Glove_Echo, Glove_Gravity) o tuong phia Nam, va LogTerminal1 o tuong phia Dong.\n\nGio ky nang chi hoat dong sau khi nhat gang tay.",
            "OK");
    }

    static void MakePickup(string objName, Vector3 pos, Color color, Ability ab, string message)
    {
        if (GameObject.Find(objName) != null) return;

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = objName;
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(45f, 45f, 0f);
        go.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        go.GetComponent<Renderer>().sharedMaterial = MakeMat(objName + "Mat", color);
        Undo.RegisterCreatedObjectUndo(go, "Create " + objName);

        AbilityPickup ap = go.AddComponent<AbilityPickup>();
        ap.ability = ab;
        ap.message = message;
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
        if (m.HasProperty("_EmissionColor"))
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", color * 1.5f);
        }
        AssetDatabase.CreateAsset(m, path);
        return m;
    }
}