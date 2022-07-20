using UnityEngine;

public class Ammo : MonoBehaviour
{

    private int _bounceCountLeft = 2;

    public readonly int ammoDamage = 15;

    private readonly int _ammoDamage = 15;
    private readonly string _characterColliderTag = "Character";

    void OnCollisionEnter(Collision other) {

        GameObject contact = other.gameObject;

        if (_bounceCountLeft > 0)
        {
            if (contact.tag == _characterColliderTag)
            {
                Destroy(gameObject);
            }

            _bounceCountLeft--;
        } else Destroy(gameObject);

    }

    void OnDestroy() {
        // Create Explosion Effect
        Debug.Log("GG");
        // Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        // Vector3 pos = contact.point;
        // Instantiate(explosionPrefab, pos, rot);
        // Destroy(gameObject);
    }
}
