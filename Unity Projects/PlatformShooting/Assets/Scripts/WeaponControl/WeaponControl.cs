using UnityEngine;
using UnityEngine.VFX;
using System.Collections;
using AmmoType;

public class EnemyControl : MonoBehaviour {
    
    private AmmoType _commonBullet = new AmmoType(20, 0.5f, 0.5f);
    private AmmoType _laserBeam = new AmmoType(100, 1f, 0f);
    private AmmoType _grenadeLauncher = new AmmoType(10, 0.8f, 2f);
    private AmmoType _explosivePayload = new AmmoType(25, 0.5f, 1f);
    private Rigidbody _barrelShaft;
    private VisualEffect _shootSmoke;
    private bool _fireConfirm = false;

    public GameObject bulletPrefab;
    public GameObject laserPrefab;
    public GameObject grenadePrefab;
    public GameObject explosivePrefab;

    void Awake()
    {
        _barrelShaft = GetComponentsInChildren<Rigidbody>()[1];
        // TODO: Use different effect for each AmmoType
        _shootSmoke = GetComponentInChildren<VisualEffect>();

        // Assign different prefab to each weapon type
        _commonBullet.AmmoPrefab = bulletPrefab;
        _laserBeam.AmmoPrefab = laserPrefab;
        _grenadeLauncher.AmmoPrefab = grenadePrefab;
        _explosivePayload.AmmoPrefab = explosivePrefab;
    }

    private void BarrelShoot(string ammoType)
    {
        _fireConfirm = true;
        StartCoroutine(RepeatShooting(SwitchAmmo(ammoType)));
    }
    private void StopShoot()
    {
        _fireConfirm = false;
    }

    private void ShootOnce(AmmoType ammoType)
    {
        Transform _barrelTransform = _barrelShaft.transform;
        if (Vector3.Angle(Vector3.up, _barrelTransform.up) <= 135)
        {
            // Create fog at barrel to hide distance between ammo
            // TODO: It play at start, fix it
            // TODO: You can disable the play on awake by changing the "Initial Event Name" field to empty.
            _shootSmoke.Play();

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

    IEnumerator RepeatShooting(AmmoType ammoType)
    {
        while(_fireConfirm)
        {
            ShootOnce(ammoType);
            yield return new WaitForSeconds(ammoType.FireInterval);
        }
    }

    private AmmoType SwitchAmmo(string typeName)
    {
        switch (typeName)
        {
            case "LaserBeam": return _laserBeam;
            case "GrenadeLauncher" : return _grenadeLauncher;
            case "ExplosivePayload" : return _explosivePayload;
            default: return _commonBullet;
        }
    }
}