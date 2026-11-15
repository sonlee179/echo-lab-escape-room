using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TunableBody : MonoBehaviour
{
    public string displayName = "";
    public float minMass = 0.5f;
    public float maxMass = 10f;
    public Color lightColor = new Color(0.3f, 0.8f, 1f);
    public Color heavyColor = new Color(1f, 0.3f, 0.2f);

    Rigidbody rb;
    Renderer rend;
    Vector3 baseScale;
    float baseMass;

    public float Mass
    {
        get { return rb.mass; }
    }

    public string DisplayName
    {
        get { return string.IsNullOrEmpty(displayName) ? name : displayName; }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        baseScale = transform.localScale;
        baseMass = rb.mass;
        UpdateColor();
    }

    // Đổi khối lượng: kích thước đổi theo căn bậc ba (mật độ không đổi)
    public void Change(float delta)
    {
        rb.mass = Mathf.Clamp(rb.mass + delta, minMass, maxMass);
        transform.localScale = baseScale * Mathf.Pow(rb.mass / baseMass, 1f / 3f);
        UpdateColor();
    }

    void UpdateColor()
    {
        if (rend == null) return;
        float t = Mathf.InverseLerp(minMass, maxMass, rb.mass);
        rend.material.color = Color.Lerp(lightColor, heavyColor, t);
    }
}