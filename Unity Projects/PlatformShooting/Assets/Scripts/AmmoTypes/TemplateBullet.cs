using UnityEngine;

public class TemplateBullet : MonoBehaviour
{

    private int _bounceCountLeft = 4;
    private float _stuckTime = 0f;

    public readonly int ammoDamage = 15;

    private const string _characterTag = "Character";
    private const string _floorTag = "Floor";
    private const float _stuckLimit = 1f;

    void OnCollisionEnter(Collision other)
    {
        // TODO: Add smoke (particle) effect when collides

        GameObject contact = other.gameObject;

        if (_bounceCountLeft > 0)
        {
            _bounceCountLeft--;
            if (contact.CompareTag(_characterTag))
            {
                WaitToDestroy();
                contact.SendMessage("ReceiveDamage", ammoDamage);
            }

            _stuckTime = Time.time;

        } else WaitToDestroy();
    }

    void OnCollisionStay()
    {
        if ((Time.time - _stuckTime) >= _stuckLimit)
        {
            Destroy(gameObject);
        }

    }

    void OnCollisionExit()
    {
        _stuckTime = 0f;
    }

    private void WaitToDestroy()
    {
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, 0.25f);
    }
}
