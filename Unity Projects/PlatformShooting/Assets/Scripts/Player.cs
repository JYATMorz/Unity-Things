using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    private Rigidbody _playerBody;
    private float _speedScaler = 5f;
    private bool _jumpPressed = false;
    private bool _isGrounded = true;
    private int _doubleJump = 2;
    private float _horizontalInput;

    // Start is called before the first frame update
    void Start()
    {
        _playerBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && (_isGrounded || (_doubleJump > 0)))
        {
            _jumpPressed = true;
        }

        _horizontalInput = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        if (_jumpPressed)
        {
            _playerBody.AddForce(Vector3.up * _speedScaler, ForceMode.VelocityChange);

            _doubleJump --;
            _jumpPressed = false;
        }

        _playerBody.velocity = new Vector3(_horizontalInput * _speedScaler, _playerBody.velocity.y, 0);
    }

    void OnCollisionEnter(Collision other) {
        _isGrounded = true;
        _doubleJump = 2;

        // if (_playerBody.velocity.y > 10) Debug.Log($"Vertical Velocity: {_playerBody.velocity.y}");
    }

    void OnCollisionExit(Collision other) {
        _isGrounded = false;
    }
}
