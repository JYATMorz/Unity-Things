using UnityEngine;

public class Player : MonoBehaviour
{
    private readonly float _speedScaler = 5f;
    private readonly int _initHealth = 100;

    public HealthBar healthBar;

    private Rigidbody _playerBody;
    private bool _jumpPressed = false;
    private bool _isGrounded = true;
    private int _doubleJump = 2;
    private int _currentHealth;
    private float _xAxisInputScaler;
    

    // Start is called before the first frame update
    void Start()
    {
        _currentHealth = _initHealth;
        healthBar.SetMaxHealth(_initHealth);
        _playerBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
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

    void OnCollisionEnter(Collision other) {
        _isGrounded = true;
        _doubleJump = 2;

        /*
        foreach (ContactPoint contact in other.contacts)
        {
            print(contact.thisCollider.name + " hit " + contact.otherCollider.name);
        }
        */
    }

    void OnCollisionExit(Collision other) {
        _isGrounded = false;
    }

    private void ReceiveDamage(int damage)
    {
        _currentHealth -= damage;
        healthBar.SetHealthValue(_currentHealth);

        if (_currentHealth <= 0)
        {
            ZeroHealth();
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
