using UnityEngine;

public class Ammo : MonoBehaviour
{

    private int _bounceCountLeft = 2;

    public readonly int ammoDamage = 15;
    public readonly int ammoSpeed = 20;
    public readonly float ammoInterval = 0.2f;

    private const string _characterTag = "Character";
    private const string _floorTag = "Floor";

    void OnCollisionEnter(Collision other) {

        GameObject contact = other.gameObject;

        if (_bounceCountLeft > 0)
        {
            if (contact.tag == _characterTag)
            {
                Destroy(gameObject);
            }

            _bounceCountLeft--;
        } else Destroy(gameObject);
    }

    void OnCollisionStay(Collision other)
    {
        if (other.gameObject.tag == _floorTag)
        {
            Invoke("StuckInWall", 1f);
        }

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
