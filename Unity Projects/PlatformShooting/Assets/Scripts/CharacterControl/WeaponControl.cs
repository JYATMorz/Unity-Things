using UnityEngine;
using UnityEngine.VFX;
using System;
using System.Collections;
using System.Collections.Generic;

public class WeaponControl : MonoBehaviour
{
    private readonly Dictionary<string, AmmoData> _ammoInfos = new();
    private readonly string[] _ammoTypes = { 
        ConstantSettings.commonTag, ConstantSettings.laserTag,
        ConstantSettings.grenadeTag, ConstantSettings.explosiveTag
    };

    private IMenuUI _gameMenu;
    private CharacterControl _characterControl;
    private TargetControl _targetControl;
    private Rigidbody _barrelShaft;
    private Transform _barrelTransform;
    private Quaternion _deltaRotation;
    private Vector3 _pointerDirection;
    private bool _fireConfirm = false;
    private bool _isFiring = false;
    private float _rotateSpeed = ConstantSettings.barrelRotateSpeed;
    private int _ammoTypeNum = 0;

    private AmmoData _commonBullet = new(ConstantSettings.commonTag, 20, 0.6f, true);
    private AmmoData _laserBeam = new(ConstantSettings.laserTag, 100, 1f, false, true);
    private AmmoData _grenadeLauncher = new(ConstantSettings.grenadeTag, 10, 1.2f, true);
    private AmmoData _explosivePayload = new(ConstantSettings.explosiveTag, 15, 0.8f);

    public bool IsBarrelIdle { get; set; } = false;
    public int AvoidLayer { get; set; } = -1;
    public AmmoData CurrentAmmo { get; private set; }

    [Header("In Game Components")]
    public GameObject sceneMenu;
    public Camera mainCamera;
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
        _characterControl = GetComponent<CharacterControl>();
        _targetControl = GetComponent<TargetControl>();

        _gameMenu = sceneMenu.GetComponent<IMenuUI>();
        _barrelShaft = GetComponentsInChildren<Rigidbody>()[1];
        _barrelTransform = _barrelShaft.transform;
        
        AvoidLayer = LayerMask.GetMask("Floor", "Elevator");

        // Assign different prefab to each weapon type
        AddAmmoInfo(ref _commonBullet, ref bulletPrefab, ref bulletSmoke);
        AddAmmoInfo(ref _laserBeam, ref laserPrefab, ref laserCharge);
        AddAmmoInfo(ref _grenadeLauncher, ref grenadePrefab, ref grenadeSmoke);
        AddAmmoInfo(ref _explosivePayload, ref explosivePrefab, ref explosiveSmoke);

    }

    void Start()
    {
        SwitchCurrentAmmo(UnityEngine.Random.Range(0, _ammoTypes.Length));

        StartCoroutine(AutoBarrel());
    }

    void FixedUpdate()
    {
        if (_characterControl.IsPlayer)
        {
            // Rotate the barrel to point at the mouse position
            _pointerDirection = Input.mousePosition - mainCamera.WorldToScreenPoint(_barrelShaft.position);
            _pointerDirection.z = 0;
            _barrelShaft.rotation = Quaternion.FromToRotation(Vector3.up, _pointerDirection);
        }
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
        if (GeneralLoadMenu.Instance.IsLoadingScene) return;

        Rigidbody newAmmo = Instantiate(ammoType.AmmoPrefab, 
            _barrelShaft.position + _barrelTransform.up * 0.55f, _barrelShaft.rotation, _barrelTransform);

        newAmmo.AddForce(_barrelTransform.up * ammoType.AmmoSpeed, ForceMode.VelocityChange);
    }

    IEnumerator RepeatShooting(AmmoData ammoType)
    {
        while(_fireConfirm)
        {
            _isFiring = true;

            if (Vector3.Angle(Vector3.up, _barrelTransform.up) > 135)
            {
                if (_characterControl.IsPlayer && !MainCamera.IsGameOver) _gameMenu.ShowNotification("DeadZone");
                yield return new WaitForSeconds(ammoType.FireInterval);
                continue;
            }

            ammoType.ShootEffect.Play();
            GeneralAudioControl.Instance.PlayAudio(CurrentAmmo.Tag, transform.position);

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
        while (!_characterControl.IsPlayer)
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
        if (_targetControl.TargetCharacter == null || _characterControl.IsPlayer)
        {
            StopShoot();
            return;
        }

        Quaternion targetRotation =
            CurrentAmmo.IsParabola
            ? ParabolaAim(_targetControl.TargetCharacter.transform.position, CurrentAmmo.AmmoSpeed)
            : DirectAim(_targetControl.TargetCharacter.transform.position);
        _barrelShaft.rotation =
            Quaternion.RotateTowards(
                _barrelShaft.rotation, targetRotation,
                3 * ConstantSettings.barrelRotateSpeed * Time.fixedDeltaTime
            );

        if (!Physics.Linecast(_barrelShaft.position, _targetControl.TargetPosition, AvoidLayer)
            && ConstantSettings.TargetInRange(_targetControl.TargetCharacter.transform.position, transform.position, ConstantSettings.shootRange))
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
        float angleScalar = deltaY switch { < -0.5f => 1.4f, > 0.5f => 0.85f, _ => 1.1f };

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

    public void ResetWeaponStatus()
    {
        _isFiring = false;
        _fireConfirm = false;
    }

    private void SwitchCurrentAmmo(int ammoNum)
    {
        if (_characterControl.IsPlayer) _gameMenu.SwitchWeaponIcon(ammoNum);
        CurrentAmmo = _ammoInfos[_ammoTypes[ammoNum]];
    }

    public int CurrentAmmoNo() => Array.IndexOf(_ammoTypes, CurrentAmmo.Tag);

    private void AddAmmoInfo(ref AmmoData ammoData, ref Rigidbody ammoPrefab, ref VisualEffect ammoVFX)
    {
        ammoData.AmmoPrefab = ammoPrefab;
        ammoData.ShootEffect = ammoVFX;
        _ammoInfos.Add(ammoData.Tag, ammoData);
    }
}