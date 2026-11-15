using UnityEngine;

public class LogTerminal : MonoBehaviour, IInteractable
{
    [TextArea(2, 6)]
    public string log;
    public float seconds = 6f;

    public void Interact()
    {
        Sfx.Play("terminal", 0.8f);
        if (GameManager.Instance != null)
            GameManager.Instance.ShowMessage(log, seconds);
    }

    public void StopInteract() { }
}
