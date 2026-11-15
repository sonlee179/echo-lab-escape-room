// Dat file nay vao thu muc Assets/Editor.
// Menu: EchoLab -> 8 - Tao phong 2 den 5 (tu dong)
// Mo scene Room1 truoc khi chay.
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class EchoLabRooms
{
    static readonly Color Gray = new Color(0.3f, 0.32f, 0.36f);

    // =====================================================================
    [MenuItem("EchoLab/8 - Tao phong 2 den 5 (tu dong)")]
    public static void BuildAllRooms()
    {
        Scene cur = EditorSceneManager.GetActiveScene();
        if (cur.name != "Room1")
        {
            EditorUtility.DisplayDialog("EchoLab",
                "Hãy mở scene Room1 trước rồi chạy lại menu này.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("EchoLab",
                "Công cụ sẽ tạo Room2, Room3, Room4, Room5 từ Room1 (ghi đè nếu đã có) " +
                "và dọn Room1 (xóa găng tay mẫu, tấm trần).\n\nTiếp tục?",
                "Tiếp tục", "Hủy"))
            return;

        // ---- Don Room1 va luu ----
        CleanupRoom1();
        EditorSceneManager.MarkSceneDirty(cur);
        EditorSceneManager.SaveScene(cur);

        string room1Path = cur.path;
        string dir = System.IO.Path.GetDirectoryName(room1Path).Replace('\\', '/');

        // ---- Tao Room2..Room5 ----
        for (int n = 2; n <= 5; n++)
        {
            string path = dir + "/Room" + n + ".unity";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CopyAsset(room1Path, path);

            Scene s = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            BuildRoom(n);
            EditorSceneManager.MarkSceneDirty(s);
            EditorSceneManager.SaveScene(s);
        }

        // ---- Build Settings ----
        System.Collections.Generic.List<EditorBuildSettingsScene> list =
            new System.Collections.Generic.List<EditorBuildSettingsScene>();
        for (int n = 1; n <= 5; n++)
            list.Add(new EditorBuildSettingsScene(dir + "/Room" + n + ".unity", true));
        EditorBuildSettings.scenes = list.ToArray();

        EditorSceneManager.OpenScene(room1Path, OpenSceneMode.Single);

        EditorUtility.DisplayDialog("EchoLab",
            "Xong! Đã tạo Room2 đến Room5 và thêm cả 5 phòng vào Build Settings.\n\n" +
            "Mở từng scene trong Assets/Scenes để xem và chơi thử.",
            "OK");
    }

    // =====================================================================
    // DON PHONG 1
    // =====================================================================
    static void CleanupRoom1()
    {
        string[] names = { "Glove_Mass", "Glove_Echo", "Glove_Gravity", "PlateCeiling" };
        for (int i = 0; i < names.Length; i++) DestroyAll(names[i]);
        EnsureExitArea();
    }

    // San ngoai cua + vung thoat de nguoi choi khong roi xuong khi di ra
    static void EnsureExitArea()
    {
        if (FindCI("FloorOut") == null)
            Cube("FloorOut", new Vector3(0f, 0f, 8f), new Vector3(6f, 0.2f, 4f), Gray, "FloorOutMat");

        GameObject exit = FindCI("ExitZone");
        if (exit == null)
        {
            exit = new GameObject("ExitZone");
            exit.transform.position = new Vector3(0f, 1.5f, 8f);
        }

        BoxCollider col = exit.GetComponent<BoxCollider>();
        if (col == null) col = exit.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(3f, 3f, 2f);

        if (exit.GetComponent<ExitZone>() == null) exit.AddComponent<ExitZone>();
    }

    // =====================================================================
    // DUNG TUNG PHONG
    // =====================================================================
    static void BuildRoom(int n)
    {
        // ---- Don do cua Room1 ----
        string[] remove =
        {
            "Door1", "Plate1", "Lever", "Crate", "LogTerminal1",
            "PlateCeiling", "Glove_Mass", "Glove_Echo", "Glove_Gravity"
        };
        for (int i = 0; i < remove.Length; i++) DestroyAll(remove[i]);

        EnsureExitArea();

        // ---- Nguoi choi quay ve diem xuat phat ----
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(0f, 1.1f, -4f);
            player.transform.rotation = Quaternion.identity;
        }

        // ---- GameManager ----
        GameManager gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();
        if (gm != null)
        {
            gm.timeLimit = n == 5 ? 420f : 300f;
            gm.keyPiecesNeeded = n == 5 ? 3 : 0;
            if (gm.keyText != null) gm.keyText.gameObject.SetActive(n == 5);
            EditorUtility.SetDirty(gm);
        }

        switch (n)
        {
            case 2: BuildRoom2(); break;
            case 3: BuildRoom3(); break;
            case 4: BuildRoom4(); break;
            case 5: BuildRoom5(gm); break;
        }
    }

    // ---------------------------------------------------------------
    // PHONG 2: CAN NANG (Mass Tuner)
    // Nhac hop (nhe) len be, dat len tam, phong to cho du 8 kg de mo cua.
    // ---------------------------------------------------------------
    static void BuildRoom2()
    {
        Door door = MakeDoor(1);

        MakeGlove("Glove_Mass", new Vector3(-4f, 0.9f, -4.5f), new Color(0.2f, 0.8f, 1f),
            Ability.MassTuner,
            "GĂNG TAY 1: ĐIỀU CHỈNH KHỐI LƯỢNG. Nhìn vào vật và lăn chuột để đổi khối lượng.");

        MakeCrate(new Vector3(-3f, 0.6f, -2f), 4f);

        Cube("Step", new Vector3(1.5f, 0.25f, 2.5f), new Vector3(1f, 0.5f, 3f), Gray, "StepMat");
        Cube("Platform", new Vector3(3.5f, 0.5f, 2.5f), new Vector3(3f, 1f, 3f), Gray, "StepMat");

        MakePlate("PlateHeavy", new Vector3(3.5f, 1.05f, 2.5f), 8f, false, door);

        MakeTerminal("NHẬT KÝ 02: Vật chất chỉ là con số. Hãy đổi con số. Vật quá nặng thì không thể nhấc, " +
                     "nhưng vật nhỏ thì mang đi được.");
    }

    // ---------------------------------------------------------------
    // PHONG 3: BONG MA (Time Echo)
    // Bong ma dung de tam, ban giu can gat (giu E) va chay qua cua.
    // ---------------------------------------------------------------
    static void BuildRoom3()
    {
        Door door = MakeDoor(2);

        MakeGlove("Glove_Echo", new Vector3(-4f, 0.9f, -4.5f), new Color(1f, 0.5f, 0.2f),
            Ability.TimeEcho,
            "GĂNG TAY 2: BÓNG MA THỜI GIAN. Nhấn Q để ghi lại chính bạn. Quá khứ có thể đứng thay bạn.");

        MakePlate("PlateGhost", new Vector3(-4.5f, 0.15f, 3.5f), 4f, false, door);
        MakeLever(new Vector3(2.9f, 1f, 5.55f), true, door);

        MakeTerminal("NHẬT KÝ 03: Cánh cửa cần hai thứ cùng lúc. Bạn chỉ có một cơ thể. " +
                     "Hãy ghi lại chính mình, rồi giữ phím E ở cần gạt và chạy ra.");
    }

    // ---------------------------------------------------------------
    // PHONG 4: DAO TRONG LUC (Gravity Flip)
    // Dat hop duoi tam tren tran, nhan F cho hop bay len.
    // ---------------------------------------------------------------
    static void BuildRoom4()
    {
        Door door = MakeDoor(1);

        MakeGlove("Glove_Gravity", new Vector3(-4f, 0.9f, -4.5f), new Color(0.7f, 0.4f, 1f),
            Ability.GravityFlip,
            "GĂNG TAY 3: ĐẢO TRỌNG LỰC. Nhấn F. Vật thể rơi lên trời, nhưng bạn thì không.");

        MakeCrate(new Vector3(-3f, 0.6f, -2f), 4f);
        MakePlate("PlateCeiling", new Vector3(3f, 3.85f, 2.5f), 4f, true, door);

        MakeTerminal("NHẬT KÝ 04: Trọng lực chỉ là một hướng. Hãy đảo nó đi, " +
                     "và giữ nguyên trạng thái đó cho đến khi bạn ra khỏi cửa.");
    }

    // ---------------------------------------------------------------
    // PHONG 5: THI NGHIEM CUOI (ket hop tat ca)
    // Cua can 4 dieu kien: bong ma dung tam san, hop 8kg tren tran,
    // giu can gat, va du 3 manh chia khoa.
    // ---------------------------------------------------------------
    static void BuildRoom5(GameManager gm)
    {
        Door door = MakeDoor(4);

        MakePlate("PlateGhost", new Vector3(-4.5f, 0.15f, 3.5f), 4f, false, door);
        MakePlate("PlateCeiling", new Vector3(3f, 3.85f, 2.5f), 8f, true, door);
        MakeLever(new Vector3(2.9f, 1f, 5.55f), true, door);

        MakeCrate(new Vector3(-3f, 0.6f, -3f), 4f);

        MakeKey("KeyPiece1", new Vector3(-5f, 0.7f, -5f));
        MakeKey("KeyPiece2", new Vector3(5f, 0.7f, -5f));
        MakeKey("KeyPiece3", new Vector3(-5f, 0.7f, 5f));

        if (gm != null)
        {
            if (gm.onAllKeysCollected == null)
                gm.onAllKeysCollected = new UnityEngine.Events.UnityEvent();

            if (gm.onAllKeysCollected.GetPersistentEventCount() == 0)
                UnityEventTools.AddPersistentListener(gm.onAllKeysCollected, door.AddInput);

            EditorUtility.SetDirty(gm);
        }

        MakeTerminal("NHẬT KÝ CUỐI: Bóng ma đã giúp bạn từ đầu... vì đó chính là bạn, từ tương lai. " +
                     "Hãy dùng cả ba găng tay để thoát ra.");
    }

    // =====================================================================
    // HAM TAO DOI TUONG
    // =====================================================================
    static Door MakeDoor(int inputs)
    {
        GameObject go = Cube("Door1", new Vector3(0f, 1.85f, 6f), new Vector3(3f, 3.5f, 0.3f),
            new Color(0.25f, 0.4f, 0.65f), "DoorMat");

        Door d = go.AddComponent<Door>();
        d.requiredInputs = inputs;
        d.openOffset = new Vector3(0f, 3.5f, 0f);
        d.speed = 2f;
        return d;
    }

    static PressurePlate MakePlate(string plateName, Vector3 pos, float mass, bool ceiling, Door door)
    {
        GameObject go = Cube(plateName, pos, new Vector3(2f, 0.1f, 2f),
            new Color(0.9f, 0.1f, 0.1f), "PlateMat");

        BoxCollider col = go.GetComponent<BoxCollider>();
        col.isTrigger = true;
        if (ceiling)
        {
            // Vung cam bien huong xuong duoi, chi cham khi hop bay sat tran
            col.center = new Vector3(0f, -7.5f, 0f);
            col.size = new Vector3(1f, 15f, 1f);
        }
        else
        {
            col.center = new Vector3(0f, 12f, 0f);
            col.size = new Vector3(1f, 25f, 1f);
        }

        PressurePlate pp = go.AddComponent<PressurePlate>();
        pp.requiredMass = mass;
        pp.indicator = go.GetComponent<Renderer>();
        if (pp.onActivated == null) pp.onActivated = new UnityEngine.Events.UnityEvent();
        if (pp.onDeactivated == null) pp.onDeactivated = new UnityEngine.Events.UnityEvent();
        UnityEventTools.AddPersistentListener(pp.onActivated, door.AddInput);
        UnityEventTools.AddPersistentListener(pp.onDeactivated, door.RemoveInput);
        return pp;
    }

    static Lever MakeLever(Vector3 pos, bool momentary, Door door)
    {
        GameObject root = new GameObject("Lever");
        root.transform.position = pos;

        MakeChildCube("LeverBase", root.transform, Vector3.zero,
            new Vector3(0.3f, 0.3f, 0.3f), new Color(0.3f, 0.3f, 0.3f), "LeverBaseMat");

        GameObject pivot = new GameObject("LeverPivot");
        pivot.transform.SetParent(root.transform, false);

        MakeChildCube("LeverHandle", pivot.transform, new Vector3(0f, 0.35f, 0f),
            new Vector3(0.1f, 0.7f, 0.1f), new Color(1f, 0.85f, 0.1f), "LeverHandleMat");

        Lever lv = root.AddComponent<Lever>();
        lv.handle = pivot.transform;
        lv.momentary = momentary;
        if (lv.onOn == null) lv.onOn = new UnityEngine.Events.UnityEvent();
        if (lv.onOff == null) lv.onOff = new UnityEngine.Events.UnityEvent();
        UnityEventTools.AddPersistentListener(lv.onOn, door.AddInput);
        UnityEventTools.AddPersistentListener(lv.onOff, door.RemoveInput);
        return lv;
    }

    static GameObject MakeCrate(Vector3 pos, float mass)
    {
        GameObject go = Cube("crate", pos, Vector3.one, new Color(0.8f, 0.5f, 0.2f), "CrateMat");

        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.mass = mass;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        TunableBody tb = go.AddComponent<TunableBody>();
        tb.displayName = "Hộp gỗ";
        tb.minMass = 0.5f;
        tb.maxMass = 10f;
        return go;
    }

    static void MakeGlove(string objName, Vector3 pos, Color color, Ability ab, string message)
    {
        GameObject go = Cube(objName, pos, new Vector3(0.4f, 0.4f, 0.4f), color, objName + "Mat");
        go.transform.rotation = Quaternion.Euler(45f, 45f, 0f);

        AbilityPickup ap = go.AddComponent<AbilityPickup>();
        ap.ability = ab;
        ap.message = message;
    }

    static void MakeTerminal(string log)
    {
        GameObject go = Cube("LogTerminal1", new Vector3(5.6f, 1f, -2f),
            new Vector3(0.3f, 1.2f, 0.9f), new Color(0.1f, 0.9f, 0.5f), "TerminalMat");

        LogTerminal lt = go.AddComponent<LogTerminal>();
        lt.log = log;
        lt.seconds = 8f;
    }

    static void MakeKey(string keyName, Vector3 pos)
    {
        GameObject go = Cube(keyName, pos, new Vector3(0.3f, 0.3f, 0.3f),
            new Color(1f, 0.85f, 0.1f), "KeyMat");
        go.transform.rotation = Quaternion.Euler(45f, 45f, 0f);
        go.AddComponent<KeyPiece>();
    }

    // =====================================================================
    // HAM HO TRO
    // =====================================================================
    static GameObject Cube(string objName, Vector3 pos, Vector3 scale, Color color, string matName)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = objName;
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = MakeMat(matName, color);
        return go;
    }

    static void MakeChildCube(string objName, Transform parent, Vector3 localPos,
        Vector3 localScale, Color color, string matName)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = objName;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        go.GetComponent<Renderer>().sharedMaterial = MakeMat(matName, color);
    }

    // Tim doi tuong theo ten, khong phan biet hoa thuong (ke ca doi tuong dang tat)
    static GameObject FindCI(string objName)
    {
        Transform[] all = UnityEngine.Object.FindObjectsByType<Transform>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < all.Length; i++)
        {
            if (string.Equals(all[i].name, objName, System.StringComparison.OrdinalIgnoreCase))
                return all[i].gameObject;
        }
        return null;
    }

    static void DestroyAll(string objName)
    {
        GameObject g = FindCI(objName);
        while (g != null)
        {
            UnityEngine.Object.DestroyImmediate(g);
            g = FindCI(objName);
        }
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