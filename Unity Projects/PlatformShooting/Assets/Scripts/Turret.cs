using UnityEngine;

public class Turret : MonoBehaviour
{
    public GameObject player;

    private Rigidbody attacker = null;

    private readonly string _bulletTag = "Bullet";
    private readonly float _seekInterval = 1f;

    private int _seekDistance;
    private bool _seekConfirm = false;
    private int _shootRange;

    void FixedUpdate()
    {
        if (!_seekConfirm)
        {
            Invoke("SeekAttacker", _seekInterval);
            BarrelIdle();
        }
    }

    void OnCollisionEnter(Collision other)
    {
        attacker = other.gameObject.GetComponentInParent<Rigidbody>();
    }

    private void SeekAttacker()
    {
        GameObject target;
        if (attacker == null) target = player;
        else target = attacker.gameObject;

        if (Vector3.Distance(target.transform.position, transform.position)) // Do you find attacker in seek distance?
        {
            _seekConfirm = true;
            BarrelAimTarget(target);
        }
    }

    private void BarrelAimTarget(GameObject target)
    {

    }

    private void BarrelIdle()
    {
        
    }
}