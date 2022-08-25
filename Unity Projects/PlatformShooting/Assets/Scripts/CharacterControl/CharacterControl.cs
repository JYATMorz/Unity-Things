using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;
using System.Collections.Generic;

public class CharacterControl : MonoBehaviour
{
    private readonly Dictionary<string, Material> _materialInfo = new();

    private WeaponControl _weaponControl;
    private HealthControl _healthControl;
    private TargetControl _targetControl;

    private Rigidbody _characterBody;
    private NavMeshAgent _npcAgent;
    private IMenuUI _gameMenu;
    private bool _jumpPressed = false;
    private bool _onGround = false;
    private bool _onForceElevator = false;
    private bool _onNavLink = false;
    private bool _onNavMesh = true;
    private int _doubleJump = 2;
    private float _wanderScalar;

    public bool IsTeleported { get; set; } = false;
    public bool ChaseMode { get; set; } = false;

    [Header("Character Status")]
    [SerializeField] private bool _isNeutral = true;
    public bool IsNeutral { get; private set; }
    [SerializeField] private bool _isPlayer = false;
    public bool IsPlayer { get; private set; }
    public Material m_BlueTeam;
    public Material m_RedTeam;

    [Header("Game Scene Elements")]
    public GameObject sceneMenu;
    public GameObject enemyDirection;

    void Awake()
    {
        _weaponControl = GetComponent<WeaponControl>();
        _healthControl = GetComponent<HealthControl>();
        _targetControl = GetComponent<TargetControl>();

        _characterBody = GetComponent<Rigidbody>();
        _npcAgent = GetComponent<NavMeshAgent>();
        _gameMenu = sceneMenu.GetComponent<IMenuUI>();

        _npcAgent.updateRotation = false;
        IsPlayer = _isPlayer;
        IsNeutral = _isNeutral;

        _weaponControl.IsBarrelIdle = !IsPlayer;

        if (IsNeutral && IsPlayer) Debug.LogWarning("Character Setting is Wrong !");

        _materialInfo.Add(ConstantSettings.redTeamTag, m_RedTeam);
        _materialInfo.Add(ConstantSettings.blueTeamTag, m_BlueTeam);

        _wanderScalar = (UnityEngine.Random.value + 1f) * ConstantSettings.speedScalar;
    }

    void Start()
    {
        StartCoroutine(ExceptionCheck());

        if (IsPlayer)
        {
            enemyDirection.SetActive(true);
            enemyDirection.GetComponent<EnemyInstruction>().StartDirectionRing();

            _gameMenu.CurrentWeaponControl = GetComponent<WeaponControl>();
            _npcAgent.updatePosition = false;
            _npcAgent.avoidancePriority = 99;
            _characterBody.isKinematic = false;
            return;
        }

        if (IsNeutral) _npcAgent.avoidancePriority = 80;

    }

    void Update()
    {
        if (GameMenu.IsPause) return;

        if (IsPlayer)
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
        if (GameMenu.IsPause || IsTeleported || _healthControl.IsDead) return;

        if (IsPlayer)
        {
            float userInput = Input.GetAxis("Horizontal");
            if (_jumpPressed)
            {
                _characterBody.AddForce(Vector3.up * ConstantSettings.jumpScalar, ForceMode.Impulse);

                _doubleJump --;
                _jumpPressed = false;

                GeneralAudioControl.Instance.PlayAudio(ConstantSettings.jumpTag, _characterBody.position);
            }

            if (!_onForceElevator)
            {
                if (!_onGround)
                {
                    _characterBody.velocity = new Vector3(userInput * ConstantSettings.speedScalar * 0.8f, _characterBody.velocity.y, 0);
                } else
                {
                    if (Physics.Raycast(_characterBody.position, Vector3.down, out RaycastHit hit, 2f))
                    {
                        _characterBody.velocity = new Vector3(
                            Mathf.Sin(Mathf.Deg2Rad * Vector3.Angle(Vector3.right, hit.normal)) * ConstantSettings.speedScalar * userInput,
                            _characterBody.velocity.y, 0);
                    }
                }
            }

            if (!_onNavMesh && NavMesh.SamplePosition(_characterBody.position, out NavMeshHit _, 1f, NavMesh.AllAreas))
            {
                if (_npcAgent.Warp(ValidPosition(_characterBody.position))) _onNavMesh = true;
            } else if (_onNavMesh && !NavMesh.SamplePosition(_characterBody.position, out NavMeshHit _, 1f, NavMesh.AllAreas))
            {
                _onNavMesh = false;
            } else
            {
                _npcAgent.nextPosition = _characterBody.position;
            }

            return;
        }

        if (_npcAgent.isOnOffMeshLink && !_onNavLink)
        {
            _onNavLink = true;

            float distanceSqr = (_npcAgent.currentOffMeshLinkData.startPos - _npcAgent.currentOffMeshLinkData.endPos).sqrMagnitude;
            _npcAgent.speed = Mathf.LerpUnclamped(0.5f, 1.25f, distanceSqr / 64f) * ConstantSettings.speedOnNav;

        } else if (_npcAgent.isOnNavMesh && _onNavLink)
        {
            _onNavLink = false;
            _npcAgent.speed = ConstantSettings.speedOnNav;
        }

        if (ChaseMode) FindTarget();
        else WanderAround();
    }

