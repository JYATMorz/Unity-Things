using UnityEngine;

public class Player : MonoBehaviour
{
    private readonly float _speedScaler = 5f;
    private readonly int _initHealth = 100;
    private readonly string _bulletTag = "Bullet";
    private readonly string _floorTag = "Floor";

    public HealthBar healthBar;

    private Rigidbody _playerBody;
    private bool _jumpPressed = false;
    private bool _isGrounded = true;
    private int _doubleJump = 2;
    private int _currentHealth;
    private float _xAxisInputScaler;
    

    void Start()
    {
        _currentHealth = _initHealth;
        healthBar.SetMaxHealth(_initHealth);
        _playerBody = GetComponent<Rigidbody>();
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
        
    }

    void FixedUpdate()
    {
        if (_jumpPressed)
        {
            _playerBody.AddForce(Vector3.up * _speedScaler, ForceMode.Impulse);

            _doubleJump --;
            _jumpPressed = false;
        }

        _playerBody.velocity = new Vector3(_xAxisInputScaler * _speedScaler, _playerBody.velocity.y, 0);
    }

    void OnCollisionEnter(Collision other) 
    {
        GameObject collideObj = other.gameObject;

        switch (collideObj.tag)
        {
            case _bulletTag:
                ReceiveDamage(15);
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
}
