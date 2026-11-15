using UnityEngine;
using UnityEngine.Events;

public class LaserReceiver : MonoBehaviour
{
    public UnityEvent onActivated = new UnityEvent();
    public UnityEvent onDeactivated = new UnityEvent();

    public Renderer indicator;
    public Color offColor = new Color(0.6f, 0.1f, 0.1f);
    public Color onColor = new Color(0.1f, 1f, 0.3f);

    float lastHit = -10f;
    bool active;

    void Start()
    {
        SetColor();
    }

    // Được LaserEmitter gọi mỗi khi tia chạm vào
    public void Hit()
    {
        lastHit = Time.time;
    }

    void Update()
    {
        bool now = Time.time - lastHit < 0.12f;
        if (now == active) return;

        active = now;
        SetColor();

        if (active)
        {
            Sfx.PlayAt("laser_ok", transform.position);
            onActivated.Invoke();
        }
        else
        {
            onDeactivated.Invoke();
        }
    }

    void SetColor()
    {
        if (indicator != null)
            indicator.material.color = active ? onColor : offColor;
    }
}
