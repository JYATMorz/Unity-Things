using UnityEngine;
using UnityEngine.VFX;

public class EnemyWeapons : MonoBehaviour {
    
    public Rigidbody ammoPrefab;

    private const int _ammoSpeed = 20;
    private const float _ammoInterval = 0.5f;

    private Rigidbody _barrelShaft;
    private VisualEffect _shootSmoke;

    void Awake()
    {
        _barrelShaft = GetComponentsInChildren<Rigidbody>()[1];
        _shootSmoke = GetComponentInChildren<VisualEffect>();
    }

    private void BarrelShoot(string weaponType)
    {
        // TODO: use WeaponType to decide ammo/weapon type

        // TODO: If is necessary to switch to coroutine
        if (!IsInvoking("RepeatShooting")) InvokeRepeating("RepeatShooting", _ammoInterval, _ammoInterval);
    }
    private void StopShoot()
    {
        if (IsInvoking("RepeatShooting")) CancelInvoke("RepeatShooting");
    }

    private void RepeatShooting()
    {
        Transform _barrelTransform = _barrelShaft.transform;
        if (Vector3.Angle(Vector3.up, _barrelTransform.up) <= 135)
        {
            // TODO: create fog at barrel to hide distance between ammo
            // TODO: It play at start, fix it
            _shootSmoke.Play();
            // TODO: Add randomness when setting the launch angle
            Rigidbody newAmmo = Instantiate(ammoPrefab, 
                _barrelShaft.position + _barrelTransform.up * 0.55f, _barrelShaft.rotation, _barrelTransform);
            newAmmo.AddForce(_barrelTransform.up * _ammoSpeed, ForceMode.VelocityChange);
        } else
        {
            // TODO: Add notification/sound effect when in wrong angle
            Debug.Log("Shooting Dead Zone! Need More Notification Here!");
        }
    }
}