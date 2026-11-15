// Menu: EchoLab -> 15 - Cai dat am thanh (ghi de script)
// Cong cu nay GHI DE noi dung cac script co am thanh vao dung file dang co trong project
// (giu nguyen lien ket voi scene), roi kiem tra du file am thanh hay chua.
using UnityEditor;
using UnityEngine;

public static class EchoLabInstallAudio
{
    struct Entry
    {
        public string cls;
        public string src;
        public Entry(string c, string s) { cls = c; src = s; }
    }

    [MenuItem("EchoLab/15 - Cai dat am thanh (ghi de script)")]
    public static void Install()
    {
        if (!EditorUtility.DisplayDialog("EchoLab",
                "Công cụ sẽ ghi đè 17 script (cửa, tấm áp suất, cần gạt, nhặt/ném vật, bước chân, laser, cổng, " +
                "GameManager, Sfx...) bằng bản có âm thanh, giữ nguyên liên kết trong scene.\n\nTiếp tục?",
                "Tiếp tục", "Hủy"))
            return;

        Entry[] entries =
        {
            new Entry("AbilityPickup", S_AbilityPickup),
            new Entry("Door", S_Door),
            new Entry("EchoRecorder", S_EchoRecorder),
            new Entry("FirstPersonController", S_FirstPersonController),
            new Entry("GameManager", S_GameManager),
            new Entry("GrabSystem", S_GrabSystem),
            new Entry("GravityFlip", S_GravityFlip),
            new Entry("KeyPiece", S_KeyPiece),
            new Entry("LaserReceiver", S_LaserReceiver),
            new Entry("Lever", S_Lever),
            new Entry("LogTerminal", S_LogTerminal),
            new Entry("MenuManager", S_MenuManager),
            new Entry("Portal", S_Portal),
            new Entry("PressurePlate", S_PressurePlate),
            new Entry("SequencePuzzle", S_SequencePuzzle),
            new Entry("LaserEmitter", S_LaserEmitter),
            new Entry("Sfx", S_Sfx)
        };

        string root = System.IO.Path.GetDirectoryName(UnityEngine.Application.dataPath);
        int updated = 0;
        int created = 0;
        string createdList = "";

        for (int i = 0; i < entries.Length; i++)
        {
            string path = FindScriptPath(entries[i].cls);
            if (path == null)
            {
                if (!AssetDatabase.IsValidFolder("Assets/scripts"))
                    AssetDatabase.CreateFolder("Assets", "scripts");
                path = "Assets/scripts/" + entries[i].cls + ".cs";
                created++;
                createdList += entries[i].cls + " ";
            }
            else
            {
                updated++;
            }

            System.IO.File.WriteAllText(
                System.IO.Path.Combine(root, path),
                entries[i].src,
                new System.Text.UTF8Encoding(false));
        }

        string missing = MissingClips();

        string msg = "Đã ghi đè " + updated + " script có sẵn";
        if (created > 0) msg += " và tạo mới " + created + " script (" + createdList.Trim() + ")";
        msg += ".\n\n";

        if (missing.Length == 0)
            msg += "Đã đủ file âm thanh trong Assets/Resources.";
        else
            msg += "THIẾU file âm thanh: " + missing + "\nHãy chép thư mục Audio2 vào Assets/Resources.";

        msg += "\n\nSau khi Unity biên dịch xong, bấm Play để nghe thử.";

        EditorUtility.DisplayDialog("EchoLab", msg, "OK");
        AssetDatabase.Refresh();
    }

    // Tim duong dan script theo ten class (khong phu thuoc ten file)
    static string FindScriptPath(string className)
    {
        string[] guids = AssetDatabase.FindAssets("t:MonoScript");
        for (int i = 0; i < guids.Length; i++)
        {
            string p = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (!p.StartsWith("Assets/")) continue;

            MonoScript ms = AssetDatabase.LoadAssetAtPath<MonoScript>(p);
            if (ms == null) continue;

            System.Type t = ms.GetClass();
            if (t != null && t.Name == className) return p;
        }
        return null;
    }

