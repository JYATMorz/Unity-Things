using UnityEngine;

public class TemplateBullet : MonoBehaviour
{
    public GameObject bulletImpact;

    public readonly int ammoDamage = 15;

    private const string _characterTag = "Character";
    private const string _floorTag = "Floor";
    private const float _stuckLimit = 1f;

    void OnCollisionEnter(Collision other)
    {
        GameObject contact = other.gameObject;
        if (contact.CompareTag(_characterTag))
        {
            contact.SendMessage("ReceiveDamage", ammoDamage);
        }
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // TODO: Add more detailed smoke (particle) effect when collides
        Instantiate(bulletImpact, transform.position, transform.rotation * Quaternion.FromToRotation(Vector3.forward, Vector3.down));
    }
}
