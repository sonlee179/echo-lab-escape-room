using UnityEngine;

// Gương luôn đứng thẳng: chỉ xoay quanh trục dọc (giữ R và di chuột ngang để xoay)
public class LaserMirror : MonoBehaviour
{
    void LateUpdate()
    {
        float yaw = transform.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }
}