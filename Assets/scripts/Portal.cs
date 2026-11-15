using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Portal target;
    public Transform exitPoint;

    static readonly Dictionary<GameObject, float> cooldowns = new Dictionary<GameObject, float>();

    void OnTriggerEnter(Collider other)
    {
        if (target == null || target.exitPoint == null) return;

        bool isPlayer = other.CompareTag("Player");
        Rigidbody rb = other.attachedRigidbody;

        if (!isPlayer && (rb == null || rb.isKinematic)) return;

        GameObject who = isPlayer ? other.gameObject : rb.gameObject;

        float until;
        if (cooldowns.TryGetValue(who, out until) && Time.time < until) return;
        cooldowns[who] = Time.time + 0.7f;

        Transform exit = target.exitPoint;
        Sfx.PlayAt("portal", transform.position);
        Sfx.PlayAt("portal", exit.position);

        if (isPlayer)
        {
            CharacterController cc = who.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            who.transform.position = exit.position;
            who.transform.rotation = Quaternion.Euler(0f, exit.eulerAngles.y, 0f);

            if (cc != null) cc.enabled = true;
        }
        else
        {
            // Vật thể giữ nguyên tốc độ, bay ra theo hướng cổng đích
            float speed = rb.linearVelocity.magnitude;
            rb.position = exit.position;
            rb.transform.position = exit.position;
            rb.linearVelocity = exit.forward * speed;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
