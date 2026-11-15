using System.Collections;
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
        if (messageText != null) messageText.text = "";
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
                Sfx.Play("tick", 0.6f);
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
            Transform tr = losePanel.transform.Find("Title");
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
        messageText.text = "";
    }

    void EndGame(bool win)
    {
        ended = true;
        Sfx.Play(win ? "win" : "lose");
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (win) winPanel.SetActive(true);
        else losePanel.SetActive(true);
    }

    void UpdateUI()
    {
        int t = Mathf.CeilToInt(timeLeft);
        timerText.text = (t / 60).ToString("00") + ":" + (t % 60).ToString("00");
        timerText.color = timeLeft < 30f ? Color.red : Color.white;
        keyText.text = "Chìa khóa: " + keys + "/" + keyPiecesNeeded;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
