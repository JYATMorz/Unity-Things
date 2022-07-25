using UnityEngine;

public class TemplateBullet : MonoBehaviour
{
    public GameObject bulletImpact;

    private const int _ammoDamage = 15;
    private const string _characterTag = "Character";
    private const string _floorTag = "Floor";

    void OnCollisionEnter(Collision other)
    {
        GameObject contact = other.gameObject;
        if (contact.CompareTag(_characterTag))
        {
            contact.SendMessage("ReceiveDamage", _ammoDamage);
        }
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // TODO: Add more detailed smoke (particle) effect when collides
        Instantiate(bulletImpact, transform.position, transform.rotation * Quaternion.FromToRotation(Vector3.forward, Vector3.down));
    }
}
