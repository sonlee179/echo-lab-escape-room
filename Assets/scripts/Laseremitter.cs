using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserEmitter : MonoBehaviour
{
    public int maxReflections = 8;
    public float maxDistance = 60f;

    // Tick vào để tia laser (kể cả đoạn đã phản xạ qua gương) giết người chơi khi chạm vào
    public bool deadly = false;

    // Thời gian chạm liên tục tối thiểu mới bị giết (tránh chết oan khi tia quét qua rất nhanh)
    public float killDelay = 0.12f;

    LineRenderer line;
    readonly List<Vector3> points = new List<Vector3>();
    float hitTimer;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;

        // Tiếng ù của tia laser (âm thanh 3D, nghe rõ khi đứng gần)
        AudioClip hum = Sfx.GetClip("laser_hum");
        if (hum != null)
        {
            AudioSource a = gameObject.AddComponent<AudioSource>();
            a.clip = hum;
            a.loop = true;
            a.spatialBlend = 1f;
            a.minDistance = 1.5f;
            a.maxDistance = 14f;
            a.volume = 0.25f;
            a.Play();
        }
    }

    void Update()
    {
        points.Clear();

        // Tia laser đi theo hướng trục X (đỏ) của khối phát
        Vector3 dir = transform.right;
        Vector3 origin = transform.position + dir * 0.35f;
        points.Add(origin);

        bool playerHit = false;

        for (int i = 0; i <= maxReflections; i++)
        {
            RaycastHit hit;
            if (!Physics.Raycast(origin, dir, out hit, maxDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                points.Add(origin + dir * maxDistance);
                break;
            }

            points.Add(hit.point);

            // Chạm người chơi
            if (hit.collider.CompareTag("Player"))
            {
                playerHit = true;
                break;
            }

            // Chạm ô thu
            LaserReceiver rec = hit.collider.GetComponentInParent<LaserReceiver>();
            if (rec != null)
            {
                rec.Hit();
                break;
            }

            // Chạm gương thì phản xạ, chạm vật khác thì dừng
            LaserMirror mirror = hit.collider.GetComponentInParent<LaserMirror>();
            if (mirror == null) break;

            dir = Vector3.Reflect(dir, hit.normal);
            origin = hit.point + dir * 0.02f;
        }

        // Giết người chơi nếu chạm tia đủ lâu
        if (deadly && playerHit)
        {
            hitTimer += Time.deltaTime;
            if (hitTimer >= killDelay && GameManager.Instance != null)
                GameManager.Instance.Lose("BỊ LASER CẮT ĐÔI!");
        }
        else
        {
            hitTimer = 0f;
        }

        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
    }
}
