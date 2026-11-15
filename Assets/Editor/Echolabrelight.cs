// Menu: EchoLab -> 12 - Chinh sang phong (sang vua)
//       EchoLab -> 13 - Chinh sang phong (rat sang)
// Can co 3 anh goc trong Assets/Textures: LabFloor.png, LabWall.png, LabCeiling.png
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class EchoLabRelight
{
    const string TexDir = "Assets/Textures";
    const string MatDir = "Assets/AutoMaterials";

    [MenuItem("EchoLab/12 - Chinh sang phong (sang vua)")]
    public static void Normal()
    {
        Run(1f);
    }

    [MenuItem("EchoLab/13 - Chinh sang phong (rat sang)")]
    public static void Bright()
    {
        Run(1.6f);
    }

    // =====================================================================
    // k = he so do sang (1 = sang vua, 1.6 = rat sang)
    static void Run(float k)
    {
        string[] sources = { "LabFloor", "LabWall", "LabCeiling" };
        for (int i = 0; i < sources.Length; i++)
        {
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(TexDir + "/" + sources[i] + ".png") == null)
            {
                EditorUtility.DisplayDialog("EchoLab",
                    "Thiếu texture gốc " + sources[i] + ".png trong Assets/Textures. " +
                    "Hãy chép lại thư mục Textures rồi chạy lại.", "OK");
                return;
            }
        }

        if (!EditorUtility.DisplayDialog("EchoLab",
                "Công cụ sẽ làm sáng tất cả phòng Room1 đến Room8 (texture, đèn, ánh sáng môi trường, " +
                "hiệu ứng hậu kỳ, bỏ đèn chập chờn).\n\nTiếp tục?",
                "Tiếp tục", "Hủy"))
            return;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string originalPath = SceneManager.GetActiveScene().path;
        string dir = System.IO.Path.GetDirectoryName(originalPath).Replace('\\', '/');

        // ---- Texture sang hon (tao file moi, khong dong vao file goc) ----
        float gamma = k > 1f ? 0.40f : 0.45f;
        Texture2D floorTex = MakeLightTexture("LabFloor", gamma, 1.15f);
        Texture2D wallTex = MakeLightTexture("LabWall", gamma, 1.15f);
        Texture2D ceilTex = MakeLightTexture("LabCeiling", gamma, 1.15f);

        Material floorBig = LabMat("LabFloorBig", floorTex, new Vector2(6f, 6f), 0.45f, 0.2f);
        Material floorSmall = LabMat("LabFloorSmall", floorTex, new Vector2(1.5f, 1.5f), 0.45f, 0.2f);
        Material wall = LabMat("LabWall", wallTex, new Vector2(4f, 2f), 0.35f, 0.15f);
        Material ceiling = LabMat("LabCeiling", ceilTex, new Vector2(6f, 6f), 0.25f, 0.1f);

        int done = 0;
        for (int n = 1; n <= 8; n++)
        {
            string path = dir + "/Room" + n + ".unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) continue;

            Scene s = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            RelightRoom(k, floorBig, floorSmall, wall, ceiling);
            EditorSceneManager.MarkSceneDirty(s);
            EditorSceneManager.SaveScene(s);
            done++;
        }

        AssetDatabase.SaveAssets();

        if (!string.IsNullOrEmpty(originalPath))
            EditorSceneManager.OpenScene(originalPath, OpenSceneMode.Single);

        EditorUtility.DisplayDialog("EchoLab",
            "Đã chỉnh sáng " + done + " phòng!\n\nBấm Play để xem. Nếu vẫn tối, chạy menu 13 (rất sáng); " +
            "nếu quá sáng, chạy lại menu 12.",
            "OK");
    }

    // =====================================================================
    static void RelightRoom(float k, Material floorBig, Material floorSmall,
        Material wall, Material ceiling)
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

        // ---- 2. Anh sang moi truong va suong mu nhe ----
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.12f, 0.15f, 0.22f);
        RenderSettings.fogDensity = 0.006f;

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.45f * k, 0.50f * k, 0.60f * k);
        RenderSettings.ambientIntensity = 1f;

        // ---- 3. Den ----
        Light[] lights = Resources.FindObjectsOfTypeAll<Light>();
        Color themeColor = Color.white;

        for (int i = 0; i < lights.Length; i++)
        {
            Light l = lights[i];
            if (l.gameObject.scene != scene) continue;

            if (l.type == LightType.Directional)
            {
                l.intensity = 0.4f * k;
            }
            else if (l.name.ToLower() == "ceilinglight")
            {
                themeColor = l.color;
                l.intensity = 6f * k;
                l.range = 22f;
                l.shadows = LightShadows.None;

                // Bo den chap chon (neu co)
                Component flicker = l.GetComponent("LightFlicker");
                if (flicker != null) UnityEngine.Object.DestroyImmediate(flicker);
            }
        }

        // ---- 4. Den goc phong ----
        for (int i = 1; i <= 4; i++) DestroyByName("CornerLight" + i);

        float[] xs = { -5f, 5f, -5f, 5f };
        float[] zs = { -5f, -5f, 5f, 5f };
        for (int i = 0; i < 4; i++)
        {
            GameObject go = new GameObject("CornerLight" + (i + 1));
            go.transform.position = new Vector3(xs[i], 3f, zs[i]);

            Light l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = themeColor;
            l.intensity = 3f * k;
            l.range = 12f;
            l.shadows = LightShadows.None;
        }

        // ---- 5. Hieu ung hau ky ----
        Volume[] vols = Resources.FindObjectsOfTypeAll<Volume>();
        for (int i = 0; i < vols.Length; i++)
        {
            if (vols[i].gameObject.scene != scene) continue;
            VolumeProfile profile = vols[i].sharedProfile;
            if (profile != null) ApplyPostProcessing(profile, k);
        }
    }

    static void ApplyPostProcessing(VolumeProfile profile, float k)
    {
        Bloom bloom = GetOrAdd<Bloom>(profile);
        bloom.active = true;
        bloom.intensity.Override(1.0f);
        bloom.threshold.Override(1.0f);
        bloom.scatter.Override(0.6f);

        Vignette vg = GetOrAdd<Vignette>(profile);
        vg.active = true;
        vg.intensity.Override(0.22f);
        vg.smoothness.Override(0.5f);

        ColorAdjustments ca = GetOrAdd<ColorAdjustments>(profile);
        ca.active = true;
        ca.postExposure.Override(0.3f * k);
        ca.contrast.Override(6f);
        ca.saturation.Override(5f);

        Tonemapping tm = GetOrAdd<Tonemapping>(profile);
        tm.active = true;
        tm.mode.Override(TonemappingMode.Neutral);

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

    // Doc texture goc, lam sang, ghi ra file moi (ten + "Light.png")
    static Texture2D MakeLightTexture(string srcName, float gamma, float mult)
    {
        string root = System.IO.Path.GetDirectoryName(UnityEngine.Application.dataPath);
        string srcPath = TexDir + "/" + srcName + ".png";
        string outPath = TexDir + "/" + srcName + "Light.png";

        byte[] bytes = System.IO.File.ReadAllBytes(System.IO.Path.Combine(root, srcPath));

        Texture2D tmp = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        ImageConversion.LoadImage(tmp, bytes);

        Color32[] px = tmp.GetPixels32();
        for (int i = 0; i < px.Length; i++)
        {
            px[i].r = Lift(px[i].r, gamma, mult);
            px[i].g = Lift(px[i].g, gamma, mult);
            px[i].b = Lift(px[i].b, gamma, mult);
        }
        tmp.SetPixels32(px);
        byte[] png = tmp.EncodeToPNG();
        UnityEngine.Object.DestroyImmediate(tmp);

        System.IO.File.WriteAllBytes(System.IO.Path.Combine(root, outPath), png);
        AssetDatabase.ImportAsset(outPath, ImportAssetOptions.ForceUpdate);

        TextureImporter ti = AssetImporter.GetAtPath(outPath) as TextureImporter;
        if (ti != null)
        {
            ti.wrapMode = TextureWrapMode.Repeat;
            ti.filterMode = FilterMode.Bilinear;
            ti.anisoLevel = 8;
            ti.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Texture2D>(outPath);
    }

    static byte Lift(byte v, float gamma, float mult)
    {
        float c = v / 255f;
        c = Mathf.Pow(c, gamma) * mult;
        return (byte)Mathf.RoundToInt(Mathf.Clamp01(c) * 255f);
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
}