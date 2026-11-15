using UnityEngine;

// Đèn chập chờn nhẹ cho không khí phòng thí nghiệm
[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    public float amount = 0.08f;
    public float speed = 2.5f;

    Light lamp;
    float baseIntensity;
    float seed;
    float dipUntil;

    void Awake()
    {
        lamp = GetComponent<Light>();
        baseIntensity = lamp.intensity;
        seed = Random.value * 100f;
    }

    void Update()
    {
        float n = Mathf.PerlinNoise(Time.time * speed, seed);
        float k = 1f - amount + amount * 2f * n;

        // Thỉnh thoảng đèn tối sầm một nhịp ngắn
        if (Time.time > dipUntil && Random.value < 0.0015f)
            dipUntil = Time.time + 0.12f;
        if (Time.time < dipUntil) k *= 0.35f;

        lamp.intensity = baseIntensity * k;
    }
}
