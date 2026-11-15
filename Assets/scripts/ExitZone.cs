using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        int current = SceneManager.GetActiveScene().buildIndex;
        int next = current + 1;

        // Còn phòng tiếp theo thì sang phòng đó, hết phòng thì thắng game
        if (current >= 0 && next < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(next);
        else if (GameManager.Instance != null)
            GameManager.Instance.Win();
    }
}