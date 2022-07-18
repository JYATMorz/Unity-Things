using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Rigidbody playerBody;
    public float jumpAcc = 10f;

    // Start is called before the first frame update
    void Start()
    {
        playerBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space Key Pressed");
            playerBody.AddForce(Vector3.up * jumpAcc, ForceMode.Impulse);
        }
    }

    void FixedUpdate() {
        
    }
}
