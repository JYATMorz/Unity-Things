using UnityEngine;

public class EnemyWeapons : MonoBehaviour {
    
    public Rigidbody ammoPrefab;

    private const int _ammoSpeed = 20;
    private const float _ammoInterval = 0.4f;

    private GameObject _barrelShaft;

    void Awake()
    {
        _barrelShaft = GetComponentInChildren<GameObject>();
    }

    private void BarrelShoot(int weaponType) // TODO: use WeaponType to decide ammo/weapon type
    {
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
            // create fog at barrel to hide distance between ammo

            Rigidbody newAmmo = Instantiate(ammoPrefab, 
                _barrelTransform.position + _barrelTransform.up * 0.55f, _barrelTransform.rotation, _barrelTransform);

            newAmmo.AddForce(_barrelTransform.up * _ammoSpeed, ForceMode.VelocityChange);
        } else
        {
            Debug.Log("Shooting Dead Zone! Need More Notification Here!");
        }
    }
}