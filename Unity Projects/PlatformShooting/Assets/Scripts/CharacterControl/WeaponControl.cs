using UnityEngine;
using UnityEngine.VFX;
using System.Collections;
using AmmoType;

public class WeaponControl : MonoBehaviour {
    
    private AmmoData _commonBullet = new(20, 0.6f, 0.5f);
    private AmmoData _laserBeam = new(100, 1f, 0f);
    private AmmoData _grenadeLauncher = new(10, 1.2f, 2f);
    private AmmoData _explosivePayload = new(25, 0.8f, 1f);
    private Rigidbody _barrelShaft;
    private Transform _barrelTransform;
    // private VisualEffect _shootSmoke;
    private bool _fireConfirm = false;
    private bool _isFiring = false;

    [Header("In Game UI")]
    public GameMenu gameMenu;
    [Header("Ammo Prefabs")]
    // FIXME: use dictionary?
    public Rigidbody bulletPrefab;
    public Rigidbody laserPrefab;
    public Rigidbody grenadePrefab;
    public Rigidbody explosivePrefab;

    // TODO: shoot effect: gameObject / VFX
    // [Header("Visual Effect Prefabs")]

    void Awake()
    {
        _barrelShaft = GetComponentsInChildren<Rigidbody>()[1];
        _barrelTransform = _barrelShaft.transform;
        // TODO: Use different effect for each AmmoType, Use name to distinguish laser from others
        // _shootSmoke = GetComponentInChildren<VisualEffect>();
        // TODO: Charge effect for laser beam

        // Assign different prefab to each weapon type
        _commonBullet.AmmoPrefab = bulletPrefab;
        _laserBeam.AmmoPrefab = laserPrefab;
        _grenadeLauncher.AmmoPrefab = grenadePrefab;
        _explosivePayload.AmmoPrefab = explosivePrefab;
    }

    public void BarrelShoot(string ammoType)
    {
        if (!_fireConfirm && !_isFiring)
        {
            _fireConfirm = true;
            StartCoroutine(RepeatShooting(SwitchAmmo(ammoType)));
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
            // _shootSmoke.Play();

            // Prepare a Selected Ammo to shoot.
            Rigidbody newAmmo = Instantiate(ammoType.AmmoPrefab, 
                _barrelShaft.position + _barrelTransform.up * 0.55f, _barrelShaft.rotation, _barrelTransform);

            // Add randomness when setting the launch angle
            Quaternion angleRandomness = Quaternion.Euler(0, 0, Random.Range(-ammoType.AmmoSpread, ammoType.AmmoSpread));

            newAmmo.AddForce(angleRandomness * _barrelTransform.up * ammoType.AmmoSpeed, ForceMode.VelocityChange);
        } else
        {
            if (!MainCamera.IsGameOver) gameMenu.ShowNotification("DeadZone");
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

    private AmmoData SwitchAmmo(string typeName) => typeName switch
    {
        "LaserBeam" => _laserBeam,
        "GrenadeLauncher" => _grenadeLauncher,
        "ExplosivePayload" => _explosivePayload,
        _ => _commonBullet,
    };

    // FIXME: parabola
    // https://physics.stackexchange.com/questions/56265/how-to-get-the-angle-needed-for-a-projectile-to-pass-through-a-given-point-for-t/70480#70480
    private Quaternion CalculateElevationAngle(Vector3 targetPosition, int speed)
    {
        Vector3 deltaPos = targetPosition - _barrelShaft.position;

        float deltaX = deltaPos.x;
        float deltaY = deltaPos.y;
        float sign = Mathf.Sign(deltaPos.x * deltaPos.y);
        float gravity = Physics.gravity.y;

        float tanAngle = sign * speed * speed / gravity / deltaX
            - Mathf.Sqrt((Mathf.Pow(speed, 4) - 2 * speed * speed * gravity * deltaY) / (gravity * gravity * deltaX * deltaX) - 1);
        Quaternion rotation = Quaternion.Euler(0, 0, Mathf.Atan(tanAngle) * Mathf.Rad2Deg - 90);

        return rotation;
    }
}