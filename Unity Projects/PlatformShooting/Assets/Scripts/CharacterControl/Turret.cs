using UnityEngine;

public class Turret : MonoBehaviour
{
    public GameObject player;
    public Rigidbody ammoPrefab;

    private const string _bulletTag = "Bullet";
    private const float _shootInterval = 0.5f;
    private const float _seekInterval = 1f;
    private const int _seekRange = 20;
    private const int _shootRange = 10;
    private const int _initHealth = 100;
    private const int _ammoSpeed = 20;

    private Rigidbody attacker = null;
    private Rigidbody _barrelRotationCenter;
    private HealthBar _healthBar;
    private GameObject _aimTarget = null;
    private bool _targetConfirm = false;
    private bool _keepShooting = false;
    private int _currentHealth;
    private int _barrelRotateSpeed = 45;


    void Awake()
    {
        _barrelRotationCenter = GetComponentsInChildren<Rigidbody>()[1];
    }

    void Start()
    {
        _currentHealth = _initHealth;
        _healthBar = GetComponentInChildren<HealthBar>();
        InvokeRepeating("SeekAttacker", 1f, _seekInterval);
        InvokeRepeating("RegenerateHealth", 1f, 0.5f);
    }

    void FixedUpdate()
    {
        if (!_targetConfirm) BarrelIdle();
        else if (_targetConfirm) BarrelAimTarget(_aimTarget);
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag(_bulletTag))
        {
            Rigidbody suspect = other.gameObject.GetComponentsInParent<Rigidbody>()[2];
            if (suspect.name != gameObject.name) attacker = suspect;

        }
    }

    private void SeekAttacker()
    {
        // Who should I attack now?
        if ((player == null) && (attacker == null))
        {
            _targetConfirm = false;
            _keepShooting = false;
            if (IsInvoking("BarrelShoot")) CancelInvoke("BarrelShoot");
            return;
        } else if (attacker == null) _aimTarget = player;
        else _aimTarget = attacker.gameObject;

        // Any obstacle between me and target?
        if (Physics.Linecast(_barrelRotationCenter.position, _aimTarget.transform.position, LayerMask.GetMask("Floor")))
        {
            _targetConfirm = false;
            _keepShooting = false;
            if (IsInvoking("BarrelShoot")) CancelInvoke("BarrelShoot");
            return;
        }

        // Are we close enough?
        float _targetDistanceSqr = (_aimTarget.transform.position - transform.position).sqrMagnitude;
        if (!_keepShooting && (_targetDistanceSqr < _shootRange * _shootRange))
        {
            _targetConfirm = true;
            _keepShooting = true;
        } else if (_targetDistanceSqr < _seekRange * _seekRange)
        {
            _targetConfirm = true;
            _keepShooting = false;
        } else if (_targetConfirm)
        {
            _targetConfirm = false;
            _keepShooting = false;
        }
    }

    private void BarrelAimTarget(GameObject target)
    {
        if (target)
        {
            _barrelRotationCenter.rotation =
                Quaternion.FromToRotation(Vector3.up, target.transform.position - _barrelRotationCenter.position);
        }

        if (_keepShooting && !IsInvoking("BarrelShoot"))
            InvokeRepeating("BarrelShoot", _shootInterval, _shootInterval);
        else if (!_keepShooting && IsInvoking("BarrelShoot")) CancelInvoke("BarrelShoot");
    }

    private void BarrelIdle()
    {
        if (Quaternion.Angle(Quaternion.identity, _barrelRotationCenter.rotation) > 105) _barrelRotateSpeed *= -1;
        Quaternion deltaRotation = Quaternion.Euler(0, 0, _barrelRotateSpeed * Time.fixedDeltaTime);
        _barrelRotationCenter.MoveRotation(_barrelRotationCenter.rotation * deltaRotation);
    }

    private void BarrelShoot()
    {
        Transform _barrelTransform = _barrelRotationCenter.transform;
        if (Vector3.Angle(Vector3.up, _barrelTransform.up) <= 105)
        {
            // TODO: create fog at barrel to hide distance between ammo

            Rigidbody newAmmo = Instantiate(ammoPrefab, 
                _barrelRotationCenter.position + _barrelTransform.up * 0.55f, _barrelRotationCenter.rotation, _barrelTransform);

            newAmmo.AddForce(_barrelTransform.up * _ammoSpeed, ForceMode.VelocityChange);
        }
    }

    private void ReceiveDamage(int damage)
    {
        _currentHealth -= damage;

        if (_currentHealth <= 0)
        {
            _healthBar.SetHealthValue(0);
            ZeroHealth();
        } else
        {
            _healthBar.SetHealthValue(_currentHealth / (float)_initHealth);
        }
    }

    private void ZeroHealth()
    {
        // TODO: Do turrets have to disappear?
        Destroy(gameObject);
    }

    private void RegenerateHealth()
    {
        if (_currentHealth < _initHealth)
        {
            _currentHealth++;
            _healthBar.SetHealthValue(_currentHealth / (float)_initHealth);
        }
    }
}