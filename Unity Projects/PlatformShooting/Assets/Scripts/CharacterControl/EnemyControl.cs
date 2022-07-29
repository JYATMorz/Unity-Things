using UnityEngine;
using System;
using System.Collections;

public class EnemyControl : MonoBehaviour
{
    private const int _maxPlayerNum = 2;
    private const int _initHealth = 50;
    private const int _barrelRotateSpeed = 45;
    private const string _bulletTag = "Bullet";
    private const float _seekRange = 20f;
    private const float _seekInterval = 1f;
    private const float _shootRange = 10f;
    private const float _revengeLimit = 5f;
    private const float _speedScaler = 5f;

    private readonly string[] _floorTag = { "Floor", "Elevator" };
    private readonly string[] _ammoTypes = 
        { "CommonBullet", "LaserBeam", "GrenadeLauncher", "ExplosivePayload" };

    private GameObject _targetCharacter = null;
    private HealthBar _healthBar;
    private Rigidbody _characterBody;
    private Rigidbody _barrelShaft;
    private Rigidbody _suspectTarget;
    private Quaternion _deltaRotation;
    private bool _revengeMode = false;
    private int _currentHealth = _initHealth;
    private int _playerLayer;
    private int _floorLayer;
    private int _redTeamLayer;
    private int _blueTeamLayer;
    private int _neutralLayer;
    private int _deadLayer;
    private int _enemyLayer = -1;
    private string _ammoType = "CommonBullet";
    private float _rotateSpeed = _barrelRotateSpeed;

    [SerializeField]
    private bool _isNeutral = true;

    public float attackAspiration = 0.6f;
    // TODO: attach public material variable here.

    void Awake()
    {
        _characterBody = GetComponent<Rigidbody>();
        _barrelShaft = GetComponentsInChildren<Rigidbody>()[1];

        _playerLayer = LayerMask.GetMask("Player");
        _floorLayer = LayerMask.GetMask("Floor", "Elevator");
        _redTeamLayer = LayerMask.NameToLayer("RedTeam");
        _blueTeamLayer = LayerMask.NameToLayer("BlueTeam");
        _neutralLayer = LayerMask.NameToLayer("Neutral");
        _deadLayer = LayerMask.NameToLayer("Dead");

        _ammoType = _ammoTypes[UnityEngine.Random.Range(0, _ammoTypes.Length)];
    }

    void Start()
    {
        _healthBar = GetComponentInChildren<HealthBar>();

        if (_isNeutral) gameObject.layer = _neutralLayer;

        StartCoroutine(SeekPlayer());
        StartCoroutine(WanderAround());
    }

    void FixedUpdate()
    {
        // TODO: Control Movement Here

        // Control Barrel Here
        if (_targetCharacter == null) BarrelIdle();
        else BarrelAim();
    }

    void OnCollisionEnter(Collision other) 
    {
        GameObject contact = other.gameObject;

        if (contact.CompareTag(_bulletTag))
        {
            _suspectTarget = contact.GetComponentsInParent<Rigidbody>()[2];
            if (_suspectTarget.name != gameObject.name)
            {
                if (TargetInRange(_suspectTarget.transform.position, _seekRange))
                {
                    _targetCharacter = _suspectTarget.gameObject;
                    _revengeMode = true;
                    _enemyLayer = _targetCharacter.layer;
                    Invoke("ContinueRevenge", _revengeLimit);
                }
            }
        } else if (Array.Exists(_floorTag, tag => tag == contact.tag))
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
                    _targetCharacter = playerCollidersFound[UnityEngine.Random.Range(0, playerCount - 1)].gameObject;
                    // Can I see the target? 
                    if (ObstacleBetween(_targetCharacter.transform.position)) _targetCharacter = null;
                } else
                {
                    _targetCharacter = null;
                }
            }
            yield return new WaitForSeconds(_seekInterval);
        }
    }

    private IEnumerator SeekEnemy()
    {
        while(true)
        {
            if(!_isNeutral)
            {
                Collider[] targetsInRange = Physics.OverlapSphere(transform.position, _seekRange, _enemyLayer);
                foreach (Collider target in targetsInRange)
                {
                    if ((UnityEngine.Random.value < attackAspiration) && ObstacleBetween(target.transform.position))
                    {
                        _targetCharacter = target.gameObject;
                        break;
                    }
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
        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, (_targetCharacter.transform.position - _barrelShaft.position).normalized);
        _barrelShaft.rotation = Quaternion.RotateTowards(_barrelShaft.rotation, targetRotation, 3 * _barrelRotateSpeed * Time.fixedDeltaTime);

        if (AimAtTarget())
        {
            if (!ObstacleBetween(_targetCharacter.transform.position)) gameObject.SendMessage("BarrelShoot", _ammoType);
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
        return Physics.Raycast(_barrelShaft.position, _barrelShaft.transform.up, _shootRange, _targetCharacter.layer);
    }

    private void ContinueRevenge()
    {
        // TODO: edit to accept enemy layer
        if ((_targetCharacter != null) && (ObstacleBetween(_targetCharacter.transform.position) || !TargetInRange(_targetCharacter.transform.position, _seekRange)))
        {
            _revengeMode = false;
            _targetCharacter = null;
        }
        if (_targetCharacter == null) _revengeMode = false;
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
        SwitchTeamStatus();
        if (!_isNeutral) 
        {
            Debug.Log("A character is dead.");
        } else _currentHealth = _initHealth;
    }

   private void SwitchTeamStatus()
    {
        if (!_isNeutral)
        {
            gameObject.layer = _deadLayer;
            gameObject.tag = "Dead";
            // TODO: Change material here.

            // TODO: enable character to rotate freely, need to check z value after death
            _characterBody.detectCollisions = false;
            _characterBody.freezeRotation = false;
            return;
        }

        _isNeutral = false;
        if (_targetCharacter!=null)
        {
            if (_targetCharacter.layer == _blueTeamLayer)
            {
                gameObject.layer = _redTeamLayer;
                gameObject.tag = "RedTeam";
                // TODO: Change material here.
            } else if (_targetCharacter.layer == _redTeamLayer)
            {
                gameObject.layer = _blueTeamLayer;
                gameObject.tag = "BlueTeam";
                // TODO: Change material here.
            }
        } else Debug.Log("Target is null when changing team!");
    }

    private IEnumerator WanderAround()
    {
        // TODO: Temperate solution for movement
        while (true)
        {
            if ((_targetCharacter == null) && !_revengeMode)
            {
                _characterBody.AddForce(new Vector3((UnityEngine.Random.value - 0.5f) * _speedScaler, 0, 0), ForceMode.Impulse);
            } else _characterBody.velocity = Vector3.zero;

            yield return new WaitForSeconds(_seekInterval);
        }
    }
}

