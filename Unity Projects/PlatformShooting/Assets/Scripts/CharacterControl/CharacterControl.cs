using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;
using System.Collections.Generic;

public class CharacterControl : MonoBehaviour
{
    private const float _speedScaler = 5f;
    private const float _jumpScaler = 20f;

    private readonly Dictionary<string, Material> _materialInfo = new();

    private WeaponControl _weaponControl;
    private HealthControl _healthControl;
    private TargetControl _targetControl;

    private Rigidbody _characterBody;
    // private NavMeshAgent _npcAgent;
    private IMenuUI _gameMenu;
    private bool _jumpPressed = false;
    private bool _onGround = false;
    private bool _onElevator = false;
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
        // _npcAgent = GetComponent<NavMeshAgent>();
        _gameMenu = sceneMenu.GetComponent<IMenuUI>();

        // _npcAgent.updateRotation = false;
        _weaponControl.IsBarrelIdle = !_isPlayer;
        IsPlayer = _isPlayer;
        IsNeutral = _isNeutral;

        if (IsNeutral && IsPlayer) Debug.LogWarning("Character Setting is Wrong !");

        _materialInfo.Add(ConstantSettings.redTeamTag, m_RedTeam);
        _materialInfo.Add(ConstantSettings.blueTeamTag, m_BlueTeam);

    }

    void Start()
    {
        StartCoroutine(OutOfMapCheck());

        if (IsPlayer)
        {
            _gameMenu.CurrentWeaponControl = GetComponent<WeaponControl>();// FIXME
            // _npcAgent.updatePosition = false;
            // _characterBody.isKinematic = false;
            // FIXME: https://docs.unity.cn/2021.3/Documentation/Manual/nav-MixingComponents.html
            return;
        }

        // StartCoroutine(WanderAround());
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

            return;
        }

        if (ChaseMode) ChaseTarget();
    }

    void OnCollisionEnter(Collision other) 
    {
        GameObject contact = other.gameObject;

        if (Array.Exists(ConstantSettings.floorTags, tag => tag == contact.tag))
        {
            if (IsPlayer) _doubleJump = 2;
            _onGround = true;
            _onElevator = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(ConstantSettings.elevatorTag)) _onElevator = true;
    }

    void OnCollisionStay()
    {
        if (IsPlayer && _doubleJump == 0) _doubleJump = 2;
    }

    void OnCollisionExit(Collision other)
    {
        if (Array.Exists(ConstantSettings.floorTags, tag => tag == other.gameObject.tag))
        {
            _onGround = false;
        }
    }

    private void ChaseTarget()
    {
        ChaseMode = false;

        if (_targetControl.TargetCharacter == null) return;

        if (!ConstantSettings.TargetInRange(_targetControl.TargetPosition, transform.position, 0.5f * _weaponControl.shootRange))
        {
            // _npcAgent.SetDestination(_targetControl.TargetPosition);
        }
    }

    // TODO: https://docs.unity3d.com/ScriptReference/AI.NavMeshAgent.CompleteOffMeshLink.html
    private void JumpAcrossGap()
    {
        // FIXME: _npcAgent.autoTraverseOffMeshLink = false;

        // in start(): StartCoroutine(StartNavMeshJump());
    }

    // IEnumerator StartNavMeshJump()
    // {
    //     if (_npcAgent.isOnOffMeshLink)
    //     {
    //         yield return StartCoroutine(ParabolaJump(_npcAgent, 1f, 1f));
    //         _npcAgent.CompleteOffMeshLink();
    //     }
    //     yield return null;
    // }

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

    // private IEnumerator WanderAround()
    // {
    //     while (true)
    //     {
    //         if (!_npcAgent.hasPath && !_npcAgent.pathPending)
    //             _npcAgent.velocity = (_targetControl.TargetCharacter == null) ?
    //                 new Vector3(Mathf.PingPong(Time.time, _speedScaler) - 0.5f * _speedScaler, _characterBody.velocity.y, 0)
    //                 : Vector3.zero;

    //         yield return new WaitForFixedUpdate();
    //     }
    // }

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
        // _npcAgent.updatePosition = false;
        // _characterBody.isKinematic = false;

        if (IsNeutral && IsPlayer) Debug.LogWarning("Neutral Character becomes Player !");

        _gameMenu.CurrentWeaponControl = GetComponent<WeaponControl>();
        _gameMenu.ShowNotification("PlayerBorn");

        StopAllCoroutines();
        _weaponControl.StopAllCoroutines();

    }

    public void BecomeDead()
    {
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

