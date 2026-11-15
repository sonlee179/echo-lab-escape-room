using TMPro;
using UnityEngine;

public class MassTuner : MonoBehaviour
{
    public Camera cam;
    public TMP_Text label;
    public float range = 6f;
    public float step = 0.5f;

    // Bật để thử ngay. Khi làm đến phòng 2 (nhặt găng tay mở khóa) thì tắt đi.
    public bool debugUnlock = true;

    void Start()
    {
        enabled = Progress.mass || debugUnlock;
        if (!enabled && label != null) label.text = "";
    }

    void Update()
    {
        TunableBody t = null;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward,
                out RaycastHit hit, range, ~0, QueryTriggerInteraction.Ignore))
        {
            t = hit.collider.GetComponentInParent<TunableBody>();
        }

        if (label != null)
        {
            label.text = t != null
                ? t.DisplayName + ": " + t.Mass.ToString("0.0") + " kg   [Lăn chuột để chỉnh]"
                : "";
        }

        if (t == null) return;

        float s = Input.mouseScrollDelta.y;
        if (s != 0f) t.Change(Mathf.Sign(s) * step);
    }
}