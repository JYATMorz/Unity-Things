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

    void OnTriggerExit() {
        // liftEffect.Stop();
    }

    void OnTriggerStay(Collider other)
    {
        other.attachedRigidbody.AddExplosionForce(-_liftForce * 0.25f, transform.position, _liftDistance, _liftForce);
    }
}