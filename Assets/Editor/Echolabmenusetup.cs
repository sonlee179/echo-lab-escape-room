// Dat file nay vao thu muc Assets/Editor.
// Menu: EchoLab -> 9 - Tao Menu chinh + Build Settings
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class EchoLabMenuSetup
{
    const string ScenesDir = "Assets/Scenes";

    [MenuItem("EchoLab/9 - Tao Menu chinh + Build Settings")]
    public static void BuildMenu()
    {
        // Luu scene dang mo (neu co thay doi) truoc khi tao scene moi
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ---- Camera ----
        GameObject camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        camGo.tag = "MainCamera";
        Camera cam = camGo.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.03f, 0.05f, 0.09f);

        // ---- EventSystem ----
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        // ---- Canvas ----
        GameObject canvasGo = new GameObject("MenuCanvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        Transform canvasT = canvasGo.transform;

        // ---- Chu ----
        MakeText(canvasT, "Title", "ECHO LAB", 140f,
            new Vector2(0.5f, 0.5f), new Vector2(0f, 230f), new Vector2(1400f, 180f),
            new Color(0.4f, 0.95f, 1f), FontStyles.Bold);

        MakeText(canvasT, "Subtitle", "Thoát khỏi phòng thí nghiệm sắp sụp đổ", 46f,
            new Vector2(0.5f, 0.5f), new Vector2(0f, 110f), new Vector2(1400f, 80f),
            Color.white, FontStyles.Normal);

        MakeText(canvasT, "Hint",
            "WASD: di chuyển   |   Chuột: nhìn   |   E: nhặt / tương tác   |   R: xoay vật   |   Chuột trái: ném   |   Q: bóng ma   |   F: đảo trọng lực",
            28f,
            new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(1800f, 100f),
            new Color(0.75f, 0.75f, 0.8f), FontStyles.Normal);

        // ---- MenuManager + nut ----
        GameObject mmGo = new GameObject("MenuManager");
        MenuManager mm = mmGo.AddComponent<MenuManager>();

        Button play = MakeButton(canvasT, "PlayButton", "CHƠI", new Vector2(0f, -40f));
        UnityEventTools.AddPersistentListener(play.onClick, mm.Play);

        Button quit = MakeButton(canvasT, "QuitButton", "THOÁT", new Vector2(0f, -180f));
        UnityEventTools.AddPersistentListener(quit.onClick, mm.Quit);

        // ---- Luu scene ----
        string menuPath = ScenesDir + "/MainMenu.unity";
        EditorSceneManager.SaveScene(scene, menuPath);

        // ---- Build Settings: MainMenu dung dau, roi Room1..Room5 ----
        System.Collections.Generic.List<EditorBuildSettingsScene> list =
            new System.Collections.Generic.List<EditorBuildSettingsScene>();
        list.Add(new EditorBuildSettingsScene(menuPath, true));

        for (int n = 1; n <= 5; n++)
        {
            string p = ScenesDir + "/Room" + n + ".unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(p) != null)
                list.Add(new EditorBuildSettingsScene(p, true));
        }
        EditorBuildSettings.scenes = list.ToArray();

        EditorUtility.DisplayDialog("EchoLab",
            "Đã tạo MainMenu và sắp xếp Build Settings (MainMenu đứng đầu, sau đó Room1 đến Room5).\n\n" +
            "Mở scene MainMenu rồi bấm Play để thử.",
            "OK");
    }

    // =====================================================================
    static TMP_Text MakeText(Transform parent, string objName, string text, float size,
        Vector2 anchor, Vector2 pos, Vector2 dims, Color color, FontStyles style)
    {
        GameObject go = new GameObject(objName, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = dims;

        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = TextAlignmentOptions.Center;
        t.color = color;
        t.raycastTarget = false;
        return t;
    }

    static Button MakeButton(Transform parent, string objName, string label, Vector2 pos)
    {
        GameObject go = new GameObject(objName,
            typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(Button));
        go.transform.SetParent(parent, false);

        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(420f, 110f);

        UnityEngine.UI.Image img = go.GetComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.1f, 0.2f, 0.3f, 1f);

        Button btn = go.GetComponent<Button>();
        btn.targetGraphic = img;

        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.3f, 0.8f, 0.9f, 1f);
        cb.pressedColor = new Color(0.2f, 0.5f, 0.6f, 1f);
        btn.colors = cb;

        MakeText(go.transform, "Label", label, 56f,
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400f, 100f),
            Color.white, FontStyles.Bold);

        return btn;
    }
}