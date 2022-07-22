using UnityEngine;

public class CommonBullet : MonoBehaviour
{

    private int _bounceCountLeft = 2;
    private float _stuckTime = 0f;

    public readonly int ammoDamage = 15;

    private const string _characterTag = "Character";
    private const string _floorTag = "Floor";

    private readonly float _stuckLimit = 1f;

    void OnCollisionEnter(Collision other) {

        GameObject contact = other.gameObject;

        if (_bounceCountLeft > 0)
        {
            _bounceCountLeft--;
            if (contact.CompareTag(_characterTag))
            {
                Destroy(gameObject);
                contact.SendMessage("ReceiveDamage", ammoDamage);
            }

            _stuckTime = Time.time;

        } else Destroy(gameObject);
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

    void OnDestroy() {
        // Create Explosion Effect

        // Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        // Vector3 pos = contact.point;
        // Instantiate(explosionPrefab, pos, rot);
        // Destroy(gameObject);
    }

    private void StuckInWall()
    {
        Destroy(gameObject);
    }
}
