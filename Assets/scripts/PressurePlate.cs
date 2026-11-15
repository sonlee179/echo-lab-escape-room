using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PressurePlate : MonoBehaviour
{
    public float requiredMass = 4f;
    public UnityEvent onActivated = new UnityEvent();
    public UnityEvent onDeactivated = new UnityEvent();

    public Renderer indicator;
    public Color offColor = Color.red;
    public Color onColor = Color.green;

    readonly HashSet<Rigidbody> bodies = new HashSet<Rigidbody>();
    bool active;

    void Start()
    {
        SetColor();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody != null)
            bodies.Add(other.attachedRigidbody);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody != null)
            bodies.Remove(other.attachedRigidbody);
    }

    void Update()
    {
        Check();
    }

    void Check()
    {
        bodies.RemoveWhere(b => b == null);

        float total = 0f;
        foreach (var b in bodies) total += b.mass;

        bool now = total >= requiredMass;
        if (now == active) return;

        active = now;
        SetColor();
        Sfx.PlayAt(active ? "plate_on" : "plate_off", transform.position, 0.9f);

        if (active) onActivated.Invoke();
        else onDeactivated.Invoke();
    }

    void SetColor()
    {
        if (indicator != null)
            indicator.material.color = active ? onColor : offColor;
    }
}
