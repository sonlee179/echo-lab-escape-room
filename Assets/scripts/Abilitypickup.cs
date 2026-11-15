using UnityEngine;

public enum Ability
{
    MassTuner,
    TimeEcho,
    GravityFlip
}

public class AbilityPickup : MonoBehaviour, IInteractable
{
    public Ability ability;

    [TextArea(2, 5)]
    public string message;

    void Update()
    {
        transform.Rotate(0f, 60f * Time.deltaTime, 0f, Space.World);
    }

    public void Interact()
    {
        Sfx.Play("pickup");
        GameObject p = GameObject.FindWithTag("Player");

        if (p != null)
        {
            switch (ability)
            {
                case Ability.MassTuner:
                    Progress.mass = true;
                    MassTuner m = p.GetComponent<MassTuner>();
                    if (m != null) m.enabled = true;
                    break;

                case Ability.TimeEcho:
                    Progress.echo = true;
                    EchoRecorder e = p.GetComponent<EchoRecorder>();
                    if (e != null) e.enabled = true;
                    break;

                case Ability.GravityFlip:
                    Progress.gravity = true;
                    GravityFlip g = p.GetComponent<GravityFlip>();
                    if (g != null) g.enabled = true;
                    break;
            }
        }

        if (GameManager.Instance != null)
            GameManager.Instance.ShowMessage(message, 5f);

        Destroy(gameObject);
    }

    public void StopInteract() { }
}
