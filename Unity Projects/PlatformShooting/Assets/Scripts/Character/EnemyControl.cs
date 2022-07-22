using UnityEngine;

public class EnemyControl : MonoBehaviour
{
    private const int _maxPlayerNum = 2;
    private const int _initHealth = 50;
    private const string _bulletTag = "Bullet";
    private const string _floorTag = "Floor";
    private const float _seekRange = 20f;
    private const float _seekInterval = 1f;
    private const float _shootRange = 10f;
    private const float _revengeLimit = 10f;

    private readonly int _playerLayer;
    private readonly int _floorLayer;

    public GameObject Target { get; private set; } = null;
    public int CurrentHealth { get; private set; }
    
    private HealthBar _healthBar;
    private Rigidbody _characterBody;
    private GameObject _barrelShaft;
    private GameObject _suspectTarget;
    private bool _revengeMode = false;
    private float _revengeStart = 0f;
    private int _barrelRotateSpeed = 45;

    void Awake()
    {
        _characterBody = GetComponent<Rigidbody>();
        _barrelShaft = GetComponentInChildren<GameObject>();
        Debug.Log(_barrelShaft.name);
    }

    void Start()
    {
        CurrentHealth = _initHealth;
        _healthBar = GetComponentInChildren<HealthBar>();
        _playerLayer = Layer.GetMask("Player");
        _floorLayer = Layer.GetMask("Floor");

        StartCoroutine(SeekPlayer());
    }

    void FixedUpdate()
    {
        // Control Movement Here

        // Control Barrel Here
        if (Target == null) BarrelIdle();
        else BarrelAim();
    }

    void OnCollisionEnter(Collision other) 
    {
        GameObject collideObj = other.gameObject;

        if (collideObj.CompareTag(_bulletTag))
        {
            _suspectTarget = collideObj.GetComponentsInParent<GameObject>()[1];
            Debug.Log(_suspectTarget.name);
            // WARNING: Change when barrel rotation center lose its rigid body
            if (_suspectTarget.name != gameObject.name)
            {
                if (TargetInRange(_suspectTarget.position, _seekRange))
                {
                    Target = suspect;
                    _revengeMode = true;
                    Invoke("ContinueRevenge", _revengeLimit);
                    // TODO: Maybe show the difference in revenge mode
                }
            }

            CommonBullet ammoScript = collideObj.GetComponent<CommonBullet>();
            ReceiveDamage(ammoScript.ammoDamage);
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
                    Target = playerCollidersFound[Random.Range(0, playerCount)].gameObject;
                    // Can I see the target? 
                    if (!NoObstacleBetween(Target.transform.position)) Target = null;
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
        Transform shaftTransform = _barrelShaft.transform;
        if (Quaternion.Angle(Quaternion.identity, shaftTransform.rotation) > 105) _barrelRotateSpeed *= -1;

        shaftTransform.RotateAround(shaftTransform.position, shaftTransform.forward, _barrelRotateSpeed * Time.fixedDeltaTime);
    }

    private void BarrelAim()
    {
        Transform shaftTransform = _barrelShaft.transform;
        Quaternion targetRotation = Quaternion.FromToRotation(shaftTransform.forward, Target.transform.position - shaftTransform.position);
        shaftTransform.rotation = Quaternion.Slerp(shaftTransform.rotation, targetRotation,  Time.fixedDeltaTime * _barrelRotateSpeed * 2);

        if (TargetInRange(Target.transform.position, _shootRange))
        {
            if (NoObstacleBetween(Target.transform.position)) gameObject.SendMessage("BarrelShoot", 0);
            else gameObject.SendMessage("StopShoot", 0);
        }
    }

    private bool TargetInRange(Vector3 targetPosition, float range)
    {
        return ((targetPosition - transform.position).sqrMagnitude < range * range);
    }

    private bool NoObstacleBetween(Vector3 targetPosition)
    {
        return Physics.Linecast(transform.position, targetPosition, _floorLayer);
    }

    private void ContinueRevenge()
    {
        if (!TargetInRange(Target.transform.position, _seekRange))
        {
            _revengeMode = false;
            Target = null;
        }
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
        // Maybe player can revive with special effect
        Destroy(gameObject);
        Debug.Log("You are dead.");
    }
}

