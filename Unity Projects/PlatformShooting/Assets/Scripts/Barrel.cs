using UnityEngine;

public class Barrel : MonoBehaviour
{
    private readonly float _bulletSpeed = 20f;
    private readonly float _shootInterval = 0.2f;

    public Camera _mainCamera;
    public Rigidbody _ammoPrefab;

    private Rigidbody _barrelRotationCenter;
    private Vector3 _rotateVector;
    private bool _isShot = false;

    void Start() {
        _barrelRotationCenter = GetComponent<Rigidbody>();
    }

    void Update() {
        if (Input.GetKey(KeyCode.Mouse0) && !_isShot)
        {
            if (Vector3.Angle(Vector3.up, transform.up) <= 135)
            {
                // create fog at barrel to hide distance between ammo

                Rigidbody newAmmo = Instantiate(_ammoPrefab, _barrelRotationCenter.position + transform.up * 0.55f, _barrelRotationCenter.rotation, transform);
                newAmmo.AddForce(transform.up * _bulletSpeed, ForceMode.VelocityChange);
                _isShot = true;
                Invoke("ResetShootInterval", _shootInterval);
            } else
            {
                Debug.Log("Shooting Dead Zone! Need More Notification Here!");
            }
        }
    }

    void FixedUpdate()
    {
        // Rotate the barrel to point at the mouse position
        _rotateVector = Input.mousePosition - _mainCamera.WorldToScreenPoint(_barrelRotationCenter.position);
        _rotateVector.z = 0;
        _barrelRotationCenter.rotation *= Quaternion.FromToRotation(transform.up, _rotateVector);
    }

    private void ResetShootInterval()
    {
        _isShot = false;
    }
}