    // Tra ve danh sach am thanh con thieu (rong neu du)
    static string MissingClips()
    {
        string[] need =
        {
            "plate_on", "plate_off", "door_open", "door_close", "lever", "pickup", "grab", "drop",
            "throw", "portal", "gravity", "rec_start", "rec_stop", "ghost", "laser_ok", "laser_hum",
            "wrong", "solved", "win", "lose", "tick", "terminal", "jump", "step", "click", "music_ambient"
        };

        System.Collections.Generic.HashSet<string> have = new System.Collections.Generic.HashSet<string>();
        string[] guids = AssetDatabase.FindAssets("t:AudioClip");
        for (int i = 0; i < guids.Length; i++)
        {
            string p = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (p.Contains("/Resources/Audio2/") || p.Contains("/Resources/Audio/"))
                have.Add(System.IO.Path.GetFileNameWithoutExtension(p));
        }

        string missing = "";
        for (int i = 0; i < need.Length; i++)
            if (!have.Contains(need[i])) missing += need[i] + " ";

        return missing.Trim();
    }

    const string S_AbilityPickup = @"using UnityEngine;

public enum Ability
{
    MassTuner,
    TimeEcho,
    GravityFlip
}

public class AbilityPickup : MonoBehaviour, IInteractable
{
    public Ability ability;

    [TextArea(2, 5)]
    public string message;

    void Update()
    {
        transform.Rotate(0f, 60f * Time.deltaTime, 0f, Space.World);
    }

    public void Interact()
    {
        Sfx.Play(""pickup"");
        GameObject p = GameObject.FindWithTag(""Player"");

        if (p != null)
        {
            switch (ability)
            {
                case Ability.MassTuner:
                    Progress.mass = true;
                    MassTuner m = p.GetComponent<MassTuner>();
                    if (m != null) m.enabled = true;
                    break;

                case Ability.TimeEcho:
                    Progress.echo = true;
                    EchoRecorder e = p.GetComponent<EchoRecorder>();
                    if (e != null) e.enabled = true;
                    break;

                case Ability.GravityFlip:
                    Progress.gravity = true;
                    GravityFlip g = p.GetComponent<GravityFlip>();
                    if (g != null) g.enabled = true;
                    break;
            }
        }

        if (GameManager.Instance != null)
            GameManager.Instance.ShowMessage(message, 5f);

        Destroy(gameObject);
    }

    public void StopInteract() { }
}
";
    const string S_Door = @"using UnityEngine;

public class Door : MonoBehaviour
{
    public int requiredInputs = 1;
    public Vector3 openOffset = new Vector3(0f, 3.5f, 0f);
    public float speed = 2f;

    // Chỉ để quan sát trong Inspector khi bấm Play (số điều kiện đang thỏa)
    public int currentInputs;

    Vector3 closedPos;
    Vector3 openPos;
    bool wasOpen;

    void Start()
    {
        closedPos = transform.position;
        openPos = closedPos + openOffset;
    }

    public void AddInput()
    {
        currentInputs++;
        Notify();
    }

    public void RemoveInput()
    {
        currentInputs = Mathf.Max(0, currentInputs - 1);
        Notify();
    }

    // Phát tiếng khi cửa chuyển trạng thái mở/đóng
    void Notify()
    {
        bool open = currentInputs >= requiredInputs;
        if (open == wasOpen) return;

        wasOpen = open;
        Sfx.PlayAt(open ? ""door_open"" : ""door_close"", transform.position);
    }

