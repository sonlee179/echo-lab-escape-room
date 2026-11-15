using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public string firstRoomScene = "Room1";

    void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Play()
    {
        Sfx.Play("click");
        // Chơi mới: xóa các kỹ năng đã mở khóa
        Progress.mass = false;
        Progress.echo = false;
        Progress.gravity = false;

        SceneManager.LoadScene(firstRoomScene);
    }

    public void Quit()
    {
        Sfx.Play("click");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
