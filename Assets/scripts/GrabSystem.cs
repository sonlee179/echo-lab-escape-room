using UnityEngine;

public interface IInteractable
{
    void Interact();
    void StopInteract();
}

public class GrabSystem : MonoBehaviour
{
    public static GrabSystem Instance;
    public static bool IsRotating;

    public Camera cam;
    public Transform holdPoint;
    public float reach = 3f;
    public float followSpeed = 12f;
    public float throwForce = 15f;
    public float rotateSpeed = 5f;
    public float maxGrabMass = 10f;

    Rigidbody held;
    IInteractable active;

    void Awake() { Instance = this; }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (held != null) Drop();
            else TryInteract();
        }

        if (Input.GetKeyUp(KeyCode.E) && active != null)
        {
            active.StopInteract();
            active = null;
        }

        IsRotating = held != null && Input.GetKey(KeyCode.R);
        if (held == null) return;

        if (Input.GetMouseButtonDown(0)) { Throw(); return; }

        if (IsRotating)
        {
            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");
            held.transform.Rotate(cam.transform.up, -mx * rotateSpeed, Space.World);
            held.transform.Rotate(cam.transform.right, my * rotateSpeed, Space.World);
        }
    }

    void FixedUpdate()
    {
        if (held == null) return;
        Vector3 d = holdPoint.position - held.position;
        held.linearVelocity = Vector3.ClampMagnitude(d * followSpeed, 15f);
        if (d.magnitude > 1.5f) Drop();   // bị kẹt thì tự thả
    }

    void TryInteract()
    {
        if (!Physics.Raycast(cam.transform.position, cam.transform.forward,
                out RaycastHit hit, reach, ~0, QueryTriggerInteraction.Ignore)) return;

        var inter = hit.collider.GetComponentInParent<IInteractable>();
        if (inter != null) { inter.Interact(); active = inter; return; }

        Rigidbody rb = hit.rigidbody;
        if (rb != null && !rb.isKinematic && rb.mass <= maxGrabMass) Grab(rb);
    }

    void Grab(Rigidbody rb)
    {
        held = rb;
        held.useGravity = false;
        held.freezeRotation = true;
        Sfx.Play("grab", 0.7f);
    }

    void Drop()
    {
        if (held == null) return;
        held.useGravity = true;
        held.freezeRotation = false;
        held = null;
        Sfx.Play("drop", 0.6f);
    }

    void Throw()
    {
        Rigidbody rb = held;
        Drop();
        rb.AddForce(cam.transform.forward * throwForce, ForceMode.Impulse);
        Sfx.Play("throw", 0.8f);
    }

    public void ForceDrop(Rigidbody rb)
    {
        if (held == rb) Drop();
    }
}
