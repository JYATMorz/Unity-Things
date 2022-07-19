using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrel : MonoBehaviour
{
    public Camera _mainCamera;

    private Rigidbody _barrelRotationCenter;
    private Vector3 _rotateVector;

    void Start() {
        _barrelRotationCenter = GetComponent<Rigidbody>();
        // _mainCamera = GetComponent<Camera>();
    }

    void FixedUpdate()
    {
        _rotateVector = Input.mousePosition - _mainCamera.WorldToScreenPoint(_barrelRotationCenter.position);
        _rotateVector.z = 0;
        _barrelRotationCenter.rotation *= Quaternion.FromToRotation(transform.up, _rotateVector);
    }
}
