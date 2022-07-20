using UnityEngine;

public class Turret : MonoBehaviour
{
    public GameObject player;

    private readonly string _bulletTag = "Bullet";
    private readonly float _seekInterval = 1f;
    private readonly int _seekDistance = 20;
    private readonly int _shootRange = 10;

    private Rigidbody attacker = null;
    private bool _seekConfirm = false;

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

        if (Vector3.Distance(target.transform.position, transform.position) < _seekDistance) // Do you find attacker in seek distance?
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