using UnityEngine;
using System;
using System.Collections;

public class CharacterControl : MonoBehaviour
{
    private const int _initHealth = 100;
    private const int _barrelRotateSpeed = 45;
    private const float _seekInterval = 1f;
    private const float _seekRange = 30f;
    private const float _shootRange = 10f;
    private const float _speedScaler = 5f;
    private const float _jumpScaler = 25f;
    private const string _bulletTag = "Bullet";
    private const string _neutralTag = "Neutral";
    private const string _blueTeamTag = "BlueTeam";
    private const string _redTeamTag = "RedTeam";

    private readonly Vector3 _nullTargetPosition = new(0, 100, 0);
    private readonly string[] _floorTag = { "Floor", "Elevator" };
    private readonly string[] _ammoTypes = 
        { "CommonBullet", "LaserBeam", "GrenadeLauncher", "ExplosivePayload" };

    private GameObject _targetCharacter = null;
    private Vector3 _targetPosition;
    private HealthBar _healthBar;
    private Rigidbody _characterBody;
    private Rigidbody _barrelShaft;
    private Quaternion _deltaRotation;
    private bool _chaseMode = false;
    private bool _jumpPressed = false;
    private bool _isGrounded = true;
    private int _doubleJump = 2;
    private int _currentHealth = _initHealth;
    private int _floorLayer;
    private int _avoidLayer;
    private int _neutralLayer;
    private int _deadLayer;
    private int _enemyLayer = -1;
    private string _ammoType = "CommonBullet";
    private float _rotateSpeed = _barrelRotateSpeed;

    [SerializeField]
    private bool _isNeutral = true;
    [SerializeField]
    private bool _isPlayer = false;

    public Camera mainCamera;
    public float attackWillingness = 0.6f;
    // TODO: attach public material variable here.

    void Awake()
    {
        _characterBody = GetComponent<Rigidbody>();
        _barrelShaft = GetComponentsInChildren<Rigidbody>()[1];

        _floorLayer = LayerMask.GetMask("Floor", "Elevator");
        _avoidLayer = _floorLayer;
        _neutralLayer = LayerMask.NameToLayer("Neutral");
        _deadLayer = LayerMask.NameToLayer("Dead");

        _ammoType = _ammoTypes[UnityEngine.Random.Range(0, _ammoTypes.Length)];

        if (_isNeutral && _isPlayer) Debug.LogWarning("Character Setting is Wrong !");
    }

    void Start()
    {
        _healthBar = GetComponentInChildren<HealthBar>();

        if (_isNeutral) gameObject.layer = _neutralLayer;
        else
        {
            if (CompareTag(_redTeamTag))
            {
                SwitchLayer(_redTeamTag, _blueTeamTag);
                // TODO: Change material here.
            } else if (CompareTag(_blueTeamTag))
            {
                SwitchLayer(_blueTeamTag, _redTeamTag);
                // TODO: Change material here.
            } else Debug.Log("???");
        }

        StartCoroutine(SeekEnemy());
        StartCoroutine(WanderAround());
    }

    void Update()
    {
        if (_isPlayer && Input.GetKeyDown(KeyCode.Space) && (_doubleJump > 0)) 
            _jumpPressed = true;
    }

    void FixedUpdate()
    {
        if (_isPlayer)
        {
            if (_jumpPressed)
            {
                _characterBody.AddForce(Vector3.up * _jumpScaler, ForceMode.Impulse);

                _doubleJump --;
                _jumpPressed = false;
            }

            _characterBody.velocity = new Vector3(Input.GetAxis("Horizontal") * _speedScaler, _characterBody.velocity.y, 0);

            // Rotate the barrel to point at the mouse position
            Vector3 _rotateVector = Input.mousePosition - mainCamera.WorldToScreenPoint(_barrelShaft.position);
            _rotateVector.z = 0;
            _barrelShaft.rotation *= Quaternion.FromToRotation(_barrelShaft.transform.up, _rotateVector);
        }


        if (_targetCharacter != null && !ObstacleBetween(_targetCharacter.transform.position))
            _targetPosition = _targetCharacter.transform.position;

        // Control Barrel Here
        if (_targetCharacter == null) BarrelIdle();
        else
        {
            BarrelAim();
            if (_chaseMode) ChaseTarget();
        }
    }

    void OnCollisionEnter(Collision other) 
    {
        GameObject contact = other.gameObject;

        if (contact.CompareTag(_bulletTag))
        {
            SwitchTarget(contact.GetComponentsInParent<Rigidbody>()[2]);

        } else if (Array.Exists(_floorTag, tag => tag == contact.tag))
        {
            if (_isPlayer) _doubleJump = 2;
            // Touch down damage
            float speedSquare = other.relativeVelocity.sqrMagnitude;
            if (speedSquare > 100) ReceiveDamage(Mathf.CeilToInt(speedSquare / 10f));
        }
    }

    private IEnumerator SeekEnemy()
    {
        while(true)
        {
            if(!_isNeutral)
            {
                if (_targetCharacter == null) SearchTarget();
                else
                {
                    if (!TargetInRange(_targetPosition, _seekRange)) ResetTargetCharacter();
                    else if (ObstacleBetween(_targetPosition)) _chaseMode = true;
                }
            }
            yield return new WaitForSeconds(_seekInterval);
        }
    }

    private void SearchTarget()
    {
        foreach (Collider target in Physics.OverlapSphere(transform.position, _seekRange, _enemyLayer))
        {
            if ((UnityEngine.Random.value < attackWillingness) && !ObstacleBetween(target.transform.position))
            {
                _targetCharacter = target.gameObject;
                break;
            }
        }
    }

