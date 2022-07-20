using UnityEngine;

public class Ammo : MonoBehaviour
{

    private int _bounceCountLeft = 2;
    private readonly string _characterColliderTag = "Character";

    void OnCollisionEnter(Collision other) {

        ContactPoint contact = other.GetContact(0);

        if (_bounceCountLeft > 0)
        {
            if (contact.otherCollider.tag == _characterColliderTag)
            {
                // tell the character to damage yourself
                Destroy(gameObject);
            }

            _bounceCountLeft--;
        }
        else Destroy(gameObject);

    }

    void OnDestroy() {
        Debug.Log("GG");
        // Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        // Vector3 pos = contact.point;
        // Instantiate(explosionPrefab, pos, rot);
        // Destroy(gameObject);
    }
}
