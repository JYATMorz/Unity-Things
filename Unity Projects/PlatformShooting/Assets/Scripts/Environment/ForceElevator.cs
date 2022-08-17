using UnityEngine;
using UnityEngine.VFX;

public class ForceElevator : MonoBehaviour
{
    private readonly float _liftForce = 30f;

    public VisualEffect liftEffect;

    void OnTriggerEnter()
    {
        // TODO: liftEffect.Play();
    }

    void OnTriggerStay(Collider other)
    {
        other.attachedRigidbody.AddForce(transform.up * _liftForce, ForceMode.Impulse);
    }
}