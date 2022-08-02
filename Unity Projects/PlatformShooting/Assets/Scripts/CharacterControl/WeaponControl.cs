using UnityEngine;
using UnityEngine.VFX;
using System.Collections;
using AmmoType;

public class WeaponControl : MonoBehaviour {
    
    private AmmoData _commonBullet = new(20, 0.5f, 0.5f);
    private AmmoData _laserBeam = new(100, 1f, 0f);
    private AmmoData _grenadeLauncher = new(10, 0.8f, 2f);
    private AmmoData _explosivePayload = new(25, 0.5f, 1f);
    private Rigidbody _barrelShaft;
    // private VisualEffect _shootSmoke;
    private bool _fireConfirm = false;
    private bool _isFiring = false;

    public Rigidbody bulletPrefab;
    public Rigidbody laserPrefab;
    public Rigidbody grenadePrefab;
    public Rigidbody explosivePrefab;

    void Awake()
    {
        _barrelShaft = GetComponentsInChildren<Rigidbody>()[1];
        // TODO: Use different effect for each AmmoType, Use name to distinguish laser from others
        // _shootSmoke = GetComponentInChildren<VisualEffect>();

        // Assign different prefab to each weapon type
        _commonBullet.AmmoPrefab = bulletPrefab;
        _laserBeam.AmmoPrefab = laserPrefab;
        _grenadeLauncher.AmmoPrefab = grenadePrefab;
        _explosivePayload.AmmoPrefab = explosivePrefab;
    }

    private void BarrelShoot(string ammoType)
    {
        if (!_fireConfirm && !_isFiring)
        {
            _fireConfirm = true;
            StartCoroutine(RepeatShooting(SwitchAmmo(ammoType)));
        }
    }
    private void StopShoot()
    {
        if (_isFiring)
        {
            _fireConfirm = false;
            // StopAllCoroutines();
        }
        
    }

    private void ShootOnce(AmmoData ammoType)
    {
        Transform _barrelTransform = _barrelShaft.transform;
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
            // TODO: Add notification/sound effect when in wrong angle
            Debug.Log($"Shooting {ammoType} in Dead Zone!");
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
}