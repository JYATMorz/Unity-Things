using UnityEngine;
using UnityEngine.AI;
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
    private const float _jumpScaler = 20f;
    private const float _attackWillingness = 0.6f;
    private const string _bulletTag = "Bullet";
    private const string _neutralTag = "Neutral";
    private const string _blueTeamTag = "BlueTeam";
    private const string _redTeamTag = "RedTeam";
    private const string _deadTag = "Dead";

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
    private WeaponControl _weaponControl;
    private bool _chaseMode = false;
    private bool _jumpPressed = false;
    private bool _onGround = false;
    private bool _onElevator = false;
    private bool _isDead = false;
    private int _doubleJump = 2;
    private int _currentHealth = _initHealth;
    private int _floorLayer;
    private int _avoidLayer;
    // private int _neutralLayer;
    private int _deadLayer;
    private int _enemyLayer = -1;
    private int _ammoTypeNum = 0;
    private float _rotateSpeed = _barrelRotateSpeed;

    public bool IsTeleported { get; set; } = false;

    [Header("Character Status")]
    [SerializeField] private bool _isNeutral = true;
    [SerializeField] private bool _isPlayer = false;
    public Material m_BlueTeam;
    public Material m_RedTeam;

    [Header("Scene Elements")]
    public Camera mainCamera;
    public GameMenu gameMenu;
    public NavMeshAgent npcAgent;

    void Awake()
    {
        _characterBody = GetComponent<Rigidbody>();
        _barrelShaft = GetComponentsInChildren<Rigidbody>()[1];
        _weaponControl = GetComponent<WeaponControl>();

        _floorLayer = LayerMask.GetMask("Floor", "Elevator");
        _avoidLayer = _floorLayer;
        _deadLayer = LayerMask.NameToLayer(_deadTag);

        _ammoTypeNum = UnityEngine.Random.Range(0, _ammoTypes.Length);

        if (_isNeutral && _isPlayer) Debug.LogWarning("Character Setting is Wrong !");
        if (!_isNeutral)
        {
            string enemyTag = CompareTag(_blueTeamTag) ? _redTeamTag : _blueTeamTag;
            _enemyLayer = LayerMask.GetMask(enemyTag, _neutralTag);
        }
    }

    void Start()
    {
        _healthBar = GetComponentInChildren<HealthBar>();

        if (_isPlayer)
        {
            gameMenu.SwitchWeaponIcon(_ammoTypeNum);
            return;
        }

        StartCoroutine(SeekEnemy());
        StartCoroutine(WanderAround());
    }

    void Update()
    {
        if (GameMenu.IsPause || MainCamera.IsGameOver) return;

        if (_isPlayer)
        {
            if (Input.GetKeyDown(KeyCode.Space) && (_doubleJump > 0)) 
                _jumpPressed = true;

            if (Input.GetKey(KeyCode.Mouse0))
                _weaponControl.BarrelShoot(_ammoTypes[_ammoTypeNum]);
            else
                _weaponControl.StopShoot();

            if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeWeapon(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeWeapon(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeWeapon(2);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) ChangeWeapon(3);
            else if (Input.mouseScrollDelta.y != 0) ChangeWeapon((_ammoTypeNum + (int)Input.mouseScrollDelta.y) % 4);
        }

    }

    void FixedUpdate()
    {
        // if (GameMenu.IsPause || MainCamera.IsGameOver) return;
        if (IsTeleported) return;

        if (_isDead) return;
    
        if (_isPlayer)
        {
            if (_jumpPressed)
            {
                _characterBody.AddForce(Vector3.up * _jumpScaler, ForceMode.Impulse);

                _doubleJump --;
                _jumpPressed = false;
            }

            if (!_onElevator)
            {
                if (_doubleJump < 2 || !_onGround)
                    _characterBody.velocity = new Vector3(Input.GetAxis("Horizontal") * _speedScaler, _characterBody.velocity.y, 0);
                else 
                    _characterBody.velocity = Vector3.ClampMagnitude(
                        new Vector3(Input.GetAxis("Horizontal") * _speedScaler, _characterBody.velocity.y, 0), _speedScaler);
            }

            // Rotate the barrel to point at the mouse position
            Vector3 _rotateVector = Input.mousePosition - mainCamera.WorldToScreenPoint(_barrelShaft.position);
            _rotateVector.z = 0;
            _barrelShaft.rotation *= Quaternion.FromToRotation(_barrelShaft.transform.up, _rotateVector);

            return;
        }

        if (_targetCharacter != null && !ObstacleBetween(_targetCharacter.transform.position))
            _targetPosition = _targetCharacter.transform.position;

        // Control Barrel Here
        if (_targetCharacter == null) BarrelIdle();
        else if (_targetCharacter.CompareTag(_deadTag) || CompareTag(_targetCharacter.tag))
        {
            ResetTargetCharacter();
            BarrelIdle();
        }
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
            if (!_isPlayer) SwitchTarget(contact.GetComponentsInParent<Rigidbody>()[2]);

        } else if (Array.Exists(_floorTag, tag => tag == contact.tag))
        {
            if (_isPlayer) _doubleJump = 2;
            _onGround = true;
            _onElevator = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Elevator")) _onElevator = true;
    }

    void OnCollisionStay()
    {
        if (_isPlayer && _doubleJump == 0) _doubleJump = 2;
    }

    void OnCollisionExit(Collision other)
    {
        if (Array.Exists(_floorTag, tag => tag == other.gameObject.tag))
        {
            _onGround = false;
        }
    }

    private IEnumerator SeekEnemy()
    {
        while(true && !_isDead)
        {
            if(!_isNeutral)
            {
                if (_targetCharacter == null) SearchTarget();
                else if (_targetCharacter.CompareTag(_deadTag) || _targetCharacter.CompareTag(gameObject.tag)) SearchTarget();
                else
                {
                    if (!TargetInRange(_targetCharacter.transform.position, _seekRange))
                        ResetTargetCharacter();
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
            if ((UnityEngine.Random.value < _attackWillingness) && !ObstacleBetween(target.transform.position))
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
            if (CompareTag(suspect.tag)) return;

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
        _weaponControl.StopShoot();

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
            if (!ObstacleBetween(_targetPosition, _avoidLayer))
                _weaponControl.BarrelShoot(_ammoTypes[_ammoTypeNum]);
            else
                _weaponControl.StopShoot();
        } else
            _weaponControl.StopShoot();
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
        // FIXME: working weirdly
        return Physics.Raycast(_barrelShaft.position, _barrelShaft.transform.up, _shootRange, ~_targetCharacter.layer);
    }

    public void ReceiveDamage(int damage, Rigidbody attacker = null)
    {
        if (attacker != null) SwitchTarget(attacker);

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

    private void FullHealth()
    {
        _currentHealth = _initHealth;
        _healthBar.SetMaxHealth();
    }

   private void ZeroHealth()
    {
        if (!_isNeutral)
        {
            gameMenu.CharacterDie(gameObject.tag);
            _weaponControl.StopShoot();

            DeadTagAndLayer();
            _characterBody.freezeRotation = false;

            if (_isPlayer)
            {
                _isPlayer = false;
                gameMenu.ShowNotification("PlayerDie");
            }

            // TODO: Big Dead Smoke Effect
            StopAllCoroutines();
            _characterBody.position = new Vector3(_characterBody.position.x, _characterBody.position.y, UnityEngine.Random.Range(0, 2) - 0.5f);
            _characterBody.AddTorque(
                new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value) * 0.1f, 
                ForceMode.Impulse);

            _isDead = true;

            return;
        }

        if (_targetCharacter != null)
        {
            _isNeutral = false;
            FullHealth();
            _targetCharacter.GetComponent<CharacterControl>().FullHealth();

            if (_targetCharacter.CompareTag(_redTeamTag))
            {
                gameObject.tag = _redTeamTag;
                SwitchLayer(_redTeamTag, _blueTeamTag);
                GetComponent<Renderer>().material = m_RedTeam;
            } else if (_targetCharacter.CompareTag(_blueTeamTag))
            {
                gameObject.tag = _blueTeamTag;
                SwitchLayer(_blueTeamTag, _redTeamTag);
                GetComponent<Renderer>().material = m_BlueTeam;
            }

            ResetTargetCharacter();
            gameMenu.TeamChanged(gameObject.tag);

        } else Debug.Log("Target is null when changing team!");
    }

    private IEnumerator WanderAround()
    {
        while (true)
        {
            if ((_targetCharacter == null) && !_chaseMode)
            {
                _characterBody.velocity = new Vector3(Mathf.PingPong(Time.time, _speedScaler) - 0.5f * _speedScaler, _characterBody.velocity.y, 0);
            } else _characterBody.velocity = new Vector3(0, _characterBody.velocity.y, 0);

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
        _enemyLayer = LayerMask.GetMask(enemyTag, _neutralTag);
    }

    private void DeadTagAndLayer()
    {
        gameObject.tag = _deadTag;
        gameObject.layer = _deadLayer;
    }

    public void BecomePlayer()
    {
        _isPlayer = true;
        if (_isNeutral && _isPlayer) Debug.LogWarning("Neutral Character becomes Player !");
        else Debug.Log("New Player Born!");

        FullHealth();

        StopAllCoroutines();
        gameMenu.ShowNotification("PlayerBorn");
    }

    private void ChangeWeapon(int num)
    {
        _ammoTypeNum = (num < 0) ? (num + _ammoTypes.Length) : num;

        gameMenu.SwitchWeaponIcon(_ammoTypeNum);
        // TODO: switch different vfx
    }
}

