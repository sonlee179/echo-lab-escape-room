// Menu: EchoLab -> 11 - Lam dep tat ca phong
// Can co 3 anh trong Assets/Textures: LabFloor.png, LabWall.png, LabCeiling.png
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class EchoLabPolish
{
    const string TexDir = "Assets/Textures";
    const string MatDir = "Assets/AutoMaterials";

    // =====================================================================
    [MenuItem("EchoLab/11 - Lam dep tat ca phong (do hoa, anh sang, hat bui)")]
    public static void PolishAll()
    {
        Texture2D floorTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexDir + "/LabFloor.png");
        Texture2D wallTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexDir + "/LabWall.png");
        Texture2D ceilTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexDir + "/LabCeiling.png");

        if (floorTex == null || wallTex == null || ceilTex == null)
        {
            EditorUtility.DisplayDialog("EchoLab",
                "Thiếu texture. Hãy chép thư mục Textures (3 ảnh LabFloor, LabWall, LabCeiling) vào Assets rồi chạy lại.",
                "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("EchoLab",
                "Công cụ sẽ làm đẹp tất cả scene Room1 đến Room8: texture sàn/tường/trần, sương mù, " +
                "hiệu ứng hậu kỳ, hạt bụi, đèn góc phòng, đèn chập chờn.\n\nTiếp tục?",
                "Tiếp tục", "Hủy"))
            return;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string originalPath = SceneManager.GetActiveScene().path;
        string dir = System.IO.Path.GetDirectoryName(originalPath).Replace('\\', '/');

        SetupTexture(TexDir + "/LabFloor.png");
        SetupTexture(TexDir + "/LabWall.png");
        SetupTexture(TexDir + "/LabCeiling.png");

        Material floorBig = LabMat("LabFloorBig", floorTex, new Vector2(6f, 6f), 0.45f, 0.25f);
        Material floorSmall = LabMat("LabFloorSmall", floorTex, new Vector2(1.5f, 1.5f), 0.45f, 0.25f);
        Material wall = LabMat("LabWall", wallTex, new Vector2(4f, 2f), 0.35f, 0.2f);
        Material ceiling = LabMat("LabCeiling", ceilTex, new Vector2(6f, 6f), 0.25f, 0.1f);
        Material dust = DustMat();

        int done = 0;
        for (int n = 1; n <= 8; n++)
        {
            string path = dir + "/Room" + n + ".unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) continue;

            Scene s = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            PolishRoom(floorBig, floorSmall, wall, ceiling, dust);
            EditorSceneManager.MarkSceneDirty(s);
            EditorSceneManager.SaveScene(s);
            done++;
        }

        AssetDatabase.SaveAssets();

        if (!string.IsNullOrEmpty(originalPath))
            EditorSceneManager.OpenScene(originalPath, OpenSceneMode.Single);

        EditorUtility.DisplayDialog("EchoLab",
            "Đã làm đẹp " + done + " phòng!\n\nMở từng phòng và bấm Play để xem. " +
            "Nhớ chép thư mục Resources/Audio để có âm thanh và nhạc nền.",
            "OK");
    }

    // =====================================================================
    static void PolishRoom(Material floorBig, Material floorSmall, Material wall,
        Material ceiling, Material dust)
    {
        Scene scene = SceneManager.GetActiveScene();

        // ---- 1. Gan texture theo ten doi tuong ----
        Renderer[] rends = Resources.FindObjectsOfTypeAll<Renderer>();
        for (int i = 0; i < rends.Length; i++)
        {
            Renderer r = rends[i];
            if (r.gameObject.scene != scene) continue;

            string nm = r.name.ToLower();
            if (nm == "floor" || nm == "floorout") r.sharedMaterial = floorBig;
            else if (nm == "step" || nm == "platform") r.sharedMaterial = floorSmall;
            else if (nm == "celing" || nm == "ceiling") r.sharedMaterial = ceiling;
            else if (nm.StartsWith("wall") || nm == "divider" || nm == "laserwall") r.sharedMaterial = wall;
        }

        // ---- 2. Suong mu + anh sang moi truong toi ----
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.02f, 0.03f, 0.05f);
        RenderSettings.fogDensity = 0.025f;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.10f, 0.12f, 0.17f);
        RenderSettings.skybox = null;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Camera cam = player.GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.01f, 0.015f, 0.03f);
            }
        }

        // ---- 3. Den: giam anh sang mat troi, den tran chap chon ----
        Light[] lights = Resources.FindObjectsOfTypeAll<Light>();
        Color themeColor = Color.white;
        for (int i = 0; i < lights.Length; i++)
        {
            Light l = lights[i];
            if (l.gameObject.scene != scene) continue;

            if (l.type == LightType.Directional)
            {
                l.intensity = 0.15f;
            }
            else if (l.name.ToLower() == "ceilinglight")
            {
                themeColor = l.color;
                if (l.GetComponent<LightFlicker>() == null) l.gameObject.AddComponent<LightFlicker>();
            }
        }

        // ---- 4. Den goc phong theo mau chu de ----
        for (int i = 1; i <= 4; i++) DestroyByName("CornerLight" + i);

        float[] xs = { -5f, 5f, -5f, 5f };
        float[] zs = { -5f, -5f, 5f, 5f };
        for (int i = 0; i < 4; i++)
        {
            GameObject go = new GameObject("CornerLight" + (i + 1));
            go.transform.position = new Vector3(xs[i], 3.2f, zs[i]);
            Light l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = themeColor;
            l.intensity = 0.9f;
            l.range = 9f;
            l.shadows = LightShadows.None;
        }

        // ---- 5. Hat bui bay lo lung ----
        DestroyByName("Dust");
        GameObject dg = new GameObject("Dust");
        dg.transform.position = new Vector3(0f, 2f, 0f);

        ParticleSystem ps = dg.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = ps.main;
        main.loop = true;
        main.startLifetime = 10f;
        main.startSpeed = 0.05f;
        main.startSize = 0.04f;
        main.maxParticles = 160;
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = new Color(0.8f, 0.9f, 1f, 0.35f);

        ParticleSystem.EmissionModule em = ps.emission;
        em.rateOverTime = 14f;

        ParticleSystem.ShapeModule sh = ps.shape;
        sh.shapeType = ParticleSystemShapeType.Box;
        sh.scale = new Vector3(11f, 3.6f, 11f);

        ParticleSystem.NoiseModule noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.15f;
        noise.frequency = 0.3f;

        ParticleSystemRenderer pr = dg.GetComponent<ParticleSystemRenderer>();
        pr.sharedMaterial = dust;

        // ---- 6. Hieu ung hau ky ----
        Volume[] vols = Resources.FindObjectsOfTypeAll<Volume>();
        for (int i = 0; i < vols.Length; i++)
        {
            if (vols[i].gameObject.scene != scene) continue;
            VolumeProfile profile = vols[i].sharedProfile;
            if (profile == null) continue;
            ApplyPostProcessing(profile);
        }
    }

    static void ApplyPostProcessing(VolumeProfile profile)
    {
        Bloom bloom = GetOrAdd<Bloom>(profile);
        bloom.active = true;
        bloom.intensity.Override(1.6f);
        bloom.threshold.Override(0.85f);
        bloom.scatter.Override(0.7f);

        Vignette vg = GetOrAdd<Vignette>(profile);
        vg.active = true;
        vg.intensity.Override(0.38f);
        vg.smoothness.Override(0.55f);

        ColorAdjustments ca = GetOrAdd<ColorAdjustments>(profile);
        ca.active = true;
        ca.contrast.Override(12f);
        ca.saturation.Override(8f);

        Tonemapping tm = GetOrAdd<Tonemapping>(profile);
        tm.active = true;
        tm.mode.Override(TonemappingMode.ACES);

        EditorUtility.SetDirty(profile);
    }

    static T GetOrAdd<T>(VolumeProfile profile) where T : VolumeComponent
    {
        T c;
        if (profile.TryGet(out c)) return c;

        c = profile.Add<T>(true);
        c.name = typeof(T).Name;
        if (EditorUtility.IsPersistent(profile))
            AssetDatabase.AddObjectToAsset(c, profile);
        return c;
    }

    // =====================================================================
    static void DestroyByName(string objName)
    {
        GameObject g = GameObject.Find(objName);
        while (g != null)
        {
            UnityEngine.Object.DestroyImmediate(g);
            g = GameObject.Find(objName);
        }
    }

    static void SetupTexture(string path)
    {
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return;

        ti.wrapMode = TextureWrapMode.Repeat;
        ti.filterMode = FilterMode.Bilinear;
        ti.anisoLevel = 8;
        ti.SaveAndReimport();
    }

    static Material LabMat(string matName, Texture2D tex, Vector2 tiling, float smooth, float metallic)
    {
        if (!AssetDatabase.IsValidFolder(MatDir))
            AssetDatabase.CreateFolder("Assets", "AutoMaterials");

        string path = MatDir + "/" + matName + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);

        if (m == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            m = new Material(shader);
            AssetDatabase.CreateAsset(m, path);
        }

        m.SetTexture("_BaseMap", tex);
        m.SetTextureScale("_BaseMap", tiling);
        m.SetColor("_BaseColor", Color.white);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);

        EditorUtility.SetDirty(m);
        return m;
    }

    static Material DustMat()
    {
        if (!AssetDatabase.IsValidFolder(MatDir))
            AssetDatabase.CreateFolder("Assets", "AutoMaterials");

        string path = MatDir + "/DustMat.mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Unlit/Color");

        Material m = new Material(shader);
        m.color = new Color(1f, 1f, 1f, 0.5f);
        AssetDatabase.CreateAsset(m, path);
        return m;
    }
}
