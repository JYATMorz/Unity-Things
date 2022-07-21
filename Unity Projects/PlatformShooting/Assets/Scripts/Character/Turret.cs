using UnityEngine;

public class Turret : MonoBehaviour
{
    public GameObject player;
    public Rigidbody ammoPrefab;

    private readonly string _bulletTag = "Bullet";
    private readonly float _shootInterval = 0.5f;
    private readonly float _seekInterval = 1f;
    private readonly int _seekRange = 20;
    private readonly int _shootRange = 10;
    private readonly int _initHealth = 100;

    private Rigidbody attacker = null;
    private Rigidbody _barrelRotationCenter;
    private HealthBar _healthBar;
    private bool _targetConfirm = false;
    private bool _keepShooting = false;
    private bool _isShot = false;
    private int _currentHealth;

    void Awake()
    {
        _barrelRotationCenter = GetComponentsInChildren<Rigidbody>()[1];
    }

    void Start()
    {
        _currentHealth = _initHealth;
        _healthBar = GetComponentInChildren<HealthBar>();
        InvokeRepeating("SeekAttacker", 1f, _seekInterval);
        InvokeRepeating("RegenerateHealth", 1f, 1f);
    }

    void Update()
    {
        // if (_targetConfirm && IsInvoking("SeekAttacker")) CancelInvoke("SeekAttacker");
        // if (!_targetConfirm && !IsInvoking("SeekAttacker")) InvokeRepeating("SeekAttacker", 0.5f, _seekInterval);
    }

    void FixedUpdate()
    {
        if (!_targetConfirm && !_keepShooting) BarrelIdle();
        else if (_targetConfirm) BarrelAimTarget(target);
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.CompareTag(_bulletTag))
        {
            attacker = other.GetComponentInParent<Rigidbody>();
            Debug.Log(attacker.name);

            Ammo ammoScript = other.gameObject.GetComponentInChildren<Ammo>();
            ReceiveDamage(ammoScript.ammoDamage);
        }
    }

    private void SeekAttacker()
    {
        GameObject target;
        if ((player == null) && (attacker == null))
        {
            _targetConfirm = false;
            _keepShooting = false;
            return;
        } else if (attacker == null) target = player;
        else target = attacker.gameObject;

        float _targetDistanceSqr = (target.transform.position - transform.position).sqrMagnitude;
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
        _barrelRotationCenter.rotation =
            Quaternion.FromToRotation(Vector3.up, target.transform.position - _barrelRotationCenter.position);

        if (_keepShooting && !IsInvoking("BarrelShoot"))
            InvokeRepeating("BarrelShoot", _shootInterval, _shootInterval);
        else if (!_keepShooting && IsInvoking("BarrelShoot")) CancelInvoke("BarrelShoot");
    }

    private void BarrelIdle()
    {
        Quaternion deltaRotation = Quaternion.Euler(0, 0, 30 * Time.fixedDeltaTime);
        _barrelRotationCenter.MoveRotation(_barrelRotationCenter.rotation * deltaRotation);
    }

    private void BarrelShoot()
    {
        Transform _barrelTransform = _barrelRotationCenter.transform;
        if (Vector3.Angle(Vector3.up, _barrelTransform.up) <= 105)
        {
            // create fog at barrel to hide distance between ammo

            Rigidbody newAmmo = Instantiate(ammoPrefab, _barrelRotationCenter.position + _barrelTransform.up * 0.55f, _barrelRotationCenter.rotation, _barrelTransform);
            Ammo ammoScript = newAmmo.GetComponent<Ammo>();

            newAmmo.AddForce(_barrelTransform.up * ammoScript.ammoSpeed, ForceMode.VelocityChange);
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
        // Maybe player can revive with special effect
        Destroy(gameObject);
        Debug.Log("You are dead.");
    }

    private void RegenerateHealth()
    {
        if (_currentHealth < _initHealth) _currentHealth++;
    }
}