    private void SwitchTarget(Rigidbody suspect)
    {
        Vector3 suspectPosition = suspect.position;

        if (_isNeutral)
        {
            if (_targetCharacter == null)
            {
                _targetCharacter = suspect.gameObject;
            }
            else if (_targetCharacter.CompareTag(_neutralTag))
            {
                if (suspect.CompareTag(_neutralTag))
                    _targetCharacter = (UnityEngine.Random.value < 0.2f) ? suspect.gameObject : _targetCharacter;
                else 
                    _targetCharacter = suspect.gameObject;
            } else
            {
                if (!suspect.CompareTag(_neutralTag))
                {
                    if (!TargetInRange(_targetPosition, _seekRange))
                    {
                        _targetCharacter = suspect.gameObject;
                    } else if (!TargetInRange(_targetPosition, _shootRange))
                    {
                        if (ObstacleBetween(_targetPosition)) _targetCharacter = suspect.gameObject;
                        else _targetCharacter = (UnityEngine.Random.value < 0.3f) ? suspect.gameObject : _targetCharacter;
                    }
                }
            }
        } else
        {
            if (_targetCharacter == null) _targetCharacter = CompareTag(suspect.tag) ? null : suspect.gameObject;
            else
            {
                if (!suspect.CompareTag(_neutralTag))
                {
                    if (_targetCharacter.CompareTag(_neutralTag)) _targetCharacter = suspect.gameObject;
                    else 
                        _targetCharacter = 
                            ((suspectPosition - transform.position).sqrMagnitude < 4 * (_targetPosition - transform.position).sqrMagnitude)
                            ? suspect.gameObject : _targetCharacter;
                }
            }
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
        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, (_targetPosition - _barrelShaft.position).normalized);
        _barrelShaft.rotation = Quaternion.RotateTowards(_barrelShaft.rotation, targetRotation, 3 * _barrelRotateSpeed * Time.fixedDeltaTime);

        if (!_chaseMode && AimAtTarget())
        {
            if (!ObstacleBetween(_targetPosition, _avoidLayer)) gameObject.SendMessage("BarrelShoot", _ammoType);
            else gameObject.SendMessage("StopShoot");
        } else gameObject.SendMessage("StopShoot");
    }

    private void ChaseTarget()
    {
        if (_targetPosition == null || _targetPosition == _nullTargetPosition) Debug.Log("Why there is no target position?");
        else
        {
            float xDirection = Mathf.Sign((_targetPosition - _characterBody.position).x);
            if (Mathf.Abs((_targetPosition - _characterBody.position).x) > 1)
            {
                // TODO: Apply NavMesh AI
                _characterBody.MovePosition(_characterBody.position + new Vector3(xDirection * _speedScaler, 0, 0));
            } else
            {
                _chaseMode = false;
                if (ObstacleBetween(_targetCharacter.transform.position))
                    ResetTargetCharacter();
            }
        }
    }

    private bool TargetInRange(Vector3 targetPosition, float range)
    {
        if ((targetPosition - transform.position).sqrMagnitude < range * range) return true;
        else return false;
    }

    private bool ObstacleBetween(Vector3 targetPosition, int layer = -1)
    {
        if (layer == -1) layer = _floorLayer;
        return Physics.Linecast(_barrelShaft.position, targetPosition, layer);
    }

    private bool AimAtTarget()
    {
        return Physics.Raycast(_barrelShaft.position, _barrelShaft.transform.up, _shootRange, _targetCharacter.layer);
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
        if (!_isNeutral)
        {
            gameObject.layer = _deadLayer;
            gameObject.tag = "Dead";
            // TODO: Change material here.

            // BUGCheck: enable character to rotate freely, need to check z value after death
            _characterBody.detectCollisions = false;
            _characterBody.freezeRotation = false;

            if (_isPlayer)
            {
                _isPlayer = false;
                Debug.Log("Player is dead.");
            } else Debug.Log("A character is dead.");

            return;
        }

        if (_targetCharacter!=null)
        {
            _isNeutral = false;
            _currentHealth = _initHealth;
            _healthBar.SetHealthValue(1f);

            if (_targetCharacter.CompareTag(_blueTeamTag))
            {
                gameObject.tag = _redTeamTag;
                SwitchLayer(_redTeamTag, _blueTeamTag);
                // TODO: Change material here.
            } else if (_targetCharacter.CompareTag(_redTeamTag))
            {
                gameObject.tag = _blueTeamTag;
                SwitchLayer(_blueTeamTag, _redTeamTag);
                // TODO: Change material here.
            }
        } else Debug.Log("Target is null when changing team!");
    }

    private IEnumerator WanderAround()
    {
        while (true)
        {
            if ((_targetCharacter == null) && !_chaseMode)
            {
                _characterBody.velocity = new Vector3(Mathf.PingPong(Time.time, _speedScaler * 2) - _speedScaler, 0, 0);
            } else _characterBody.velocity = Vector3.zero;

            yield return new WaitForFixedUpdate();
        }
    }

    private void ResetTargetCharacter()
    {
        _targetCharacter = null;
        _targetPosition = _nullTargetPosition;
    }

    private void SwitchLayer(string teamTag, string enemyTag)
    {
        gameObject.layer = LayerMask.NameToLayer(teamTag);
        _avoidLayer = LayerMask.GetMask(teamTag, "Floor", "Elevator");
        _enemyLayer = LayerMask.GetMask(enemyTag, "Neutral");
    }

    private void BecomePlayer()
    {
        _isPlayer = true;
        if (_isNeutral && _isPlayer) Debug.LogWarning("Neutral Character becomes Player !");
        else Debug.Log("New Player Born!");

        _currentHealth = _initHealth;
        _healthBar.SetMaxHealth();

         StopAllCoroutines();
        // TODO: Use UI to notify the player transformation is complete 
    }
}