    void Update()
    {
        Vector3 target = currentInputs >= requiredInputs ? openPos : closedPos;
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }
}
";
    const string S_EchoRecorder = @"using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EchoRecorder : MonoBehaviour
{
    public GameObject ghostPrefab;
    public TMP_Text label;
    public float maxTime = 10f;

    // Bật để thử ngay. Khi làm đến phòng 3 (nhặt găng tay mở khóa) thì tắt đi.
    public bool debugUnlock = true;

    struct Frame
    {
        public Vector3 pos;
        public Quaternion rot;
    }

    readonly List<Frame> frames = new List<Frame>();
    bool recording;
    bool playing;
    float timer;
    int playIndex;
    Rigidbody ghostRb;

    void Start()
    {
        // Bóng ma không va chạm với người chơi (không đẩy bạn khi xuất hiện)
        int p = LayerMask.NameToLayer(""Player"");
        int g = LayerMask.NameToLayer(""Ghost"");
        if (p >= 0 && g >= 0) Physics.IgnoreLayerCollision(p, g, true);

        enabled = Progress.echo || debugUnlock;
        if (!enabled && label != null) label.text = """";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (recording) StopRecording();
            else StartRecording();
        }

        if (recording)
        {
            timer += Time.deltaTime;
            SetLabel(""ĐANG GHI "" + (maxTime - timer).ToString(""0.0"") + ""s   [Q để dừng]"");
            if (timer >= maxTime) StopRecording();
        }
        else if (playing)
        {
            SetLabel(""BÓNG MA ĐANG HOẠT ĐỘNG   [Q để ghi lại]"");
        }
        else
        {
            SetLabel(""[Q] Ghi bóng ma ("" + maxTime.ToString(""0"") + ""s)"");
        }
    }

    void FixedUpdate()
    {
        if (recording)
        {
            Frame f = new Frame();
            f.pos = transform.position;
            f.rot = transform.rotation;
            frames.Add(f);
        }
        else if (playing && ghostRb != null)
        {
            // Hết băng thì đứng yên ở khung cuối (bóng ma đứng đè tấm áp suất)
            int i = Mathf.Min(playIndex, frames.Count - 1);
            playIndex++;
            ghostRb.MovePosition(frames[i].pos);
            ghostRb.MoveRotation(frames[i].rot);
        }
    }

    void StartRecording()
    {
        Sfx.Play(""rec_start"", 0.7f);
        StopPlayback();
        frames.Clear();
        timer = 0f;
        recording = true;
    }

    void StopRecording()
    {
        recording = false;
        Sfx.Play(""rec_stop"", 0.7f);
        if (frames.Count < 2 || ghostPrefab == null) return;

        GameObject g = Instantiate(ghostPrefab, frames[0].pos, frames[0].rot);
        ghostRb = g.GetComponent<Rigidbody>();
        playIndex = 0;
        playing = true;
        Sfx.Play(""ghost"", 0.8f);
    }

    void StopPlayback()
    {
        playing = false;
        if (ghostRb != null) Destroy(ghostRb.gameObject);
        ghostRb = null;
    }

    void SetLabel(string s)
    {
        if (label != null) label.text = s;
    }
}
";
    const string S_FirstPersonController = @"using UnityEngine;

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
            float mx = Input.GetAxis(""Mouse X"") * mouseSensitivity;
            float my = Input.GetAxis(""Mouse Y"") * mouseSensitivity;
            xRot = Mathf.Clamp(xRot - my, -85f, 85f);
            cam.localRotation = Quaternion.Euler(xRot, 0f, 0f);
            transform.Rotate(Vector3.up * mx);
        }

        // Hướng đi ngang
        float h = Input.GetAxis(""Horizontal"");
        float v = Input.GetAxis(""Vertical"");
        Vector3 move = transform.right * h + transform.forward * v;

        // Trọng lực và nhảy
        bool grounded = cc.isGrounded;
        if (grounded && vel.y < 0f) vel.y = -2f;

        if (grounded && Input.GetKeyDown(KeyCode.Space))
        {
            vel.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            Sfx.Play(""jump"", 0.5f);
        }

        vel.y += gravity * Time.deltaTime;

        // MỘT lần Move duy nhất cho cả đi ngang và rơi/nhảy
        Vector3 total = move * speed + vel;
        cc.Move(total * Time.deltaTime);

        // Tiếng chạm đất khi rơi xuống
        if (grounded && !wasGrounded) Sfx.Play(""step"", 0.8f, 0.8f);
        wasGrounded = grounded;

        // Tiếng bước chân
        bool moving = grounded && (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f);
        if (moving)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                stepTimer = stepInterval;
                Sfx.Play(""step"", 0.35f, Random.Range(0.85f, 1.15f));
            }
        }
        else
        {
            stepTimer = 0.1f;
        }
    }
}
";
    const string S_GameManager = @"using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float timeLimit = 300f;
    public int keyPiecesNeeded = 3;
    public TMP_Text timerText;
    public TMP_Text keyText;
    public TMP_Text messageText;
    public GameObject winPanel;
    public GameObject losePanel;
    public UnityEvent onAllKeysCollected;

    float timeLeft;
    int keys;
    bool ended;
    Coroutine msgRoutine;
    int lastTickSecond = -1;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        timeLeft = timeLimit;
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (messageText != null) messageText.text = """";
        UpdateUI();
    }

    void Update()
    {
        if (ended) return;

        timeLeft -= Time.deltaTime;

        // Tiếng tích tắc 10 giây cuối
        if (timeLeft > 0f && timeLeft < 10f)
        {
            int sec = Mathf.CeilToInt(timeLeft);
            if (sec != lastTickSecond)
            {
                lastTickSecond = sec;
                Sfx.Play(""tick"", 0.6f);
            }
        }
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            EndGame(false);
        }
        UpdateUI();
    }