    void OnCollisionEnter(Collision other) 
    {
        GameObject contact = other.gameObject;

        if (Array.Exists(ConstantSettings.floorTags, tag => contact.CompareTag(tag))
            && Physics.Raycast(_characterBody.position, Vector3.down, 1f))
        {
            if (IsPlayer) _doubleJump = 2;
            _onGround = true;
            _onForceElevator = false;
        }

        if (!IsPlayer && contact.CompareTag(ConstantSettings.elevatorTag))
            if (!_npcAgent.Warp(ValidPosition(_characterBody.position))) _npcAgent.nextPosition = _characterBody.position;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(ConstantSettings.elevatorTag))
        {
            _onForceElevator = true;
            BecomeFree();
        }
    }

    void OnCollisionStay()
    {
        if (!IsPlayer) return;

        if (Physics.Raycast(_characterBody.position + Vector3.down * 0.1f, Vector3.left, 0.5f) ||
            Physics.Raycast(_characterBody.position + Vector3.down * 0.1f, Vector3.right, 0.5f) ||
            Physics.Raycast(_characterBody.position + Vector3.up * 0.1f, Vector3.left, 0.5f) ||
            Physics.Raycast(_characterBody.position + Vector3.up * 0.1f, Vector3.right, 0.5f))
            _doubleJump = 1;
    }

    void OnCollisionExit(Collision other)
    {
        if (Array.Exists(ConstantSettings.floorTags, tag => other.gameObject.CompareTag(tag)))
        {
            _onGround = false;
        }
    }

    private void FindTarget()
    {
        ChaseMode = false;
        ResetNavMeshPath();
        _npcAgent.SetDestination(_targetControl.TargetPosition);
    }

    private void WanderAround()
    {
        if (!IsNeutral)
        {
            if (!_npcAgent.hasPath && !_npcAgent.pathPending)
            {
                _npcAgent.SetDestination(UnityEngine.Random.value < 0.5f
                        ? new Vector3(
                            Mathf.Sign(transform.position.x) * ConstantSettings.leftIdlePosition.x,
                            ConstantSettings.leftIdlePosition.y, 0)
                        : new Vector3(
                            Mathf.Sign(-transform.position.x) * ConstantSettings.rightIdlePosition.x,
                            ConstantSettings.rightIdlePosition.y, 0)
                );
            } else if (_npcAgent.hasPath && (_npcAgent.remainingDistance < 3f)) ResetNavMeshPath();

        } else if (_targetControl.TargetCharacter == null)
        {
            float wanderSpeed = Mathf.PingPong(Time.time, _wanderScalar) - 0.5f * _wanderScalar;
            _npcAgent.velocity = new Vector3(wanderSpeed, _characterBody.velocity.y, 0);
        }
    }

    public void SwitchToTeamLayer(string teamTag, string enemyTag)
    {
        tag = teamTag;
        IsNeutral = false;

        gameObject.layer = LayerMask.NameToLayer(teamTag);
        _targetControl.EnemyLayer= LayerMask.GetMask(enemyTag, ConstantSettings.neutralTag);
        _weaponControl.AvoidLayer = LayerMask.GetMask(teamTag, ConstantSettings.floorTag, ConstantSettings.elevatorTag);

        GetComponent<Renderer>().material = _materialInfo[teamTag];
    }

    public void BecomePlayer()
    {
        IsPlayer = true;
        _npcAgent.updatePosition = false;
        _characterBody.isKinematic = false;
        _characterBody.position = new Vector3(_characterBody.position.x, _characterBody.position.y, 0);

        if (IsNeutral && IsPlayer) Debug.LogWarning("Neutral Character becomes Player !");

        _gameMenu.CurrentWeaponControl = GetComponent<WeaponControl>();
        _gameMenu.ShowNotification("PlayerBorn");

        StopAllCoroutines();
        _weaponControl.StopAllCoroutines();
        _weaponControl.ResetWeaponStatus();
        _targetControl.StopAllCoroutines();

        GeneralAudioControl.Instance.PlayAudio(ConstantSettings.reviveTag, transform.position);
        StartCoroutine(ExceptionCheck());

        enemyDirection.SetActive(true);
        enemyDirection.GetComponent<EnemyInstruction>().StartDirectionRing();
    }

    public void BecomeDead()
    {
        _npcAgent.enabled = false;
        _characterBody.isKinematic = false;
        _characterBody.freezeRotation = false;
        _characterBody.constraints = RigidbodyConstraints.None;

        if (IsPlayer)
        {
            IsPlayer = false;
            _gameMenu.ShowNotification("PlayerDie");

            enemyDirection.SetActive(false);
            enemyDirection.GetComponent<EnemyInstruction>().StopAllCoroutines();
        }

        StopAllCoroutines();

        _characterBody.position = new Vector3(_characterBody.position.x, _characterBody.position.y, UnityEngine.Random.Range(0, 2) - 0.5f);
        _characterBody.AddTorque(
            new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value) * 0.1f, 
            ForceMode.Impulse);

        StartCoroutine(ExceptionCheck());

    }

    public void BecomeFree()
    {
        if (IsPlayer || MainCamera.IsGameOver || CompareTag(ConstantSettings.deadTag)) return;

        _npcAgent.isStopped = true;
        _npcAgent.updatePosition = false;
        ResetNavMeshPath();
        _characterBody.isKinematic = false;

        StartCoroutine(BecomeUnfree());
    }

    IEnumerator BecomeUnfree()
    {
        yield return new WaitForSeconds(0.2f);

        float deltaDistance = (_targetControl.TargetCharacter != null)
                            ? (_targetControl.TargetPosition.x - _characterBody.position.x)
                            : (UnityEngine.Random.value - 0.5f);
        float horizontalForce = ConstantSettings.speedScalar * Mathf.Sign(deltaDistance);
        _characterBody.AddForce(horizontalForce * (UnityEngine.Random.value + 1f), 0, 0, ForceMode.Impulse);

        while (!_onGround || _onForceElevator)
        {
            yield return new WaitForFixedUpdate();
        }
        if (_npcAgent.isOnNavMesh)
        {
            if (!_npcAgent.Warp(ValidPosition(_characterBody.position))
                && NavMesh.SamplePosition(_characterBody.position, out NavMeshHit hit, 1f, NavMesh.AllAreas))
            _npcAgent.Warp(ValidPosition(hit.position));
        }

        if (_targetControl.TargetCharacter != null 
            && ConstantSettings.TargetInRange(_targetControl.TargetPosition, _characterBody.position, ConstantSettings.seekRange))
        {
            ResetNavMeshPath();
            _npcAgent.SetDestination(_targetControl.TargetPosition);
        } else ChaseMode = false;

        _npcAgent.isStopped = false;
        _npcAgent.updatePosition = true;
        _characterBody.isKinematic = true;
    }

    private void ResetNavMeshPath()
    {
        if (_npcAgent.hasPath || _npcAgent.pathPending) _npcAgent.ResetPath();
    }

    IEnumerator ExceptionCheck()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(3f);

            if (transform.position.y < -15 || transform.position.y > 25)
            {
                if (CompareTag(ConstantSettings.deadTag))
                {
                    Destroy(gameObject);
                    break;
                } else
                {
                    _healthControl.ReceiveDamage(1000, null);
                }
            }

            if (!_healthControl.IsDead && !Mathf.Approximately(transform.position.z, 0))
                transform.position = new Vector3(transform.position.x, transform.position.y, 0);

            if (_targetControl.TargetCharacter == null || _targetControl.TargetCharacter.CompareTag(tag) || _targetControl.TargetCharacter.CompareTag(ConstantSettings.deadTag))
            {
                ChaseMode = false;
                ResetNavMeshPath();
            }
        }
    }

    private Vector3 ValidPosition(Vector3 targetPos)
    {
        Vector3 position = targetPos;
        if (position.y > 23.5f) position.y = 23f;
        return position;
    }
}