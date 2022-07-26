using UnityEngine;
using System.Collections;

public class EnemyControl : MonoBehaviour
{
    private const int _maxPlayerNum = 2;
    private const int _initHealth = 50;
    private const int _barrelRotateSpeed = 45;
    private const string _bulletTag = "Bullet";
    private const string _floorTag = "Floor";
    private const float _seekRange = 20f;
    private const float _seekInterval = 1f;
    private const float _shootRange = 10f;
    private const float _revengeLimit = 5f;
    private const float _speedScaler = 5f;


    public GameObject Target { get; private set; } = null;
    public int CurrentHealth { get; private set; }
    
    private HealthBar _healthBar;
    private Rigidbody _characterBody;
    private Rigidbody _barrelShaft;
    private Rigidbody _suspectTarget;
    private Quaternion _deltaRotation;
    private bool _revengeMode = false;
    private int _playerLayer;
    private int _floorLayer;
    private string _ammoType = "CommonBullet"; // TODO: Randomly select the weapon
    private float _rotateSpeed = _barrelRotateSpeed;

    void Awake()
    {
        _characterBody = GetComponent<Rigidbody>();
        _barrelShaft = GetComponentsInChildren<Rigidbody>()[1];
    }

    void Start()
    {
        CurrentHealth = _initHealth;
        _healthBar = GetComponentInChildren<HealthBar>();
        _playerLayer = LayerMask.GetMask("Player");
        _floorLayer = LayerMask.GetMask("Floor");

        StartCoroutine(SeekPlayer());
        StartCoroutine(WanderAround());
    }

    void FixedUpdate()
    {
        // TODO: Control Movement Here

        // Control Barrel Here
        if (Target == null) BarrelIdle();
        else BarrelAim();
    }

    void OnCollisionEnter(Collision other) 
    {
        GameObject collideObj = other.gameObject;

        if (collideObj.CompareTag(_bulletTag))
        {
            _suspectTarget = collideObj.GetComponentsInParent<Rigidbody>()[2];
            if (_suspectTarget.name != gameObject.name)
            {
                if (TargetInRange(_suspectTarget.transform.position, _seekRange))
                {
                    Target = _suspectTarget.gameObject;
                    _revengeMode = true;
                    Invoke("ContinueRevenge", _revengeLimit);
                    // TODO: Show the difference when in revenge mode
                }
            }
        } else if (collideObj.CompareTag(_floorTag))
        {
            // Touch down damage
            float speedSquare = other.relativeVelocity.sqrMagnitude;
            if (speedSquare > 100) ReceiveDamage(Mathf.CeilToInt(speedSquare / 10f));
        }
    }

    private IEnumerator SeekPlayer()
    {
        while (true)
        {
            if (!_revengeMode)
            {
                Collider[] playerCollidersFound = new Collider[_maxPlayerNum];
                int playerCount = Physics.OverlapSphereNonAlloc(transform.position, _seekRange, playerCollidersFound, _playerLayer);
                
                if (playerCount > 0)
                {
                    Target = playerCollidersFound[Random.Range(0, playerCount - 1)].gameObject;
                    // Can I see the target? 
                    if (ObstacleBetween(Target.transform.position)) Target = null;
                } else
                {
                    Target = null;
                }
            }
            yield return new WaitForSeconds(_seekInterval);
        }
    }

    private void BarrelIdle()
    {
        gameObject.SendMessage("StopShoot");

        if (Quaternion.Angle(Quaternion.identity, _barrelShaft.rotation) > 120) _rotateSpeed *= -1;

        _deltaRotation = Quaternion.Euler(0, 0, _rotateSpeed * Time.fixedDeltaTime);
        _barrelShaft.MoveRotation(_barrelShaft.rotation * _deltaRotation);
    }

    private void BarrelAim()
    {
        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, (Target.transform.position - _barrelShaft.position).normalized);
        _barrelShaft.rotation = Quaternion.RotateTowards(_barrelShaft.rotation, targetRotation, 3 * _barrelRotateSpeed * Time.fixedDeltaTime);

        if (AimAtTarget())
        {
            if (!ObstacleBetween(Target.transform.position)) gameObject.SendMessage("BarrelShoot", _ammoType);
            else gameObject.SendMessage("StopShoot");
        } else gameObject.SendMessage("StopShoot");
    }

    private bool TargetInRange(Vector3 targetPosition, float range)
    {
        if ((targetPosition - transform.position).sqrMagnitude < range * range) return true;
        else return false;
    }

    private bool ObstacleBetween(Vector3 targetPosition)
    {
        return Physics.Linecast(_barrelShaft.position, targetPosition, _floorLayer);
    }

    private bool AimAtTarget()
    {
        return Physics.Raycast(_barrelShaft.position, _barrelShaft.transform.up, _shootRange, _playerLayer);
    }

    private void ContinueRevenge()
    {
        if ((Target != null) && (ObstacleBetween(Target.transform.position) || !TargetInRange(Target.transform.position, _seekRange)))
        {
            _revengeMode = false;
            Target = null;
        }
        if (Target == null) _revengeMode = false;
    }

    private void ReceiveDamage(int damage)
    {
        CurrentHealth -= damage;

        if (CurrentHealth <= 0)
        {
            _healthBar.SetHealthValue(0);
            ZeroHealth();
        } else
        {
            _healthBar.SetHealthValue(CurrentHealth / (float)_initHealth);
        }
    }

    private void ZeroHealth()
    {
        Destroy(gameObject);
        Debug.Log("An enemy is dead.");
    }

    private IEnumerator WanderAround()
    {
        // TODO: Temperate solution for movement
        while (true)
        {
            if ((Target == null) && !_revengeMode)
            {
                _characterBody.AddForce(new Vector3((Random.value - 0.5f) * _speedScaler, 0, 0), ForceMode.Impulse);
            } else _characterBody.velocity = Vector3.zero;

            yield return new WaitForSeconds(_seekInterval);
        }
    }
}