    public void AddKeyPiece()
    {
        keys++;
        UpdateUI();
        if (keys == keyPiecesNeeded) onAllKeysCollected.Invoke();
    }

    public void Win()
    {
        if (!ended) EndGame(true);
    }

    // Thua có lý do riêng (ví dụ bị laser). Hết giờ vẫn dùng chữ mặc định.
    public void Lose(string reason)
    {
        if (ended) return;

        if (losePanel != null)
        {
            Transform tr = losePanel.transform.Find(""Title"");
            if (tr != null)
            {
                TMP_Text tt = tr.GetComponent<TMP_Text>();
                if (tt != null) tt.text = reason;
            }
        }
        EndGame(false);
    }

    public void ShowMessage(string msg, float seconds = 4f)
    {
        if (messageText == null) return;
        if (msgRoutine != null) StopCoroutine(msgRoutine);
        msgRoutine = StartCoroutine(MsgRoutine(msg, seconds));
    }

    IEnumerator MsgRoutine(string m, float s)
    {
        messageText.text = m;
        yield return new WaitForSecondsRealtime(s);
        messageText.text = """";
    }

    void EndGame(bool win)
    {
        ended = true;
        Sfx.Play(win ? ""win"" : ""lose"");
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (win) winPanel.SetActive(true);
        else losePanel.SetActive(true);
    }

    void UpdateUI()
    {
        int t = Mathf.CeilToInt(timeLeft);
        timerText.text = (t / 60).ToString(""00"") + "":"" + (t % 60).ToString(""00"");
        timerText.color = timeLeft < 30f ? Color.red : Color.white;
        keyText.text = ""Chìa khóa: "" + keys + ""/"" + keyPiecesNeeded;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
";
    const string S_GrabSystem = @"using UnityEngine;

public interface IInteractable
{
    void Interact();
    void StopInteract();
}

public class GrabSystem : MonoBehaviour
{
    public static GrabSystem Instance;
    public static bool IsRotating;

    public Camera cam;
    public Transform holdPoint;
    public float reach = 3f;
    public float followSpeed = 12f;
    public float throwForce = 15f;
    public float rotateSpeed = 5f;
    public float maxGrabMass = 10f;

    Rigidbody held;
    IInteractable active;

    void Awake() { Instance = this; }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (held != null) Drop();
            else TryInteract();
        }

        if (Input.GetKeyUp(KeyCode.E) && active != null)
        {
            active.StopInteract();
            active = null;
        }

        IsRotating = held != null && Input.GetKey(KeyCode.R);
        if (held == null) return;

        if (Input.GetMouseButtonDown(0)) { Throw(); return; }

        if (IsRotating)
        {
            float mx = Input.GetAxis(""Mouse X"");
            float my = Input.GetAxis(""Mouse Y"");
            held.transform.Rotate(cam.transform.up, -mx * rotateSpeed, Space.World);
            held.transform.Rotate(cam.transform.right, my * rotateSpeed, Space.World);
        }
    }

    void FixedUpdate()
    {
        if (held == null) return;
        Vector3 d = holdPoint.position - held.position;
        held.linearVelocity = Vector3.ClampMagnitude(d * followSpeed, 15f);
        if (d.magnitude > 1.5f) Drop();   // bị kẹt thì tự thả
    }

    void TryInteract()
    {
        if (!Physics.Raycast(cam.transform.position, cam.transform.forward,
                out RaycastHit hit, reach, ~0, QueryTriggerInteraction.Ignore)) return;

        var inter = hit.collider.GetComponentInParent<IInteractable>();
        if (inter != null) { inter.Interact(); active = inter; return; }

        Rigidbody rb = hit.rigidbody;
        if (rb != null && !rb.isKinematic && rb.mass <= maxGrabMass) Grab(rb);
    }

    void Grab(Rigidbody rb)
    {
        held = rb;
        held.useGravity = false;
        held.freezeRotation = true;
        Sfx.Play(""grab"", 0.7f);
    }

    void Drop()
    {
        if (held == null) return;
        held.useGravity = true;
        held.freezeRotation = false;
        held = null;
        Sfx.Play(""drop"", 0.6f);
    }

    void Throw()
    {
        Rigidbody rb = held;
        Drop();
        rb.AddForce(cam.transform.forward * throwForce, ForceMode.Impulse);
        Sfx.Play(""throw"", 0.8f);
    }

    public void ForceDrop(Rigidbody rb)
    {
        if (held == rb) Drop();
    }
}
";
    const string S_GravityFlip = @"using TMPro;
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
        if (!enabled && label != null) label.text = """";
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
            string state = flipped ? ""ĐẢO NGƯỢC"" : ""BÌNH THƯỜNG"";
            label.text = wait > 0f
                ? ""Trọng lực: "" + state + ""   ("" + wait.ToString(""0.0"") + ""s)""
                : ""[F] Đảo trọng lực   (hiện tại: "" + state + "")"";
        }
    }

    void Flip()
    {
        flipped = !flipped;
        nextTime = Time.time + cooldown;
        Sfx.Play(""gravity"");

        // Đảo trọng lực của vật thể. Người chơi có trọng lực riêng nên không bị ảnh hưởng.
        Physics.gravity = new Vector3(0f, flipped ? 9.81f : -9.81f, 0f);

        // Đánh thức các vật đang ""ngủ"" để chúng bay/rơi ngay
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
";
    const string S_KeyPiece = @"using UnityEngine;

public class KeyPiece : MonoBehaviour, IInteractable
{
    void Update()
    {
        transform.Rotate(0f, 80f * Time.deltaTime, 0f, Space.World);
    }

    public void Interact()
    {
        Sfx.Play(""pickup"");
        GameManager.Instance.AddKeyPiece();
        Destroy(gameObject);
    }

    public void StopInteract() { }
}
";
    const string S_LaserReceiver = @"using UnityEngine;
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
            Sfx.PlayAt(""laser_ok"", transform.position);
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
";
    const string S_Lever = @"using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour, IInteractable
{
    public bool momentary;          // true = phải GIỮ phím E mới có tác dụng
    public Transform handle;
    public UnityEvent onOn = new UnityEvent();
    public UnityEvent onOff = new UnityEvent();

    bool on;

    void Start()
    {
        SetState(false, false);
    }

    public void Interact()
    {
        if (momentary) SetState(true, true);
        else SetState(!on, true);
    }

    public void StopInteract()
    {
        if (momentary) SetState(false, true);
    }

    // Đặt cần gạt về vị trí tắt mà không kích hoạt sự kiện (dùng khi reset câu đố)
    public void ForceOff()
    {
        SetState(false, false);
    }

    void SetState(bool value, bool fire)
    {
        on = value;
        if (handle != null)
            handle.localRotation = Quaternion.Euler(on ? 45f : -45f, 0f, 0f);

        if (!fire) return;
        Sfx.PlayAt(""lever"", transform.position);
        if (on) onOn.Invoke();
        else onOff.Invoke();
    }
}
";
    const string S_LogTerminal = @"using UnityEngine;

public class LogTerminal : MonoBehaviour, IInteractable
{
    [TextArea(2, 6)]
    public string log;
    public float seconds = 6f;

    public void Interact()
    {
        Sfx.Play(""terminal"", 0.8f);
        if (GameManager.Instance != null)
            GameManager.Instance.ShowMessage(log, seconds);
    }

    public void StopInteract() { }
}
";
    const string S_MenuManager = @"using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public string firstRoomScene = ""Room1"";

    void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Play()
    {
        Sfx.Play(""click"");
        // Chơi mới: xóa các kỹ năng đã mở khóa
        Progress.mass = false;
        Progress.echo = false;
        Progress.gravity = false;

        SceneManager.LoadScene(firstRoomScene);
    }

    public void Quit()
    {
        Sfx.Play(""click"");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
";
    const string S_Portal = @"using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Portal target;
    public Transform exitPoint;

    static readonly Dictionary<GameObject, float> cooldowns = new Dictionary<GameObject, float>();

    void OnTriggerEnter(Collider other)
    {
        if (target == null || target.exitPoint == null) return;

        bool isPlayer = other.CompareTag(""Player"");
        Rigidbody rb = other.attachedRigidbody;

        if (!isPlayer && (rb == null || rb.isKinematic)) return;

        GameObject who = isPlayer ? other.gameObject : rb.gameObject;

        float until;
        if (cooldowns.TryGetValue(who, out until) && Time.time < until) return;
        cooldowns[who] = Time.time + 0.7f;

        Transform exit = target.exitPoint;
        Sfx.PlayAt(""portal"", transform.position);
        Sfx.PlayAt(""portal"", exit.position);

        if (isPlayer)
        {
            CharacterController cc = who.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            who.transform.position = exit.position;
            who.transform.rotation = Quaternion.Euler(0f, exit.eulerAngles.y, 0f);

            if (cc != null) cc.enabled = true;
        }
        else
        {
            // Vật thể giữ nguyên tốc độ, bay ra theo hướng cổng đích
            float speed = rb.linearVelocity.magnitude;
            rb.position = exit.position;
            rb.transform.position = exit.position;
            rb.linearVelocity = exit.forward * speed;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
";
    const string S_PressurePlate = @"using System.Collections.Generic;
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
        Sfx.PlayAt(active ? ""plate_on"" : ""plate_off"", transform.position, 0.9f);

        if (active) onActivated.Invoke();
        else onDeactivated.Invoke();
    }

    void SetColor()
    {
        if (indicator != null)
            indicator.material.color = active ? onColor : offColor;
    }
}
";
    const string S_SequencePuzzle = @"using UnityEngine;
using UnityEngine.Events;

public class SequencePuzzle : MonoBehaviour
{
    // Các cần gạt, xếp theo THỨ TỰ ĐÚNG
    public Lever[] levers;
    public UnityEvent onSolved = new UnityEvent();

    public string wrongMessage = ""Sai thứ tự! Tất cả cần gạt đã bị đặt lại."";
    public string solvedMessage = ""Cơ chế đã mở!"";

    int next;
    bool solved;

    // Được mỗi cần gạt gọi khi bật, kèm vị trí của nó trong thứ tự đúng
    public void Pressed(int index)
    {
        if (solved) return;

        if (index == next)
        {
            next++;
            if (next >= levers.Length)
            {
                solved = true;
                Sfx.Play(""solved"");
                if (GameManager.Instance != null)
                    GameManager.Instance.ShowMessage(solvedMessage, 3f);
                onSolved.Invoke();
            }
        }
        else
        {
            next = 0;
            Sfx.Play(""wrong"");
            for (int i = 0; i < levers.Length; i++)
                if (levers[i] != null) levers[i].ForceOff();

            if (GameManager.Instance != null)
                GameManager.Instance.ShowMessage(wrongMessage, 3f);
        }
    }
}
";
    const string S_LaserEmitter = @"using System.Collections.Generic;
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
        AudioClip hum = Sfx.GetClip(""laser_hum"");
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
            if (hit.collider.CompareTag(""Player""))
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
                GameManager.Instance.Lose(""BỊ LASER CẮT ĐÔI!"");
        }
        else
        {
            hitTimer = 0f;
        }

        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
    }
}
";
    const string S_Sfx = @"using System.Collections.Generic;
