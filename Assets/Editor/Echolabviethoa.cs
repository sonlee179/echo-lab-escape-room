using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class EchoLabVietHoa
{
    [MenuItem("EchoLab/7 - Viet hoa toan bo (font + chu)")]
    public static void VietHoa()
    {
        // ---- 1. Font ho tro tieng Viet ----
        TMP_FontAsset font = EnsureVietnameseFont();
        if (font == null) return;

        // ---- 2. Dat lam font mac dinh cua TextMeshPro ----
        SetDefaultFont(font);

        // ---- 3. Doi font cho moi chu trong scene (ke ca doi tuong dang tat) ----
        TMP_Text[] all = UnityEngine.Object.FindObjectsByType<TMP_Text>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            all[i].font = font;
            EditorUtility.SetDirty(all[i]);
        }

        // ---- 4. Doi noi dung chu ----
        GameManager gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();
        if (gm != null)
        {
            if (gm.keyText != null)
            {
                gm.keyText.text = "Chìa khóa: 0/" + gm.keyPiecesNeeded;
                EditorUtility.SetDirty(gm.keyText);
            }

            if (gm.winPanel != null)
            {
                SetText(gm.winPanel.transform, "Title", "THOÁT THÀNH CÔNG!");
                SetText(gm.winPanel.transform, "RestartButton/Label", "CHƠI LẠI");
            }

            if (gm.losePanel != null)
            {
                SetText(gm.losePanel.transform, "Title", "HẾT GIỜ!");
                SetText(gm.losePanel.transform, "RestartButton/Label", "CHƠI LẠI");
            }
        }

        // ---- 5. Thong bao cua gang tay va nhat ky ----
        SetGlove("Glove_Mass",
            "GĂNG TAY 1: ĐIỀU CHỈNH KHỐI LƯỢNG. Nhìn vào vật và lăn chuột để đổi khối lượng.");
        SetGlove("Glove_Echo",
            "GĂNG TAY 2: BÓNG MA THỜI GIAN. Nhấn Q để ghi lại chính bạn. Quá khứ có thể đứng thay bạn.");
        SetGlove("Glove_Gravity",
            "GĂNG TAY 3: ĐẢO TRỌNG LỰC. Nhấn F. Vật thể rơi lên trời, nhưng bạn thì không.");

        GameObject term = GameObject.Find("LogTerminal1");
        if (term != null)
        {
            LogTerminal lt = term.GetComponent<LogTerminal>();
            if (lt != null)
            {
                lt.log = "NHẬT KÝ 01: Tiến sĩ Hạo chế tạo cỗ máy Echo để sao chép bản thân xuyên thời gian. " +
                         "Nó đã thành công. Phòng thí nghiệm đang sụp đổ.";
                EditorUtility.SetDirty(lt);
            }
        }

        // ---- 6. Ten hien thi cua hop ----
        GameObject crate = GameObject.Find("crate");
        if (crate == null) crate = GameObject.Find("Crate");
        if (crate != null)
        {
            TunableBody tb = crate.GetComponent<TunableBody>();
            if (tb != null)
            {
                tb.displayName = "Hộp gỗ";
                EditorUtility.SetDirty(tb);
            }
        }

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("EchoLab",
            "Đã Việt hóa xong!\n\nFont tiếng Việt: Assets/Fonts/VN_Font SDF\nHãy Ctrl+S rồi bấm Play để kiểm tra.",
            "OK");
    }

    // =====================================================================
    static void SetGlove(string objName, string message)
    {
        GameObject go = GameObject.Find(objName);
        if (go == null) return;

        AbilityPickup ap = go.GetComponent<AbilityPickup>();
        if (ap == null) return;

        ap.message = message;
        EditorUtility.SetDirty(ap);
    }

    static void SetText(Transform root, string path, string text)
    {
        Transform t = root.Find(path);
        if (t == null) return;

        TMP_Text tmp = t.GetComponent<TMP_Text>();
        if (tmp == null) return;

        tmp.text = text;
        EditorUtility.SetDirty(tmp);
    }

    static void SetDefaultFont(TMP_FontAsset font)
    {
        TMP_Settings settings = TMP_Settings.instance;
        if (settings == null) return;

        SerializedObject so = new SerializedObject(settings);
        SerializedProperty p = so.FindProperty("m_defaultFontAsset");
        if (p == null) return;

        p.objectReferenceValue = font;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(settings);
    }

    static Font FindFontInFolder(string dir)
    {
        string[] guids = AssetDatabase.FindAssets("t:Font", new[] { dir });
        Font fallback = null;

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            Font f = AssetDatabase.LoadAssetAtPath<Font>(path);
            if (f == null) continue;

            // Uu tien font do ban tu them (khong phai arial copy tu Windows)
            if (!path.ToLower().Contains("arial")) return f;
            fallback = f;
        }
        return fallback;
    }

    static TMP_FontAsset EnsureVietnameseFont()
    {
        const string dir = "Assets/Fonts";
        const string assetPath = dir + "/VN_Font SDF.asset";

        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets", "Fonts");

        TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
        if (existing != null) return existing;

        Font src = FindFontInFolder(dir);

        // Khong co font nao: thu lay Arial cua Windows (ho tro day du tieng Viet)
        if (src == null)
        {
            string sys = System.IO.Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.Fonts),
                "arial.ttf");

            if (System.IO.File.Exists(sys))
            {
                string dest = System.IO.Path.Combine(Application.dataPath, "Fonts/arial.ttf");
                System.IO.File.Copy(sys, dest, true);
                AssetDatabase.ImportAsset("Assets/Fonts/arial.ttf");
                AssetDatabase.Refresh();
                src = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/arial.ttf");
            }
        }

        if (src == null)
        {
            EditorUtility.DisplayDialog("EchoLab",
                "Không tìm thấy font hỗ trợ tiếng Việt.\n\nHãy kéo một file font .ttf (ví dụ Be Vietnam Pro, Roboto, Arial) vào thư mục Assets/Fonts rồi chạy lại menu này.",
                "OK");
            return null;
        }

        TMP_FontAsset fa = TMP_FontAsset.CreateFontAsset(src);
        if (fa == null)
        {
            EditorUtility.DisplayDialog("EchoLab",
                "Không tạo được font asset từ font này. Hãy thử font .ttf khác.",
                "OK");
            return null;
        }

        fa.name = "VN_Font SDF";
        AssetDatabase.CreateAsset(fa, assetPath);

        if (fa.atlasTexture != null)
        {
            fa.atlasTexture.name = "VN_Font Atlas";
            AssetDatabase.AddObjectToAsset(fa.atlasTexture, fa);
        }

        if (fa.material != null)
        {
            fa.material.name = "VN_Font Material";
            AssetDatabase.AddObjectToAsset(fa.material, fa);
        }

        EditorUtility.SetDirty(fa);
        AssetDatabase.SaveAssets();
        return fa;
    }
}