using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;

public class CharacterControl : MonoBehaviour
{
    private const int _initHealth = 100;
    private const float _seekInterval = 1f;
    private const float _seekRange = 30f;
    private const float _speedScaler = 5f;
    private const float _jumpScaler = 20f;
    private const string _bulletTag = "Bullet";
    private const string _neutralTag = "Neutral";
    private const string _blueTeamTag = "BlueTeam";
    private const string _redTeamTag = "RedTeam";
    private const string _deadTag = "Dead";

    private readonly string[] _floorTag = { "Floor", "Elevator" };

    private GameObject _targetCharacter = null;
    private HealthBar _healthBar;
    private Rigidbody _characterBody;
    private NavMeshAgent _npcAgent;
    private Rigidbody _barrelShaft;
    private WeaponControl _weaponControl;
    private IMenuUI _gameMenu;
    private bool _chaseMode = false;
    private bool _jumpPressed = false;
    private bool _onGround = false;
    private bool _onElevator = false;
    private bool _isDead = false;
    private int _doubleJump = 2;
    private int _currentHealth = _initHealth;
    private int _floorLayer;
    private int _deadLayer;
    private int _enemyLayer = -1;

    public bool IsTeleported { get; set; } = false;

    [Header("Character Status")]
    [SerializeField] private bool _isNeutral = true;
    [SerializeField] private bool _isPlayer = false;
    public Material m_BlueTeam;
    public Material m_RedTeam;

    [Header("Game Scene Elements")]
    public Camera mainCamera;
    public GameObject sceneMenu;

    void Awake()
    {
        _characterBody = GetComponent<Rigidbody>();
        _barrelShaft = GetComponentsInChildren<Rigidbody>()[1];
        _weaponControl = GetComponent<WeaponControl>();
        _npcAgent = GetComponent<NavMeshAgent>();
        _gameMenu = sceneMenu.GetComponent<IMenuUI>();

        _floorLayer = LayerMask.GetMask("Floor", "Elevator");
        _deadLayer = LayerMask.NameToLayer(_deadTag);

        _npcAgent.updateRotation = false;
        _weaponControl.IsBarrelIdle = !_isPlayer;
        _weaponControl.IsPlayer = _isPlayer;

        if (_isNeutral && _isPlayer) Debug.LogWarning("Character Setting is Wrong !");
        if (!_isNeutral)
        {
            string enemyTag = CompareTag(_blueTeamTag) ? _redTeamTag : _blueTeamTag;
            _enemyLayer = LayerMask.GetMask(enemyTag, _neutralTag);
        }
    }

    void Start()
    {
        StartCoroutine(OutOfMapCheck());

        _healthBar = GetComponentInChildren<HealthBar>();

        if (_isPlayer)
        {
            _gameMenu.CurrentWeaponControl = GetComponent<WeaponControl>();
            _npcAgent.updatePosition = false;
            _characterBody.isKinematic = false;
            // FIXME: https://docs.unity.cn/2021.3/Documentation/Manual/nav-MixingComponents.html
            return;
        }

        StartCoroutine(SeekEnemy());
        StartCoroutine(WanderAround());
    }