using UnityEngine;

// Quản lý âm thanh toàn game. Tự khởi tạo khi game chạy, không cần đặt vào scene.
// Âm thanh nằm trong Assets/Resources/Audio2 (ưu tiên) hoặc Assets/Resources/Audio.
// Phím M: bật/tắt nhạc nền.
public class Sfx : MonoBehaviour
{
    static Sfx instance;

    // Chỉnh âm lượng tổng ở đây (0 đến 1)
    public float sfxVolume = 0.7f;
    public float musicVolume = 0.22f;

    AudioSource musicSource;
    AudioSource[] pool;
    int poolIndex;
    readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Boot()
    {
        if (instance != null) return;
        GameObject go = new GameObject(""Sfx"");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<Sfx>();
    }

    void Awake()
    {
        pool = new AudioSource[10];
        for (int i = 0; i < pool.Length; i++)
        {
            pool[i] = gameObject.AddComponent<AudioSource>();
            pool[i].playOnAwake = false;
            pool[i].spatialBlend = 0f;
        }

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = musicVolume;
        musicSource.clip = Load(""music_ambient"");
        if (musicSource.clip != null) musicSource.Play();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            musicSource.mute = !musicSource.mute;
    }

    AudioClip Load(string clipName)
    {
        AudioClip clip;
        if (cache.TryGetValue(clipName, out clip)) return clip;

        clip = Resources.Load<AudioClip>(""Audio2/"" + clipName);
        if (clip == null) clip = Resources.Load<AudioClip>(""Audio/"" + clipName);

        if (clip == null)
            UnityEngine.Debug.LogWarning(""Sfx: không tìm thấy âm thanh '"" + clipName + ""' trong Assets/Resources/Audio2 hoặc Audio"");

        cache[clipName] = clip;
        return clip;
    }

