// Dat file nay vao thu muc Assets/Editor (ten thu muc phai la "Editor").
// Sau khi Unity bien dich xong, dung menu "EchoLab" tren thanh menu.
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class EchoLabSetup
{
    // =====================================================================
    // MENU 1: Cua + Tam ap suat + Can gat (va noi su kien)
    // =====================================================================
    [MenuItem("EchoLab/1 - Tao Cua, Tam ap suat, Can gat")]
    public static void BuildRoom1Objects()
    {
        // ---- Cua ----
        GameObject doorGo = FindOrCube("Door1",
            new Vector3(0f, 1.85f, 6f), new Vector3(3f, 3.5f, 0.3f),
            new Color(0.25f, 0.4f, 0.65f));
        Door door = GetOrAdd<Door>(doorGo);
        door.requiredInputs = 2;
        door.openOffset = new Vector3(0f, 3.5f, 0f);
        door.speed = 2f;
        EditorUtility.SetDirty(door);

        // ---- Tam ap suat ----
        GameObject plateGo = FindOrCube("Plate1",
            new Vector3(-3f, 0.15f, -1f), new Vector3(2f, 0.1f, 2f),
            new Color(0.9f, 0.1f, 0.1f));
        BoxCollider col = GetOrAdd<BoxCollider>(plateGo);
        col.isTrigger = true;
        col.center = new Vector3(0f, 12f, 0f);
        col.size = new Vector3(1f, 25f, 1f);

        PressurePlate plate = GetOrAdd<PressurePlate>(plateGo);
        plate.requiredMass = 4f;
        plate.indicator = plateGo.GetComponent<Renderer>();
        if (plate.onActivated.GetPersistentEventCount() == 0)
            UnityEventTools.AddPersistentListener(plate.onActivated, door.AddInput);
        if (plate.onDeactivated.GetPersistentEventCount() == 0)
            UnityEventTools.AddPersistentListener(plate.onDeactivated, door.RemoveInput);
        EditorUtility.SetDirty(plate);

        // ---- Can gat ----
        GameObject leverGo = GameObject.Find("Lever");
        if (leverGo == null)
        {
            leverGo = new GameObject("Lever");
            leverGo.transform.position = new Vector3(-5.6f, 1f, 2f);
            Undo.RegisterCreatedObjectUndo(leverGo, "Create Lever");

            MakeChildCube("LeverBase", leverGo.transform,
                Vector3.zero, new Vector3(0.3f, 0.3f, 0.3f), new Color(0.3f, 0.3f, 0.3f));

            GameObject pivot = new GameObject("LeverPivot");
            pivot.transform.SetParent(leverGo.transform, false);

            MakeChildCube("LeverHandle", pivot.transform,
                new Vector3(0f, 0.35f, 0f), new Vector3(0.1f, 0.7f, 0.1f),
                new Color(1f, 0.85f, 0.1f));
        }

        Lever lever = GetOrAdd<Lever>(leverGo);
        lever.handle = leverGo.transform.Find("LeverPivot");
        if (lever.onOn.GetPersistentEventCount() == 0)
            UnityEventTools.AddPersistentListener(lever.onOn, door.AddInput);
        if (lever.onOff.GetPersistentEventCount() == 0)
            UnityEventTools.AddPersistentListener(lever.onOff, door.RemoveInput);
        EditorUtility.SetDirty(lever);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("EchoLab: da tao Door1, Plate1, Lever va noi su kien.");
    }

    // =====================================================================
    // MENU 2: UI + GameManager + ExitZone (va noi tham chieu)
    // =====================================================================
    [MenuItem("EchoLab/2 - Tao UI, GameManager, ExitZone")]
    public static void BuildUI()
    {
        if (UnityEngine.Object.FindAnyObjectByType<GameManager>() != null)
        {
            EditorUtility.DisplayDialog("EchoLab",
                "Scene da co GameManager. Hay xoa GameManager va Canvas cu truoc, roi chay lai.",
                "OK");
            return;
        }

        // ---- EventSystem (de nut bam duoc) ----
        if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem",
                typeof(EventSystem), typeof(StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
        }

        // ---- Canvas ----
        GameObject canvasGo = new GameObject("GameCanvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasGo, "Create GameCanvas");
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        Transform canvasT = canvasGo.transform;

        // ---- GameManager ----
        GameObject gmGo = new GameObject("GameManager");
        Undo.RegisterCreatedObjectUndo(gmGo, "Create GameManager");
        GameManager gm = gmGo.AddComponent<GameManager>();
        gm.timeLimit = 300f;
        gm.keyPiecesNeeded = 3;

        // ---- Chu tren man hinh ----
        gm.timerText = MakeText(canvasT, "TimerText", "05:00", 60f,
            new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(400f, 100f),
            TextAlignmentOptions.Center);

        gm.keyText = MakeText(canvasT, "KeyText", "KEYS: 0/3", 36f,
            new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(500f, 60f),
            TextAlignmentOptions.Left);

        // ---- Man thang / thua ----
        gm.winPanel = MakePanel(canvasT, "WinPanel", new Color(0.05f, 0.3f, 0.1f, 0.85f),
            "ESCAPED!", gm);
        gm.losePanel = MakePanel(canvasT, "LosePanel", new Color(0.4f, 0.05f, 0.05f, 0.85f),
            "TIME'S UP!", gm);
        gm.winPanel.SetActive(false);
        gm.losePanel.SetActive(false);
        EditorUtility.SetDirty(gm);

        // ---- ExitZone ----
        GameObject exitGo = GameObject.Find("ExitZone");
        if (exitGo == null)
        {
            exitGo = new GameObject("ExitZone");
            exitGo.transform.position = new Vector3(0f, 1.5f, 8f);
            Undo.RegisterCreatedObjectUndo(exitGo, "Create ExitZone");
        }
        BoxCollider exitCol = GetOrAdd<BoxCollider>(exitGo);
        exitCol.isTrigger = true;
        exitCol.size = new Vector3(3f, 3f, 2f);
        GetOrAdd<ExitZone>(exitGo);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = gmGo;
        Debug.Log("EchoLab: da tao UI, GameManager, ExitZone va noi tham chieu.");
    }

    // =====================================================================
    // Ham ho tro
    // =====================================================================
    static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T c = go.GetComponent<T>();
        return c != null ? c : Undo.AddComponent<T>(go);
    }

    static Material MakeMat(string name, Color color)
    {
        const string dir = "Assets/AutoMaterials";
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets", "AutoMaterials");

        string path = dir + "/" + name + ".mat";
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

    static GameObject FindOrCube(string name, Vector3 pos, Vector3 scale, Color color)
    {
        GameObject existing = GameObject.Find(name);
        if (existing != null) return existing;

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = MakeMat(name + "Mat", color);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        return go;
    }

    static void MakeChildCube(string name, Transform parent,
        Vector3 localPos, Vector3 localScale, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        go.GetComponent<Renderer>().sharedMaterial = MakeMat(name + "Mat", color);
    }

    static TMP_Text MakeText(Transform parent, string name, string text, float size,
        Vector2 anchor, Vector2 pos, Vector2 dims, TextAlignmentOptions align)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = dims;

        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.alignment = align;
        t.color = Color.white;
        t.raycastTarget = false;
        return t;
    }

    static GameObject MakePanel(Transform parent, string name, Color color,
        string title, GameManager gm)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rt = (RectTransform)panel.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = color;

        MakeText(panel.transform, "Title", title, 100f,
            new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), new Vector2(1200f, 160f),
            TextAlignmentOptions.Center);

        // Nut "RESTART"
        GameObject btnGo = new GameObject("RestartButton",
            typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(panel.transform, false);

        RectTransform brt = (RectTransform)btnGo.transform;
        brt.anchorMin = new Vector2(0.5f, 0.5f);
        brt.anchorMax = new Vector2(0.5f, 0.5f);
        brt.pivot = new Vector2(0.5f, 0.5f);
        brt.anchoredPosition = new Vector2(0f, -100f);
        brt.sizeDelta = new Vector2(360f, 100f);

        Image img = btnGo.GetComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.2f, 1f);

        Button btn = btnGo.GetComponent<Button>();
        btn.targetGraphic = img;
        UnityEventTools.AddPersistentListener(btn.onClick, gm.Restart);

        MakeText(btnGo.transform, "Label", "RESTART", 48f,
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(340f, 90f),
            TextAlignmentOptions.Center);

        return panel;
    }
}