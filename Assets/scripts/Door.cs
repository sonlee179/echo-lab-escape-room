using UnityEngine;

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
        Sfx.PlayAt(open ? "door_open" : "door_close", transform.position);
    }

    void Update()
    {
        Vector3 target = currentInputs >= requiredInputs ? openPos : closedPos;
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }
}