    // Âm thanh 2D (không có vị trí)
    public static void Play(string clipName, float volume = 1f, float pitch = 1f)
    {
        if (instance == null) return;

        AudioClip clip = instance.Load(clipName);
        if (clip == null) return;

        AudioSource s = instance.pool[instance.poolIndex];
        instance.poolIndex = (instance.poolIndex + 1) % instance.pool.Length;

        s.pitch = pitch;
        s.volume = volume * instance.sfxVolume;
        s.clip = clip;
        s.Play();
    }

    // Âm thanh 3D tại một vị trí: nhỏ dần theo khoảng cách (2m đến 30m)
    public static void PlayAt(string clipName, Vector3 position, float volume = 1f)
    {
        if (instance == null) return;

        AudioClip clip = instance.Load(clipName);
        if (clip == null) return;

        GameObject go = new GameObject(""Sfx3D"");
        go.transform.position = position;

        AudioSource a = go.AddComponent<AudioSource>();
        a.clip = clip;
        a.volume = volume * instance.sfxVolume;
        a.spatialBlend = 0.85f;
        a.rolloffMode = AudioRolloffMode.Linear;
        a.minDistance = 2f;
        a.maxDistance = 30f;
        a.Play();

        Destroy(go, clip.length + 0.3f);
    }

    // Dùng cho vòng lặp của vật thể (ví dụ tiếng ù của laser)
    public static AudioClip GetClip(string clipName)
    {
        return instance != null ? instance.Load(clipName) : null;
    }
}
";
}