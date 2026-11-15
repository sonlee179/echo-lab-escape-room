using UnityEngine;
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
        Sfx.PlayAt("lever", transform.position);
        if (on) onOn.Invoke();
        else onOff.Invoke();
    }
}
