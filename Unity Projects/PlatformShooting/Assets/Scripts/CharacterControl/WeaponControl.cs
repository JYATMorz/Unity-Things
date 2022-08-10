using UnityEngine;
using UnityEngine.VFX;
using System.Collections;
using System.Collections.Generic;

public class WeaponControl : MonoBehaviour {
    private const int _barrelRotateSpeed = 45;

    private readonly Dictionary<string, AmmoData> _ammoInfos = new();
    private readonly string[] _ammoTypes =
        { "CommonBullet", "LaserBeam", "GrenadeLauncher", "ExplosivePayload" };

    private AmmoData _commonBullet = new("CommonBullet", 20, 0.6f, true);
    private AmmoData _laserBeam = new("LaserBeam", 100, 1f);
    private AmmoData _grenadeLauncher = new("GrenadeLauncher", 10, 1.2f, true);
    private AmmoData _explosivePayload = new("ExplosivePayload", 25, 0.8f);
    private Rigidbody _barrelShaft;
    private Transform _barrelTransform;
    private Quaternion _deltaRotation;
    // private VisualEffect _shootSmoke;
    private bool _fireConfirm = false;
    private bool _isFiring = false;
    private float _rotateSpeed = _barrelRotateSpeed;
    private int _ammoTypeNum = 0;

    public bool IsBarrelIdle { get; set; } = false;
    public int AvoidLayer { get; set; } = -1;
    public AmmoData CurrentAmmo { get; private set; }
    public Vector3 TargetPosition { get; set; }

    [Header("In Game UI")]
    public GameMenu gameMenu;
    [Header("Ammo Prefabs & VFX")]
    public Rigidbody bulletPrefab;
    // TODO: shoot effect: gameObject / VFX
    [Space]
    public Rigidbody laserPrefab;
    [Space]
    public Rigidbody grenadePrefab;
    [Space]
    public Rigidbody explosivePrefab;

    void Awake()
    {
        _barrelShaft = GetComponentsInChildren<Rigidbody>()[1];
        _barrelTransform = _barrelShaft.transform;
        
        AvoidLayer = LayerMask.GetMask("Floor", "Elevator");

        // Assign different prefab to each weapon type
        _commonBullet.AmmoPrefab = bulletPrefab;
        // TODO: _commonBullet.ShootEffect = ;
        _laserBeam.AmmoPrefab = laserPrefab;
        _grenadeLauncher.AmmoPrefab = grenadePrefab;
        _explosivePayload.AmmoPrefab = explosivePrefab;
        
        _ammoInfos.Add(_commonBullet.Tag, _commonBullet);
        _ammoInfos.Add(_laserBeam.Tag, _laserBeam);
        _ammoInfos.Add(_grenadeLauncher.Tag, _grenadeLauncher);
        _ammoInfos.Add(_explosivePayload.Tag, _explosivePayload);
    }

    void Start()
    {
        SwitchCurrentAmmo(Random.Range(0, _ammoTypes.Length));
    }

    public void StartNPC()
    {
        StartCoroutine(AutoBarrel());
    }

    public void BarrelShoot()
    {
        if (!_fireConfirm && !_isFiring)
        {
            _fireConfirm = true;
            StartCoroutine(RepeatShooting(CurrentAmmo));
        }
    }

    public void StopShoot()
    {
        if (_isFiring)
        {
            _fireConfirm = false;
        }
    }

    private void ShootOnce(AmmoData ammoType)
    {
        if (Vector3.Angle(Vector3.up, _barrelTransform.up) <= 135)
        {
            // TODO: Create fog at barrel to hide distance between ammo
            // CurrentAmmo.ShootEffect.Play();

            // Prepare a Selected Ammo to shoot.
            Rigidbody newAmmo = Instantiate(ammoType.AmmoPrefab, 
                _barrelShaft.position + _barrelTransform.up * 0.55f, _barrelShaft.rotation, _barrelTransform);

            newAmmo.AddForce(_barrelTransform.up * ammoType.AmmoSpeed, ForceMode.VelocityChange);
        } else
        {
            // FIXME:if (!MainCamera.IsGameOver) gameMenu.ShowNotification("DeadZone");
            // only player's notification
        }
    }

    IEnumerator RepeatShooting(AmmoData ammoType)
    {
        while(_fireConfirm)
        {
            _isFiring = true;
            ShootOnce(ammoType);
            yield return new WaitForSeconds(ammoType.FireInterval);
        }
        _isFiring = false;
    }

    IEnumerator AutoBarrel()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();

            if (IsBarrelIdle) BarrelIdle();
            else BarrelAim();
        }
    }

    private void BarrelIdle()
    {
        StopShoot();

        if (Quaternion.Angle(Quaternion.identity, _barrelShaft.rotation) > 120) _rotateSpeed *= -1;

        _deltaRotation = Quaternion.Euler(0, 0, _rotateSpeed * Time.fixedDeltaTime);
        _barrelShaft.MoveRotation(_barrelShaft.rotation * _deltaRotation);
    }

    private void BarrelAim()
    {
        Quaternion targetRotation =
            // FIXME: CurrentAmmo.IsParabola ? ParabolaAim(TargetPosition, CurrentAmmo.AmmoSpeed) : DirectAim(TargetPosition);
            CurrentAmmo.IsParabola ? DirectAim(TargetPosition) : DirectAim(TargetPosition);
        _barrelShaft.rotation =
            Quaternion.RotateTowards(_barrelShaft.rotation, targetRotation, 3 * _barrelRotateSpeed * Time.fixedDeltaTime);

        if (!Physics.Linecast(_barrelShaft.position, TargetPosition, AvoidLayer))
            BarrelShoot();
        else
            StopShoot();
    }

    private Quaternion DirectAim(Vector3 targetPos)
    {
        return Quaternion.FromToRotation(Vector3.up, (targetPos - _barrelShaft.position).normalized);
    }

    // BUG: parabola
    // https://physics.stackexchange.com/questions/56265/how-to-get-the-angle-needed-for-a-projectile-to-pass-through-a-given-point-for-t/70480#70480
    private Quaternion ParabolaAim(Vector3 targetPos, int speed)
    {
        Vector3 deltaPos = targetPos - _barrelShaft.position;

        float deltaX = deltaPos.x;
        float deltaY = deltaPos.y;
        float sign = Mathf.Sign(deltaPos.x * deltaPos.y);
        float gravity = Physics.gravity.y;

        float tanAngle = sign * speed * speed / gravity / deltaX
            - Mathf.Sqrt((Mathf.Pow(speed, 4) - 2 * speed * speed * gravity * deltaY) / (gravity * gravity * deltaX * deltaX) - 1);
        Quaternion rotation = Quaternion.Euler(0, 0, Mathf.Atan(tanAngle) * Mathf.Rad2Deg);

        return rotation.normalized;
    }

    public void ChangeWeapon(int num, bool isScroll = false)
    {
        if (isScroll)
        {
            int temp = (_ammoTypeNum - num) % 4;
            _ammoTypeNum = (temp < 0) ? temp + _ammoTypes.Length : temp;
        } else _ammoTypeNum = num;

        SwitchCurrentAmmo(_ammoTypeNum);
    }

    private void SwitchCurrentAmmo(int ammoNum)
    {
        gameMenu.SwitchWeaponIcon(ammoNum);
        CurrentAmmo = _ammoInfos[_ammoTypes[ammoNum]];
    }
}