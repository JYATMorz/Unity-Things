using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float jumpAcc = 10f;
    public float moveSpd = 5f;

    private Rigidbody _playerBody;
    private bool _jumpPressed = false;
    private float _horizontalInput;
    private int _doubleJump = 2;

    // Start is called before the first frame update
    void Start()
    {
        _playerBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _jumpPressed = true;
            _doubleJump--;
        }

        _horizontalInput = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        if (_jumpPressed && (_doubleJump > 0))
        {
            _playerBody.AddForce(Vector3.up * jumpAcc, ForceMode.Impulse);

            _jumpPressed = false;
        }

        _playerBody.AddForce(_horizontalInput * moveSpd, 0, 0, ForceMode.VelocityChange);

    }

}
