using UnityEngine;
using UnityEngine.Events;

public class SequencePuzzle : MonoBehaviour
{
    // Các cần gạt, xếp theo THỨ TỰ ĐÚNG
    public Lever[] levers;
    public UnityEvent onSolved = new UnityEvent();

    public string wrongMessage = "Sai thứ tự! Tất cả cần gạt đã bị đặt lại.";
    public string solvedMessage = "Cơ chế đã mở!";

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
                Sfx.Play("solved");
                if (GameManager.Instance != null)
                    GameManager.Instance.ShowMessage(solvedMessage, 3f);
                onSolved.Invoke();
            }
        }
        else
        {
            next = 0;
            Sfx.Play("wrong");
            for (int i = 0; i < levers.Length; i++)
                if (levers[i] != null) levers[i].ForceOff();

            if (GameManager.Instance != null)
                GameManager.Instance.ShowMessage(wrongMessage, 3f);
        }
    }
}
