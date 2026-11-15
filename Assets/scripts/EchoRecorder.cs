using System.Collections.Generic;
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
        int p = LayerMask.NameToLayer("Player");
        int g = LayerMask.NameToLayer("Ghost");
        if (p >= 0 && g >= 0) Physics.IgnoreLayerCollision(p, g, true);

        enabled = Progress.echo || debugUnlock;
        if (!enabled && label != null) label.text = "";
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
            SetLabel("ĐANG GHI " + (maxTime - timer).ToString("0.0") + "s   [Q để dừng]");
            if (timer >= maxTime) StopRecording();
        }
        else if (playing)
        {
            SetLabel("BÓNG MA ĐANG HOẠT ĐỘNG   [Q để ghi lại]");
        }
        else
        {
            SetLabel("[Q] Ghi bóng ma (" + maxTime.ToString("0") + "s)");
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
        Sfx.Play("rec_start", 0.7f);
        StopPlayback();
        frames.Clear();
        timer = 0f;
        recording = true;
    }

    void StopRecording()
    {
        recording = false;
        Sfx.Play("rec_stop", 0.7f);
        if (frames.Count < 2 || ghostPrefab == null) return;

        GameObject g = Instantiate(ghostPrefab, frames[0].pos, frames[0].rot);
        ghostRb = g.GetComponent<Rigidbody>();
        playIndex = 0;
        playing = true;
        Sfx.Play("ghost", 0.8f);
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
