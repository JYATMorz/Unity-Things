using UnityEngine;
using UnityEngine.VFX;

public class ForceElevator : MonoBehaviour
{
    public float _liftForce = 30f;
    public ForceMode forceType = ForceMode.Impulse;

    public VisualEffect liftEffect;

    void OnTriggerEnter() {
        // liftEffect.Play();
    }

    void OnTriggerExit() {
        // liftEffect.Stop();
    }

    void OnTriggerStay(Collider other)
    {
        other.attachedRigidbody.AddForce(transform.up * _liftForce, forceType);
    }
}