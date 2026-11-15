using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    public Transform cam;
    public float speed = 5f;
    public float mouseSensitivity = 2f;
    public float gravity = -20f;
    public float jumpHeight = 1.2f;
    public float stepInterval = 0.45f;

    CharacterController cc;
    float xRot;
    Vector3 vel;
    float stepTimer;
    bool wasGrounded = true;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        // Nhìn quanh (tạm dừng khi đang xoay vật bằng phím R)
        if (!GrabSystem.IsRotating)
        {
            float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
            float my = Input.GetAxis("Mouse Y") * mouseSensitivity;
            xRot = Mathf.Clamp(xRot - my, -85f, 85f);
            cam.localRotation = Quaternion.Euler(xRot, 0f, 0f);
            transform.Rotate(Vector3.up * mx);
        }

        // Hướng đi ngang
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = transform.right * h + transform.forward * v;

        // Trọng lực và nhảy
        bool grounded = cc.isGrounded;
        if (grounded && vel.y < 0f) vel.y = -2f;

        if (grounded && Input.GetKeyDown(KeyCode.Space))
        {
            vel.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            Sfx.Play("jump", 0.5f);
        }

        vel.y += gravity * Time.deltaTime;

        // MỘT lần Move duy nhất cho cả đi ngang và rơi/nhảy
        Vector3 total = move * speed + vel;
        cc.Move(total * Time.deltaTime);

        // Tiếng chạm đất khi rơi xuống
        if (grounded && !wasGrounded) Sfx.Play("step", 0.8f, 0.8f);
        wasGrounded = grounded;

        // Tiếng bước chân
        bool moving = grounded && (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f);
        if (moving)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                stepTimer = stepInterval;
                Sfx.Play("step", 0.35f, Random.Range(0.85f, 1.15f));
            }
        }
        else
        {
            stepTimer = 0.1f;
        }
    }
}
