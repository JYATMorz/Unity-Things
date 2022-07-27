using UnityEngine;
using UnityEngine.VFX;

public class ForceElevator : MonoBehaviour
{
    // TODO: Change it to lift character quick
    private const float _liftForce = 10f;
    private const float _liftDistance = 15f;

    public VisualEffect liftEffect;

    void OnTriggerEnter() {
        // liftEffect.Play();
    }

    void OnTriggerExit(Collider other) {
        // liftEffect.Stop();
    }

    void OnTriggerStay(Collider other)
    {
        other.attachedRigidbody.AddExplosionForce(_liftForce * 0.5f, transform.position, _liftDistance, _liftForce);
    }
}