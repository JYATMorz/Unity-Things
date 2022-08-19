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

    }

    void Start()
    {
        StartCoroutine(OutOfMapCheck());

        if (IsPlayer)
        {
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
        if (IsTeleported || _healthControl.IsDead) return;

        if (IsPlayer)
        {
            float userInput = Input.GetAxis("Horizontal");
            if (_jumpPressed)
            {
                _characterBody.AddForce(Vector3.up * ConstantSettings.jumpScaler, ForceMode.Impulse);

                _doubleJump --;
                _jumpPressed = false;
            }

            if (!_onForceElevator)
            {
                if (_doubleJump < 2 || !_onGround)
                    _characterBody.velocity = new Vector3(
                            userInput * ConstantSettings.speedScaler,
                            _characterBody.velocity.y, 0);
                else
                    _characterBody.velocity = Vector3.ClampMagnitude(
                            new Vector3(userInput * ConstantSettings.speedScaler, _characterBody.velocity.y, 0),
                            ConstantSettings.speedScaler);
            }

            if (!_onNavMesh && _npcAgent.isOnNavMesh)
            {
                if (_npcAgent.Warp(_characterBody.position)) _onNavMesh = true;
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
            _npcAgent.speed = Mathf.LerpUnclamped(0.6f, 1.2f, distanceSqr / 25f);
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

        if (Array.Exists(ConstantSettings.floorTags, tag => tag == contact.tag))
        {
            if (IsPlayer) _doubleJump = 2;
            _onGround = true;
            _onForceElevator = false;
        }

        if (!IsPlayer && contact.CompareTag(ConstantSettings.elevatorTag)) _npcAgent.Warp(_characterBody.position);
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
        if (IsPlayer && _doubleJump == 0) _doubleJump = 2;
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
        _npcAgent.SetDestination(_targetControl.TargetPosition);
    }

    private void WanderAround()
    {
        if (!_npcAgent.hasPath && !_npcAgent.pathPending)
        {
            float wanderSpeed = Mathf.PingPong(Time.time, ConstantSettings.speedScaler) - 0.5f * ConstantSettings.speedScaler;
            _npcAgent.velocity = new Vector3(wanderSpeed * (UnityEngine.Random.value / 2 + 0.75f), _characterBody.velocity.y, 0);
        }
    }

    public void SwitchToTeamLayer(string teamTag, string enemyTag)
    {
        gameObject.tag = teamTag;
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

        if (IsNeutral && IsPlayer) Debug.LogWarning("Neutral Character becomes Player !");

        _gameMenu.CurrentWeaponControl = GetComponent<WeaponControl>();
        _gameMenu.ShowNotification("PlayerBorn");

        StopAllCoroutines();
        _weaponControl.StopAllCoroutines();

        StartCoroutine(OutOfMapCheck());
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
        }

        StopAllCoroutines();

        _characterBody.position = new Vector3(_characterBody.position.x, _characterBody.position.y, UnityEngine.Random.Range(0, 2) - 0.5f);
        _characterBody.AddTorque(
            new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value) * 0.1f, 
            ForceMode.Impulse);

        StartCoroutine(OutOfMapCheck());

    }

    public void BecomeFree()
    {
        if (IsPlayer || MainCamera.IsGameOver) return;

        _npcAgent.isStopped = true;
        _npcAgent.updatePosition = false;
        _npcAgent.ResetPath();
        _characterBody.isKinematic = false;

        StartCoroutine(BecomeUnfree());
    }

    IEnumerator BecomeUnfree()
    {
        yield return new WaitForSeconds(0.2f);

        float deltaDistance = (_targetControl.TargetCharacter != null)
                            ? (_targetControl.TargetPosition.x - _characterBody.position.x)
                            : (UnityEngine.Random.value - 0.5f);
        float horizontalForce = ConstantSettings.speedScaler * Mathf.Sign(deltaDistance);
        _characterBody.AddForce(horizontalForce * (UnityEngine.Random.value + 1f), 0, 0, ForceMode.Impulse);

        while (!_onGround || _onForceElevator)
        {
            yield return new WaitForFixedUpdate();
        }
        if (_npcAgent.isOnNavMesh)
        {
            if (!_npcAgent.Warp(_characterBody.position) && _npcAgent.FindClosestEdge(out NavMeshHit hit))
                _npcAgent.Warp(hit.position);
        }

        if (_targetControl.TargetCharacter != null 
            && ConstantSettings.TargetInRange(_targetControl.TargetPosition, _characterBody.position, ConstantSettings.seekRange))
            _npcAgent.SetDestination(_targetControl.TargetPosition);
        _npcAgent.isStopped = false;
        _npcAgent.updatePosition = true;
        _characterBody.isKinematic = true;
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

            if (!_healthControl.IsDead && !Mathf.Approximately(transform.position.z, 0))
                transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        }
    }

}