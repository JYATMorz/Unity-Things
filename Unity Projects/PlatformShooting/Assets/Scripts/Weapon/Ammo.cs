using UnityEngine;

public class Ammo : MonoBehaviour
{

    private int _bounceCountLeft = 2;
    private float _stuckTime = 0f;

    public readonly int ammoDamage = 15;
    public readonly int ammoSpeed = 20;
    public readonly float ammoInterval = 0.2f;

    private const string _characterTag = "Character";
    private const string _floorTag = "Floor";

    private readonly float _stuckLimit = 1f;

    void OnCollisionEnter(Collision other) {

        GameObject contact = other.gameObject;

        if (_bounceCountLeft > 0)
        {
            if (contact.tag == _characterTag)
            {
                Destroy(gameObject);
            }

            _bounceCountLeft--;
            _stuckTime = Time.time;
        } else Destroy(gameObject);
    }

    void OnCollisionStay(Collision other)
    {
        if ((Time.time - _stuckTime) >= _stuckLimit))
        {
            Destroy(gameObject);
        }

    }

    void OnCollisionExit(Collision other)
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