    void Update()
    {
        if (GameMenu.IsPause) return;

        if (_isPlayer)
        {
            if (Input.GetKeyDown(KeyCode.Space) && (_doubleJump > 0)) 
                _jumpPressed = true;

            if (Input.GetKey(KeyCode.Mouse0))
                _weaponControl.BarrelShoot();
            else
                _weaponControl.StopShoot();

            if (MainCamera.IsGameOver) return;

            if (Input.GetKeyDown(KeyCode.Alpha1)) _weaponControl.ChangeWeapon(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) _weaponControl.ChangeWeapon(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) _weaponControl.ChangeWeapon(2);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) _weaponControl.ChangeWeapon(3);
            else if (Input.mouseScrollDelta.y != 0) 
                _weaponControl.ChangeWeapon((int)Input.mouseScrollDelta.y, true);
        }

    }

    void FixedUpdate()
    {
        if (IsTeleported || _isDead) return;
    
        if (_isPlayer)
        {
            if (_jumpPressed)
            {
                _characterBody.AddForce(Vector3.up * _jumpScaler, ForceMode.Impulse);

                _doubleJump --;
                _jumpPressed = false;
            }

            // FIXME: character movement without clamping other forces
            if (!_onElevator)
            {
                if (_doubleJump < 2 || !_onGround)
                    _characterBody.velocity = new Vector3(Input.GetAxis("Horizontal") * _speedScaler, _characterBody.velocity.y, 0);
                else
                {
                    _characterBody.velocity = Vector3.ClampMagnitude(
                        new Vector3(Input.GetAxis("Horizontal") * _speedScaler, _characterBody.velocity.y, 0), _speedScaler);
                }

            }

            // Rotate the barrel to point at the mouse position
            Vector3 _rotateVector = Input.mousePosition - mainCamera.WorldToScreenPoint(_barrelShaft.position);
            _rotateVector.z = 0;
            _barrelShaft.rotation *= Quaternion.FromToRotation(_barrelShaft.transform.up, _rotateVector);

            return;
        }

        if (_chaseMode) ChaseTarget();

        if (_targetCharacter == null) _weaponControl.IsBarrelIdle = true;
        else if (_targetCharacter.CompareTag(_deadTag) || CompareTag(_targetCharacter.tag))
            ResetTargetCharacter();
        else if (!ObstacleBetween(_targetCharacter.transform.position))
        {
            _weaponControl.IsBarrelIdle = false;
            _weaponControl.TargetPosition = _targetCharacter.transform.position;
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
                else
                {
                    Vector3 targetPos = _targetCharacter.transform.position;
                    if (!TargetInRange(targetPos, _seekRange))
                        ResetTargetCharacter();
                    else// if (!_npcAgent.hasPath && !_npcAgent.pathPending)
                        _chaseMode = true;
                }
            }
            yield return new WaitForSeconds(_seekInterval);
        }
    }

    private void SearchTarget()
    {
        foreach (Collider target in Physics.OverlapSphere(transform.position, _seekRange, _enemyLayer))
        {
            if (!ObstacleBetween(target.transform.position))
            {
                _targetCharacter = target.gameObject;
                break;
            }
        }
    }

    private void SwitchTarget(Rigidbody suspect)
    {
        Vector3 suspectPosition = suspect.position;

        if (_isNeutral) // I'm a neutral character
        {
            if (_targetCharacter == null) // Currently has no target
                _targetCharacter = suspect.gameObject;
            else if (_targetCharacter.CompareTag(_neutralTag)) // Currently has neutral target
            {
                if (suspect.CompareTag(_neutralTag))
                    // 50% chance to switch from one neutral target to another
                    _targetCharacter = (UnityEngine.Random.value < 0.5f) ? suspect.gameObject : _targetCharacter;
                else
                    // 100% chance to switch from one neutral target to team target
                    _targetCharacter = suspect.gameObject;
            } else if (!suspect.CompareTag(_neutralTag)) // Currently has team target & suspect is not neutral
            {
                if (!TargetInRange(_targetCharacter.transform.position, _seekRange)) // Current target is out of seek range
                {
                    _targetCharacter = suspect.gameObject;
                } else if (!TargetInRange(_targetCharacter.transform.position, WeaponControl.ShootRange)) // Current target is out of shoot range
                {
                    _targetCharacter = ObstacleBetween(suspectPosition) ? _targetCharacter : suspect.gameObject;
                }
            }
        } else // I'm a team character
        {
            if (CompareTag(suspect.tag)) return; // Attack by teammate

            if (_targetCharacter == null) _targetCharacter = suspect.gameObject;
            else if (!suspect.CompareTag(_neutralTag))
            {
                if (_targetCharacter.CompareTag(_neutralTag)) _targetCharacter = suspect.gameObject;
                else _targetCharacter =
                        (ObstacleBetween(_targetCharacter.transform.position) && !ObstacleBetween(suspectPosition))
                        ? suspect.gameObject : _targetCharacter;
            }
        }

        if (_targetCharacter != null) _chaseMode = true;
    }

    private void ChaseTarget()
    {
        _chaseMode = false;

        if (_targetCharacter == null) return;

        Vector3 targetPosition = _targetCharacter.transform.position;
        if (!TargetInRange(targetPosition, 0.5f * WeaponControl.ShootRange) && !ObstacleBetween(targetPosition))
        {
            _npcAgent.SetDestination(targetPosition);
        }
    }

    // TODO: https://docs.unity3d.com/ScriptReference/AI.NavMeshAgent.CompleteOffMeshLink.html
    private void JumpAcrossGap()
    {
        // FIXME: _npcAgent.autoTraverseOffMeshLink = false;

        // in start(): StartCoroutine(StartNavMeshJump());
    }

    IEnumerator StartNavMeshJump()
    {
        if (_npcAgent.isOnOffMeshLink)
        {
            yield return StartCoroutine(ParabolaJump(_npcAgent, 1f, 1f));
            _npcAgent.CompleteOffMeshLink();
        }
        yield return null;
    }

    IEnumerator ParabolaJump(NavMeshAgent agent, float height, float duration)
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 startPos = agent.transform.position;
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
        float normalizedTime = 0f;
        while (normalizedTime < 1f)
        {
            float yOffset = height * 4f * (normalizedTime - normalizedTime * normalizedTime);
            agent.transform.position = Vector3.Lerp(startPos, endPos, normalizedTime) + yOffset * Vector3.up;
            normalizedTime += Time.deltaTime / duration;
            yield return null;
        }
    }

    private bool TargetInRange(Vector3 targetPosition, float range)
        => (targetPosition - transform.position).sqrMagnitude < range * range;

    private bool ObstacleBetween(Vector3 targetPosition, int layer = -1)
    {
        if (layer == -1) layer = _floorLayer;
        return Physics.Linecast(transform.position, targetPosition, layer);
    }

    public void ReceiveDamage(int damage, Rigidbody attacker = null)
    {
        if (!_isPlayer && attacker != null) SwitchTarget(attacker);

        // FIXME: _currentHealth -= Mathf.Clamp(damage, 0, 25);

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
            _gameMenu.CharacterDie(gameObject.tag);
            _weaponControl.StopShoot();
            _weaponControl.StopAllCoroutines();

            DeadTagAndLayer();
            _characterBody.freezeRotation = false;
            _characterBody.constraints = RigidbodyConstraints.None;
            _npcAgent.enabled = false;
            _characterBody.isKinematic = false; //FIXME

            if (_isPlayer)
            {
                _isPlayer = false;
                _weaponControl.IsPlayer = _isPlayer;
                _gameMenu.ShowNotification("PlayerDie");
            }

            StopAllCoroutines();
            _characterBody.position = new Vector3(_characterBody.position.x, _characterBody.position.y, UnityEngine.Random.Range(0, 2) - 0.5f);
            _characterBody.AddTorque(
                new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value) * 0.1f, 
                ForceMode.Impulse);

            _isDead = true;
            StartCoroutine(OutOfMapCheck());

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
                SwitchToTeamLayer(_redTeamTag, _blueTeamTag);
                GetComponent<Renderer>().material = m_RedTeam;
            } else if (_targetCharacter.CompareTag(_blueTeamTag))
            {
                gameObject.tag = _blueTeamTag;
                SwitchToTeamLayer(_blueTeamTag, _redTeamTag);
                GetComponent<Renderer>().material = m_BlueTeam;
            }

            ResetTargetCharacter();
            _gameMenu.TeamChanged(gameObject.tag);

        } else Debug.Log("Target is null when changing team!");
    }

    private IEnumerator WanderAround()
    {
        while (true)
        {
            if (!_npcAgent.hasPath && !_npcAgent.pathPending)
                _npcAgent.velocity = (_targetCharacter == null) ?
                    new Vector3(Mathf.PingPong(Time.time, _speedScaler) - 0.5f * _speedScaler, _characterBody.velocity.y, 0)
                    : Vector3.zero;

            yield return new WaitForFixedUpdate();
        }
    }

    private void ResetTargetCharacter() => _targetCharacter = null;

    private void SwitchToTeamLayer(string teamTag, string enemyTag)
    {
        gameObject.layer = LayerMask.NameToLayer(teamTag);
        _enemyLayer = LayerMask.GetMask(enemyTag, _neutralTag);

        _weaponControl.AvoidLayer = LayerMask.GetMask(teamTag, "Floor", "Elevator");
    }

    private void DeadTagAndLayer()
    {
        gameObject.tag = _deadTag;
        gameObject.layer = _deadLayer;
    }

    public void BecomePlayer()
    {
        _isPlayer = true;
        _npcAgent.updatePosition = false;
        _characterBody.isKinematic = false; // FIXME
        _weaponControl.IsPlayer = _isPlayer;

        if (_isNeutral && _isPlayer) Debug.LogWarning("Neutral Character becomes Player !");

        _gameMenu.CurrentWeaponControl = GetComponent<WeaponControl>();
        _gameMenu.ShowNotification("PlayerBorn");

        StopAllCoroutines();
        _weaponControl.StopAllCoroutines();

    }

    IEnumerator OutOfMapCheck()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(5f);

            if (transform.position.y < -15)
            {
                Destroy(gameObject);
                break;
            }

            if (!_isDead && !Mathf.Approximately(transform.position.z, 0))
                transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        }
    }

    public void TempAgentOff(float time = 1f)
    {
        if (_npcAgent.enabled)
        {
            _npcAgent.enabled = false;
            _characterBody.isKinematic = false;
            StartCoroutine(ReEnableAgent(time));
        }
    }

    IEnumerator ReEnableAgent(float limit)
    {
        yield return new WaitForSeconds(limit);
        if (!_isDead)
        {
            _npcAgent.enabled = true;
            _characterBody.isKinematic = true;
        }
    }
}

