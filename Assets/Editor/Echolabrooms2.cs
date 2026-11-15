// Dat file nay vao thu muc Assets/Editor.
// Menu: EchoLab -> 10 - Mo rong: Phong 5 den 8
// Mo scene Room1 truoc khi chay. Can da chay menu 8 (de co Room2, Room3, Room4).
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class EchoLabRooms2
{
    static readonly Color Gray = new Color(0.3f, 0.32f, 0.36f);

    // =====================================================================
    [MenuItem("EchoLab/10 - Mo rong: Phong 5 den 8")]
    public static void BuildExpansion()
    {
        Scene cur = EditorSceneManager.GetActiveScene();
        if (cur.name != "Room1")
        {
            EditorUtility.DisplayDialog("EchoLab",
                "Hãy mở scene Room1 trước rồi chạy lại menu này.", "OK");
            return;
        }

        string room1Path = cur.path;
        string dir = System.IO.Path.GetDirectoryName(room1Path).Replace('\\', '/');

        for (int n = 2; n <= 4; n++)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(dir + "/Room" + n + ".unity") == null)
            {
                EditorUtility.DisplayDialog("EchoLab",
                    "Chưa có Room" + n + ". Hãy chạy menu 8 (Tạo phòng 2 đến 5) trước, rồi chạy lại menu này.",
                    "OK");
                return;
            }
        }

        if (!EditorUtility.DisplayDialog("EchoLab",
                "Công cụ sẽ tạo Room5 (Gương laser), Room6 (Thứ tự cần gạt), Room7 (Cổng dịch chuyển), " +
                "Room8 (Lõi Echo, phòng cuối). Room5 cũ sẽ bị ghi đè.\n\nTiếp tục?",
                "Tiếp tục", "Hủy"))
            return;

        EditorSceneManager.SaveScene(cur);

        for (int n = 5; n <= 8; n++)
        {
            string path = dir + "/Room" + n + ".unity";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CopyAsset(room1Path, path);

            Scene s = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            BuildRoom(n);
            EditorSceneManager.MarkSceneDirty(s);
            EditorSceneManager.SaveScene(s);
        }

        // ---- Build Settings: MainMenu (neu co) + Room1..Room8 ----
        System.Collections.Generic.List<EditorBuildSettingsScene> list =
            new System.Collections.Generic.List<EditorBuildSettingsScene>();

        string menuPath = dir + "/MainMenu.unity";
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(menuPath) != null)
            list.Add(new EditorBuildSettingsScene(menuPath, true));

        for (int n = 1; n <= 8; n++)
        {
            string p = dir + "/Room" + n + ".unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(p) != null)
                list.Add(new EditorBuildSettingsScene(p, true));
        }
        EditorBuildSettings.scenes = list.ToArray();

        EditorSceneManager.OpenScene(room1Path, OpenSceneMode.Single);

        EditorUtility.DisplayDialog("EchoLab",
            "Xong! Game giờ có 8 phòng và đã được thêm đủ vào Build Settings.\n\n" +
            "Hãy mở MainMenu và bấm Play để chơi từ đầu.",
            "OK");
    }

    // =====================================================================
    // DUNG TUNG PHONG
    // =====================================================================
    static void BuildRoom(int n)
    {
        string[] remove =
        {
            "Door1", "Plate1", "Lever", "Crate", "LogTerminal1",
            "PlateCeiling", "Glove_Mass", "Glove_Echo", "Glove_Gravity"
        };
        for (int i = 0; i < remove.Length; i++) DestroyAll(remove[i]);

        EnsureExitArea();
        ApplyTheme(n);

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(0f, 1.1f, -4f);
            player.transform.rotation = Quaternion.identity;
        }

        GameManager gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();
        if (gm != null)
        {
            gm.timeLimit = n == 5 ? 240f : n == 6 ? 240f : n == 7 ? 300f : 480f;
            gm.keyPiecesNeeded = n == 8 ? 3 : 0;
            if (gm.keyText != null) gm.keyText.gameObject.SetActive(n == 8);
            EditorUtility.SetDirty(gm);
        }

        string title =
            n == 5 ? "PHÒNG 5: GƯƠNG LASER" :
            n == 6 ? "PHÒNG 6: THỨ TỰ CẦN GẠT" :
            n == 7 ? "PHÒNG 7: CỔNG DỊCH CHUYỂN" :
                     "PHÒNG 8: LÕI ECHO";

        GameObject intro = new GameObject("RoomIntro");
        RoomIntro ri = intro.AddComponent<RoomIntro>();
        ri.title = title;

        switch (n)
        {
            case 5: BuildRoom5(); break;
            case 6: BuildRoom6(); break;
            case 7: BuildRoom7(); break;
            case 8: BuildRoom8(gm); break;
        }
    }

    // ---------------------------------------------------------------
    // PHONG 5: GUONG LASER
    // Nhat guong (E), giu R va di chuot de xoay, dua tia laser vao o thu.
    // ---------------------------------------------------------------
    static void BuildRoom5()
    {
        Door door = MakeDoor(1);

        // Buc tuong chan, buoc tia phai di vong
        Cube("LaserWall", new Vector3(-3f, 2f, -0.5f), new Vector3(6f, 4f, 0.4f), Gray, "WallMat2", 0f);

        // Bo phat laser (tuong Tay, huong ve phia Dong)
        GameObject em = Cube("LaserEmitter", new Vector3(-5.6f, 0.9f, -4f),
            new Vector3(0.5f, 0.5f, 0.5f), new Color(0.5f, 0.1f, 0.1f), "EmitterMat", 0f);
        em.AddComponent<LaserEmitter>();
        ConfigureLaser(em, new Color(1f, 0.15f, 0.15f));

        // O thu o tuong Tay, phia Bac
        MakeReceiver(new Vector3(-5.75f, 0.9f, 3.5f), new Vector3(0.2f, 1.5f, 2f), door);

        MakeMirror("Mirror1", new Vector3(-2.5f, 0.8f, -2.3f));
        MakeMirror("Mirror2", new Vector3(-1f, 0.8f, -2.3f));

        MakeTerminal("NHẬT KÝ 05: Ánh sáng đi thẳng, trừ khi gặp gương. " +
                     "Nhấn E để nhặt gương, giữ R và di chuột để xoay. Đưa tia laser vào ô thu ở tường Tây.");
    }

    // ---------------------------------------------------------------
    // PHONG 6: THU TU CAN GAT
    // Gat dung thu tu, sai thi tat ca bi dat lai.
    // ---------------------------------------------------------------
    static void BuildRoom6()
    {
        Door door = MakeDoor(1);

        GameObject pz = new GameObject("SequencePuzzle");
        SequencePuzzle sp = pz.AddComponent<SequencePuzzle>();
        if (sp.onSolved == null) sp.onSolved = new UnityEngine.Events.UnityEvent();

        // Thu tu DUNG: Do -> Vang -> Xanh la (nguoc lai voi den giao thong)
        Color[] cols =
        {
            new Color(1f, 0.15f, 0.15f),
            new Color(1f, 0.9f, 0.1f),
            new Color(0.2f, 1f, 0.3f)
        };
        // Vi tri tren tuong Tay (khong xep theo thu tu de nguoi choi phai suy nghi)
        Vector3[] pos =
        {
            new Vector3(-5.6f, 1f, 0f),
            new Vector3(-5.6f, 1f, 3f),
            new Vector3(-5.6f, 1f, -3f)
        };

        sp.levers = new Lever[3];
        for (int i = 0; i < 3; i++)
        {
            Lever lv = MakeColoredLever(pos[i], cols[i], "LeverHandleMat" + i);
            sp.levers[i] = lv;
            UnityEventTools.AddIntPersistentListener(lv.onOn, sp.Pressed, i);
        }

        UnityEventTools.AddPersistentListener(sp.onSolved, door.AddInput);

        MakeTerminal("NHẬT KÝ 06: Đèn giao thông đi từ XANH sang VÀNG rồi ĐỎ. " +
                     "Hãy gạt theo thứ tự NGƯỢC LẠI. Gạt sai thì mọi cần gạt bị đặt lại.");
    }

    // ---------------------------------------------------------------
    // PHONG 7: CONG DICH CHUYEN
    // Buc tuong chia phong, hop va nguoi choi di qua cong de sang nua ben kia.
    // ---------------------------------------------------------------
    static void BuildRoom7()
    {
        Door door = MakeDoor(2);

        Cube("Divider", new Vector3(0f, 2f, 0f), new Vector3(12f, 4f, 0.4f), Gray, "WallMat2", 0f);

        Portal a = MakePortal("PortalA", new Vector3(-4f, 1.3f, -0.3f),
            new Color(0.2f, 0.6f, 1f), new Vector3(-4f, 1.1f, -1.8f), 180f, "PortalBlueMat");
        Portal b = MakePortal("PortalB", new Vector3(4f, 1.3f, 0.3f),
            new Color(1f, 0.55f, 0.1f), new Vector3(4f, 1.1f, 1.8f), 0f, "PortalOrangeMat");
        a.target = b;
        b.target = a;

        MakeCrate(new Vector3(-3f, 0.6f, -3f), 4f);
        MakePlate("PlatePortal", new Vector3(-3f, 0.15f, 3.5f), 4f, false, door);
        MakeLever(new Vector3(2.9f, 1f, 5.55f), false, door);

        MakeTerminal("NHẬT KÝ 07: Hai cánh cổng, hai nửa căn phòng. " +
                     "Vật thể đi qua cổng vẫn giữ nguyên vận tốc. Hãy ném hộp qua cổng.");
    }

    // ---------------------------------------------------------------
    // PHONG 8: LOI ECHO (phong cuoi)
    // Cua can 5 dieu kien. Co tia laser chet nguoi truoc cua.
    // ---------------------------------------------------------------
    static void BuildRoom8(GameManager gm)
    {
        Door door = MakeDoor(5);

        // 1. Bong ma dung tam san
        MakePlate("PlateGhost", new Vector3(-4.5f, 0.15f, 3.5f), 4f, false, door);

        // 2. Hop 8 kg tren tam tran
        MakePlate("PlateCeiling", new Vector3(3f, 3.85f, 2.5f), 8f, true, door);

        // 3. Giu can gat
        MakeLever(new Vector3(2.9f, 1f, 5.55f), true, door);

        // 4. Du 3 manh chia khoa
        MakeKey("KeyPiece1", new Vector3(-5f, 0.7f, -5f));
        MakeKey("KeyPiece2", new Vector3(5f, 0.7f, -5f));
        MakeKey("KeyPiece3", new Vector3(-5f, 0.7f, 5.4f));

        if (gm != null)
        {
            if (gm.onAllKeysCollected == null)
                gm.onAllKeysCollected = new UnityEngine.Events.UnityEvent();

            if (gm.onAllKeysCollected.GetPersistentEventCount() == 0)
                UnityEventTools.AddPersistentListener(gm.onAllKeysCollected, door.AddInput);

            EditorUtility.SetDirty(gm);

            // Cau chuyen: man thang cua phong cuoi
            if (gm.winPanel != null)
            {
                Transform tr = gm.winPanel.transform.Find("Title");
                if (tr != null)
                {
                    TMP_Text tt = tr.GetComponent<TMP_Text>();
                    if (tt != null)
                    {
                        tt.text = "BẠN CHÍNH LÀ BÓNG MA!";
                        EditorUtility.SetDirty(tt);
                    }
                }
            }
        }

        // 5. Dua tia laser vao o thu bang guong
        GameObject em = Cube("LaserEmitter", new Vector3(-5.6f, 0.9f, -4.5f),
            new Vector3(0.5f, 0.5f, 0.5f), new Color(0.5f, 0.1f, 0.1f), "EmitterMat", 0f);
        em.AddComponent<LaserEmitter>();
        ConfigureLaser(em, new Color(1f, 0.15f, 0.15f));

        MakeReceiver(new Vector3(4.9f, 0.9f, 5.75f), new Vector3(2f, 1.5f, 0.2f), door);
        MakeMirror("Mirror1", new Vector3(-2f, 0.8f, -3f));

        // Laser CHET NGUOI truoc cua: phai chan lai bang mot vat
        GameObject em2 = Cube("DeadlyLaser", new Vector3(-5.6f, 1.0f, 4.6f),
            new Vector3(0.5f, 0.5f, 0.5f), new Color(0.6f, 0f, 0f), "DeadlyEmitterMat", 0f);
        LaserEmitter le = em2.AddComponent<LaserEmitter>();
        le.deadly = true;
        ConfigureLaser(em2, new Color(1f, 0f, 0f));

        // Hop chinh (phai len tam tran) va hop phu (de chan laser)
        MakeCrate(new Vector3(-3f, 0.6f, -2f), 4f);
        GameObject blocker = MakeCrate(new Vector3(-1f, 0.6f, -2f), 4f);
        TunableBody tb = blocker.GetComponent<TunableBody>();
        if (tb != null) tb.displayName = "Hộp chặn laser";

        MakeTerminal("NHẬT KÝ CUỐI: Bóng ma đã giúp bạn từ đầu... vì đó chính là bạn, từ tương lai. " +
                     "Tia laser đỏ trước cửa sẽ giết bạn, hãy chặn nó lại bằng một vật.");
    }

    // =====================================================================
    // HAM TAO DOI TUONG
    // =====================================================================
    static Door MakeDoor(int inputs)
    {
        GameObject go = Cube("Door1", new Vector3(0f, 1.85f, 6f), new Vector3(3f, 3.5f, 0.3f),
            new Color(0.25f, 0.4f, 0.65f), "DoorMat", 0f);

        Door d = go.AddComponent<Door>();
        d.requiredInputs = inputs;
        d.openOffset = new Vector3(0f, 3.5f, 0f);
        d.speed = 2f;
        return d;
    }

    static PressurePlate MakePlate(string plateName, Vector3 pos, float mass, bool ceiling, Door door)
    {
        GameObject go = Cube(plateName, pos, new Vector3(2f, 0.1f, 2f),
            new Color(0.9f, 0.1f, 0.1f), "PlateMat", 0f);

        BoxCollider col = go.GetComponent<BoxCollider>();
        col.isTrigger = true;
        if (ceiling)
        {
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

    static Lever MakeColoredLever(Vector3 pos, Color color, string handleMatName)
    {
        GameObject root = new GameObject("Lever");
        root.transform.position = pos;

        MakeChildCube("LeverBase", root.transform, Vector3.zero,
            new Vector3(0.3f, 0.3f, 0.3f), new Color(0.3f, 0.3f, 0.3f), "LeverBaseMat");

        GameObject pivot = new GameObject("LeverPivot");
        pivot.transform.SetParent(root.transform, false);

        MakeChildCube("LeverHandle", pivot.transform, new Vector3(0f, 0.35f, 0f),
            new Vector3(0.1f, 0.7f, 0.1f), color, handleMatName);

        Lever lv = root.AddComponent<Lever>();
        lv.handle = pivot.transform;
        if (lv.onOn == null) lv.onOn = new UnityEngine.Events.UnityEvent();
        if (lv.onOff == null) lv.onOff = new UnityEngine.Events.UnityEvent();
        return lv;
    }

    static Lever MakeLever(Vector3 pos, bool momentary, Door door)
    {
        Lever lv = MakeColoredLever(pos, new Color(1f, 0.85f, 0.1f), "LeverHandleMat");
        lv.momentary = momentary;
        UnityEventTools.AddPersistentListener(lv.onOn, door.AddInput);
        UnityEventTools.AddPersistentListener(lv.onOff, door.RemoveInput);
        return lv;
    }

    static GameObject MakeCrate(Vector3 pos, float mass)
    {
        GameObject go = Cube("crate", pos, Vector3.one, new Color(0.8f, 0.5f, 0.2f), "CrateMat", 0f);

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

    static void MakeMirror(string mirrorName, Vector3 pos)
    {
        GameObject go = Cube(mirrorName, pos, new Vector3(1.4f, 1.4f, 0.12f),
            new Color(0.6f, 0.85f, 1f), "MirrorMat", 0.6f);

        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.mass = 2f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        go.AddComponent<LaserMirror>();
    }

    static LaserReceiver MakeReceiver(Vector3 pos, Vector3 scale, Door door)
    {
        GameObject go = Cube("LaserReceiver", pos, scale, new Color(0.6f, 0.1f, 0.1f), "ReceiverMat", 0.4f);

        LaserReceiver rec = go.AddComponent<LaserReceiver>();
        rec.indicator = go.GetComponent<Renderer>();
        if (rec.onActivated == null) rec.onActivated = new UnityEngine.Events.UnityEvent();
        if (rec.onDeactivated == null) rec.onDeactivated = new UnityEngine.Events.UnityEvent();
        UnityEventTools.AddPersistentListener(rec.onActivated, door.AddInput);
        UnityEventTools.AddPersistentListener(rec.onDeactivated, door.RemoveInput);
        return rec;
    }

    static Portal MakePortal(string portalName, Vector3 pos, Color color,
        Vector3 exitPos, float exitYaw, string matName)
    {
        GameObject go = Cube(portalName, pos, new Vector3(2f, 2.6f, 0.15f), color, matName, 2f);

        BoxCollider col = go.GetComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(1f, 1f, 2f);

        GameObject ex = new GameObject(portalName + "_Exit");
        ex.transform.position = exitPos;
        ex.transform.rotation = Quaternion.Euler(0f, exitYaw, 0f);

        Portal p = go.AddComponent<Portal>();
        p.exitPoint = ex.transform;
        return p;
    }

    static void MakeTerminal(string log)
    {
        GameObject go = Cube("LogTerminal1", new Vector3(5.6f, 1f, -2f),
            new Vector3(0.3f, 1.2f, 0.9f), new Color(0.1f, 0.9f, 0.5f), "TerminalMat", 0.8f);

        LogTerminal lt = go.AddComponent<LogTerminal>();
        lt.log = log;
        lt.seconds = 9f;
    }

    static void MakeKey(string keyName, Vector3 pos)
    {
        GameObject go = Cube(keyName, pos, new Vector3(0.3f, 0.3f, 0.3f),
            new Color(1f, 0.85f, 0.1f), "KeyMat", 1f);
        go.transform.rotation = Quaternion.Euler(45f, 45f, 0f);
        go.AddComponent<KeyPiece>();
    }

    static void ConfigureLaser(GameObject go, Color color)
    {
        LineRenderer lr = go.GetComponent<LineRenderer>();
        if (lr == null) lr = go.AddComponent<LineRenderer>();

        lr.startWidth = 0.07f;
        lr.endWidth = 0.07f;
        lr.numCapVertices = 4;
        lr.sharedMaterial = MakeLaserMat(color);
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.positionCount = 2;
        lr.SetPosition(0, go.transform.position);
        lr.SetPosition(1, go.transform.position + go.transform.right);
    }

    // =====================================================================
    // GIAO DIEN / HO TRO
    // =====================================================================
    static void ApplyTheme(int n)
    {
        Color c =
            n == 5 ? new Color(1f, 0.25f, 0.25f) :
            n == 6 ? new Color(0.25f, 1f, 0.45f) :
            n == 7 ? new Color(1f, 0.65f, 0.15f) :
                     new Color(0.75f, 0.35f, 1f);

        Material neon = MakeMat("NeonRoom" + n, new Color(0.05f, 0.05f, 0.05f), 0f);
        if (neon.HasProperty("_EmissionColor"))
        {
            neon.EnableKeyword("_EMISSION");
            neon.SetColor("_EmissionColor", c * 2.5f);
            EditorUtility.SetDirty(neon);
        }

        Transform[] all = UnityEngine.Object.FindObjectsByType<Transform>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < all.Length; i++)
        {
            if (!all[i].name.ToLower().StartsWith("neonstripe")) continue;
            Renderer r = all[i].GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = neon;
        }

        GameObject lg = FindCI("ceilinglight");
        if (lg != null)
        {
            Light l = lg.GetComponent<Light>();
            if (l != null) l.color = Color.Lerp(Color.white, c, 0.45f);
        }
    }

    static void EnsureExitArea()
    {
        if (FindCI("FloorOut") == null)
            Cube("FloorOut", new Vector3(0f, 0f, 8f), new Vector3(6f, 0.2f, 4f), Gray, "FloorOutMat", 0f);

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

    static GameObject Cube(string objName, Vector3 pos, Vector3 scale, Color color,
        string matName, float glow)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = objName;
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = MakeMat(matName, color, glow);
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
        go.GetComponent<Renderer>().sharedMaterial = MakeMat(matName, color, 0f);
    }

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

    static Material MakeMat(string matName, Color color, float glow)
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

        if (glow > 0f && m.HasProperty("_EmissionColor"))
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", color * glow);
        }

        AssetDatabase.CreateAsset(m, path);
        return m;
    }

    static Material MakeLaserMat(Color color)
    {
        const string dir = "Assets/AutoMaterials";
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets", "AutoMaterials");

        string path = dir + "/LaserMat_" + ColorUtility.ToHtmlStringRGB(color) + ".mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Unlit/Color");

        Material m = new Material(shader);
        m.color = color;
        AssetDatabase.CreateAsset(m, path);
        return m;
    }
}