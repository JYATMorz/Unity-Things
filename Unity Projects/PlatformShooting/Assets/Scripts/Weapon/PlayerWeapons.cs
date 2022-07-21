using UnityEngine;

public class PlayerWeapons : MonoBehaviour
{
    public Rigidbody ammoPrefab;

    private Rigidbody _barrelRotationCenter;
    private bool _isShot = false;

    void Awake()
    {
        _barrelRotationCenter = GetComponentsInChildren<Rigidbody>()[1];
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0) && !_isShot) BarrelShoot()
    
    }

    private void BarrelShoot()
    {
        Transform _barrelTransform = _barrelRotationCenter.transform;
        if (Vector3.Angle(Vector3.up, _barrelTransform.up) <= 135)
        {
            // create fog at barrel to hide distance between ammo

            Rigidbody newAmmo = Instantiate(ammoPrefab, 
                _barrelRotationCenter.position + _barrelTransform.up * 0.55f, _barrelRotationCenter.rotation, _barrelTransform);
            Ammo ammoScript = newAmmo.GetComponent<Ammo>();

            newAmmo.AddForce(_barrelTransform.up * ammoScript.ammoSpeed, ForceMode.VelocityChange);
            _isShot = true;
            Invoke("ResetShootInterval", ammoScript.ammoInterval);
        } else
        {
            Debug.Log("Shooting Dead Zone! Need More Notification Here!");
        }
    }

    private void ResetShootInterval()
    {
        _isShot = false;
    }
}