using UnityEngine;
using UnityEngine.VFX;
using System.Collections;
using System.Collections.Generic;

public class WeaponControl : MonoBehaviour {
    private const int _barrelRotateSpeed = 45;
    [HideInInspector] public static readonly float ShootRange = 10f;

    private readonly Dictionary<string, AmmoData> _ammoInfos = new();
    private readonly string[] _ammoTypes =
        { "CommonBullet", "LaserBeam", "GrenadeLauncher", "ExplosivePayload" };

    private AmmoData _commonBullet = new("CommonBullet", 20, 0.6f, true, true);
    private AmmoData _laserBeam = new("LaserBeam", 100, 1f, false, true);
    private AmmoData _grenadeLauncher = new("GrenadeLauncher", 10, 1.2f, true);
    private AmmoData _explosivePayload = new("ExplosivePayload", 25, 0.8f);
    private Rigidbody _barrelShaft;
    private Transform _barrelTransform;
    private Quaternion _deltaRotation;
    private bool _fireConfirm = false;
    private bool _isFiring = false;
    private float _rotateSpeed = _barrelRotateSpeed;
    private int _ammoTypeNum = 0;

    public bool IsBarrelIdle { get; set; } = false;
    public bool IsPlayer { get; set; } = false;
    public int AvoidLayer { get; set; } = -1;
    public AmmoData CurrentAmmo { get; private set; }
    public Vector3 TargetPosition { get; set; }

    [Header("In Game UI")]
    public GameMenu gameMenu;
    [Header("Ammo Prefabs & VFX")]
    public Rigidbody bulletPrefab;
    public VisualEffect bulletSmoke;
    [Space]
    public Rigidbody laserPrefab;
    public VisualEffect laserCharge;
    [Space]
    public Rigidbody grenadePrefab;
    public VisualEffect grenadeSmoke;
    [Space]
    public Rigidbody explosivePrefab;
    public VisualEffect explosiveSmoke;

    void Awake()
    {
        _barrelShaft = GetComponentsInChildren<Rigidbody>()[1];
        _barrelTransform = _barrelShaft.transform;
        
        AvoidLayer = LayerMask.GetMask("Floor", "Elevator");

        // Assign different prefab to each weapon type
        _commonBullet.AmmoPrefab = bulletPrefab;
        _commonBullet.ShootEffect = bulletSmoke;
        _laserBeam.AmmoPrefab = laserPrefab;
        _laserBeam.ShootEffect = laserCharge;
        _grenadeLauncher.AmmoPrefab = grenadePrefab;
        _grenadeLauncher.ShootEffect = grenadeSmoke;
        _explosivePayload.AmmoPrefab = explosivePrefab;
        _explosivePayload.ShootEffect = explosiveSmoke;
        
        _ammoInfos.Add(_commonBullet.Tag, _commonBullet);
        _ammoInfos.Add(_laserBeam.Tag, _laserBeam);
        _ammoInfos.Add(_grenadeLauncher.Tag, _grenadeLauncher);
        _ammoInfos.Add(_explosivePayload.Tag, _explosivePayload);
    }

    void Start()
    {
        SwitchCurrentAmmo(Random.Range(0, _ammoTypes.Length));

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
            // Prepare a Selected Ammo to shoot.
            Rigidbody newAmmo = Instantiate(ammoType.AmmoPrefab, 
                _barrelShaft.position + _barrelTransform.up * 0.55f, _barrelShaft.rotation, _barrelTransform);

            newAmmo.AddForce(_barrelTransform.up * ammoType.AmmoSpeed, ForceMode.VelocityChange);
        } else
        {
            if (IsPlayer && !MainCamera.IsGameOver) gameMenu.ShowNotification("DeadZone");
        }
    }

    IEnumerator RepeatShooting(AmmoData ammoType)
    {
        while(_fireConfirm)
        {
            _isFiring = true;
            ammoType.ShootEffect.Play();

            if (ammoType.RequireCharge)
            {
                yield return new WaitForSeconds(ammoType.FireInterval * 0.25f);
                ShootOnce(ammoType);
                yield return new WaitForSeconds(ammoType.FireInterval * 0.75f);
            } else
            {
                yield return new WaitForSeconds(ammoType.FireInterval * 0.1f);
                ShootOnce(ammoType);
                yield return new WaitForSeconds(ammoType.FireInterval * 0.9f);
            }
        }
        _isFiring = false;
    }

    IEnumerator AutoBarrel()
    {
        while (!IsPlayer)
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
        /*
        if ((TargetPosition - transform.position).sqrMagnitude > 100)
        {
            StopShoot();
            return;
        }*/
        Quaternion targetRotation =
            CurrentAmmo.IsParabola ? ParabolaAim(TargetPosition, CurrentAmmo.AmmoSpeed) : DirectAim(TargetPosition);
        _barrelShaft.rotation =
            Quaternion.RotateTowards(_barrelShaft.rotation, targetRotation, 3 * _barrelRotateSpeed * Time.fixedDeltaTime);

        if (!Physics.Linecast(_barrelShaft.position, TargetPosition, AvoidLayer)
            && (TargetPosition - transform.position).sqrMagnitude < ShootRange * ShootRange)
            BarrelShoot();
        else
            StopShoot();
    }

    private Quaternion DirectAim(Vector3 targetPos)
    {
        return Quaternion.FromToRotation(Vector3.up, (targetPos - _barrelShaft.position).normalized);
    }

    // https://physics.stackexchange.com/questions/56265/how-to-get-the-angle-needed-for-a-projectile-to-pass-through-a-given-point-for-t/70480#70480
    private Quaternion ParabolaAim(Vector3 targetPos, int speed)
    {
        Vector3 deltaPos = targetPos - _barrelShaft.position;

        float deltaX = deltaPos.x;
        float deltaY = deltaPos.y;
        float gravity = Physics.gravity.y;
        float angleScalar = deltaY switch { < -0.5f => 1.4f, > 0.5f => 0.9f, _ => 1.1f };

        float tanAngleLeft = speed * speed / gravity / Mathf.Abs(deltaX);
        float tanAngleRightSqr = (Mathf.Pow(speed, 4) - 2 * speed * speed * gravity * deltaY) / (gravity * gravity * deltaX * deltaX) - 1;
        float tanAngle = (tanAngleRightSqr < 0) ? Mathf.Sign(tanAngleLeft) : (tanAngleLeft - Mathf.Sqrt(tanAngleRightSqr));

        float actualAngle = Mathf.Sign(deltaX) * Mathf.Atan(tanAngle) * Mathf.Rad2Deg * angleScalar;
        Quaternion rotation = Quaternion.Euler(0, 0, actualAngle);

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