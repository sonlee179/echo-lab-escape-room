// Menu: EchoLab -> 14 - Laser chet nguoi (Phong 5 va 8)
// Mo scene Room1 (hoac bat ky Room nao) truoc khi chay.
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class EchoLabLaserDeadly
{
    [MenuItem("EchoLab/14 - Laser chet nguoi (Phong 5 va 8)")]
    public static void MakeDeadly()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string originalPath = SceneManager.GetActiveScene().path;
        string dir = System.IO.Path.GetDirectoryName(originalPath).Replace('\\', '/');

        int changed = 0;
        int[] rooms = { 5, 8 };

        for (int r = 0; r < rooms.Length; r++)
        {
            int n = rooms[r];
            string path = dir + "/Room" + n + ".unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) continue;

            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            // Tia puzzle phai nam ngoai duong xuat phat cua nguoi choi (z = -4)
            float puzzleZ = n == 5 ? -5.0f : -5.2f;

            LaserEmitter[] emitters = Resources.FindObjectsOfTypeAll<LaserEmitter>();
            for (int i = 0; i < emitters.Length; i++)
            {
                LaserEmitter em = emitters[i];
                if (em.gameObject.scene != scene) continue;

                em.deadly = true;
                em.killDelay = 0.12f;

                // Day tia puzzle ra xa cho spawn (tia "DeadlyLaser" o phong 8 giu nguyen)
                if (em.name.ToLower() == "laseremitter")
                {
                    Vector3 p = em.transform.position;
                    em.transform.position = new Vector3(p.x, p.y, puzzleZ);
                }

                // Tia day hon, de nhin thay nguy hiem
                LineRenderer lr = em.GetComponent<LineRenderer>();
                if (lr != null)
                {
                    lr.startWidth = 0.09f;
                    lr.endWidth = 0.09f;
                    EditorUtility.SetDirty(lr);
                }

                EditorUtility.SetDirty(em);
                changed++;
            }

            // Phong 8: dich 2 manh chia khoa ra xa tia laser puzzle
            if (n == 8)
            {
                Transform[] all = Resources.FindObjectsOfTypeAll<Transform>();
                for (int i = 0; i < all.Length; i++)
                {
                    if (all[i].gameObject.scene != scene) continue;

                    string nm = all[i].name;
                    if (nm == "KeyPiece1" || nm == "KeyPiece2")
                    {
                        Vector3 p = all[i].position;
                        all[i].position = new Vector3(p.x, p.y, -3f);
                    }
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        if (!string.IsNullOrEmpty(originalPath))
            EditorSceneManager.OpenScene(originalPath, OpenSceneMode.Single);

        EditorUtility.DisplayDialog("EchoLab",
            "Xong! Đã bật chế độ chết người cho " + changed + " tia laser ở Phòng 5 và Phòng 8.\n\n" +
            "Chạm tia laser trên 0,12 giây là thua.",
            "OK");
    }
}