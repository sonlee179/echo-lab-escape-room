using UnityEngine;

public class KeyPiece : MonoBehaviour, IInteractable
{
    void Update()
    {
        transform.Rotate(0f, 80f * Time.deltaTime, 0f, Space.World);
    }

    public void Interact()
    {
        Sfx.Play("pickup");
        GameManager.Instance.AddKeyPiece();
        Destroy(gameObject);
    }

    public void StopInteract() { }
}
