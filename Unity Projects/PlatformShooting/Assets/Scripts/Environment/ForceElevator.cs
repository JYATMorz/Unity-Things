using UnityEngine;
using UnityEngine.VFX;

public class ForceElevator : MonoBehaviour
{
    private readonly float _liftForce = 40f;

    public VisualEffect liftEffect;

    void OnTriggerEnter()
    {
        // TODO: liftEffect.Play();
        GeneralAudioControl.Instance.PlayAudio(ConstantSettings.jumpTag, transform.position);
    }

    void OnTriggerStay(Collider other)
    {
        other.attachedRigidbody.AddForce(transform.up * _liftForce, ForceMode.Impulse);
    }
}