using UnityEngine;

public class Character : MonoBehaviour
{
    private readonly float _speedScaler = 5f;
    private readonly float _bulletSpeed = 20f;
    private readonly float _shootInterval = 0.2f;
    private readonly int _initHealth = 100;
    private readonly string _bulletTag = "Bullet";
    private readonly string _floorTag = "Floor";

    public HealthBar healthBar;
    public Camera mainCamera;
    public Rigidbody ammoPrefab;

    private Rigidbody _characterBody;
    private Rigidbody _barrelRotationCenter;
    private Vector3 _rotateVector;
    private bool _isShot = false;
    private bool _jumpPressed = false;
    private bool _isGrounded = true;
    private int _doubleJump = 2;
    private int _currentHealth;
    private float _xAxisInputScaler;
    

    void Start()
    {
        _currentHealth = _initHealth;
        healthBar.SetMaxHealth(_initHealth);
        _characterBody = GetComponent<Rigidbody>();
        _barrelRotationCenter = GetComponentInChildren<Rigidbody>()
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && (_isGrounded || (_doubleJump > 0)))
        {
            _jumpPressed = true;
        }

        _xAxisInputScaler = Input.GetAxis("Horizontal");

        // Health Bar Check, should replace it with message receiver
        if (Input.GetKeyDown(KeyCode.F))
        {
            ReceiveDamage(10);
        }

        if (Input.GetKey(KeyCode.Mouse0) && !_isShot)
        {
            Transform _barrelTransform = _barrelRotationCenter.transform;
            if (Vector3.Angle(Vector3.up, _barrelTransform.up) <= 135)
            {
                // create fog at barrel to hide distance between ammo

                Rigidbody newAmmo = Instantiate(ammoPrefab, _barrelRotationCenter.position + _barrelTransform.up * 0.55f, _barrelRotationCenter.rotation, _barrelTransform);
                newAmmo.AddForce(_barrelTransform.up * _bulletSpeed, ForceMode.VelocityChange);
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
        if (_jumpPressed)
        {
            _characterBody.AddForce(Vector3.up * _speedScaler, ForceMode.Impulse);

            _doubleJump --;
            _jumpPressed = false;
        }

        _characterBody.velocity = new Vector3(_xAxisInputScaler * _speedScaler, _characterBody.velocity.y, 0);

        // Rotate the barrel to point at the mouse position
        _rotateVector = Input.mousePosition - mainCamera.WorldToScreenPoint(_barrelRotationCenter.position);
        _rotateVector.z = 0;
        _barrelRotationCenter.rotation *= Quaternion.FromToRotation(_barrelRotationCenter.transform.up, _rotateVector);
    }

    void OnCollisionEnter(Collision other) 
    {
        GameObject collideObj = other.gameObject;

        switch (collideObj.tag)
        {
            case _bulletTag:
                Ammo ammoScript = collideObj.GetComponent<Ammo>();
                ReceiveDamage(ammoScript.ammoDamage);
                break;
            case _floorTag:
                TouchGround();
                break;
            default:
                TouchGround();
                break;
        }
        Debug.Log(other.contactCount);
    }

    void OnCollisionExit(Collision other)
    {
        _isGrounded = false;
    }

    private void TouchGround()
    {
        _isGrounded = true;
        _doubleJump = 2;
    }

    private void ReceiveDamage(int damage)
    {
        _currentHealth -= damage;

        if (_currentHealth <= 0)
        {
            healthBar.SetHealthValue(0)
            ZeroHealth();
        } else
        {
            healthBar.SetHealthValue(_currentHealth);
        }
    }

    private void ZeroHealth()
    {
        // Maybe player can revive with special effect

        // !!! Move Camera Outside the Player before try to kill player !!!
        // Destroy(gameObject);
        Debug.Log("You are dead.");
    }

    private void ResetShootInterval()
    {
        _isShot = false;
    }
}
