using TMPro;
using UnityEngine;

public class GravityFlip : MonoBehaviour
{
    public float cooldown = 3f;
    public Light[] roomLights;
    public Color flippedColor = new Color(0.65f, 0.35f, 1f);
    public TMP_Text label;

    // Bật để thử ngay. Khi làm đến phòng 4 (nhặt găng tay mở khóa) thì tắt đi.
    public bool debugUnlock = true;

    bool flipped;
    float nextTime;
    Color[] originalColors;

    void Start()
    {
        Physics.gravity = new Vector3(0f, -9.81f, 0f);

        originalColors = new Color[roomLights.Length];
        for (int i = 0; i < roomLights.Length; i++)
            if (roomLights[i] != null) originalColors[i] = roomLights[i].color;

        enabled = Progress.gravity || debugUnlock;
        if (!enabled && label != null) label.text = "";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && Time.time >= nextTime)
        {
            Flip();
        }

        if (label != null)
        {
            float wait = nextTime - Time.time;
            string state = flipped ? "ĐẢO NGƯỢC" : "BÌNH THƯỜNG";
            label.text = wait > 0f
                ? "Trọng lực: " + state + "   (" + wait.ToString("0.0") + "s)"
                : "[F] Đảo trọng lực   (hiện tại: " + state + ")";
        }
    }

    void Flip()
    {
        flipped = !flipped;
        nextTime = Time.time + cooldown;
        Sfx.Play("gravity");

        // Đảo trọng lực của vật thể. Người chơi có trọng lực riêng nên không bị ảnh hưởng.
        Physics.gravity = new Vector3(0f, flipped ? 9.81f : -9.81f, 0f);

        // Đánh thức các vật đang "ngủ" để chúng bay/rơi ngay
        Collider[] cols = Physics.OverlapSphere(Vector3.zero, 500f);
        for (int i = 0; i < cols.Length; i++)
        {
            Rigidbody rb = cols[i].attachedRigidbody;
            if (rb != null && !rb.isKinematic) rb.WakeUp();
        }

        for (int i = 0; i < roomLights.Length; i++)
        {
            if (roomLights[i] == null) continue;
            roomLights[i].color = flipped ? flippedColor : originalColors[i];
        }
    }

    void OnDestroy()
    {
        Physics.gravity = new Vector3(0f, -9.81f, 0f);
    }
}
