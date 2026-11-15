using UnityEngine;

public class RoomIntro : MonoBehaviour
{
    public string title = "";
    public float seconds = 4f;

    void Start()
    {
        if (GameManager.Instance != null && !string.IsNullOrEmpty(title))
            GameManager.Instance.ShowMessage(title, seconds);
    }
}