using UnityEngine;
using System.Collections;

public class BrickElevator : MonoBehaviour
{
    private const int _moveSpeed = 2;
    private const float _waitTime = 3f;
    private const float _targetTopPosition = 21.25f;
    private const float _targetDownPosition = 2.25f;
    private const float _startParkingDistance = 1.5f;
    private const string _floorTag = "Floor";

    private bool _goUpward = true;
    private bool _isParking = false;
    private bool _startParking = false;
    private bool _isWaiting = false;
    private float _randomSpeed;
    private Rigidbody brickRB;

    void Start()
    {
        brickRB = GetComponent<Rigidbody>();
        _randomSpeed = _moveSpeed * Random.Range(0.8f, 1.2f);
    }

    void FixedUpdate()
    {
        if (!_isWaiting && !_isParking) MoveElevator();
        if (!_isWaiting && _isParking) ParkElevator();
    }

    private void MoveElevator()
    {
        if (_goUpward) brickRB.position += new Vector3(0, _randomSpeed * Time.deltaTime, 0);
        else brickRB.position -= new Vector3(0, _randomSpeed * Time.deltaTime, 0);

        if (!_startParking && (_startParkingDistance > (brickRB.position.y - _targetDownPosition) || _startParkingDistance > (_targetTopPosition - brickRB.position.y)))
        {
            _startParking = true;
            _isParking = true;
        } else if (_startParking && (_startParkingDistance < (brickRB.position.y - _targetDownPosition) || _startParkingDistance < (_targetTopPosition - brickRB.position.y)))
        {
            _startParking = false;
        }
    }

    private void ParkElevator()
    {
        if (_goUpward)
        {
            brickRB.position = Vector3.MoveTowards(brickRB.position, new Vector3(brickRB.position.x, 21.25f, 0), Time.fixedDeltaTime * _moveSpeed / 2);
        } else
        {
            brickRB.position = Vector3.MoveTowards(brickRB.position, new Vector3(brickRB.position.x, 2.25f, 0), Time.fixedDeltaTime * _moveSpeed / 2);
        }

        if ((Mathf.Abs(transform.position.y - _targetTopPosition) < 0.01f) || (Mathf.Abs(transform.position.y - _targetDownPosition) < 0.01f))
        {
            _isWaiting = true;
            StartCoroutine(WaitBeforeMove());
        }
    }

    IEnumerator WaitBeforeMove()
    {
        _randomSpeed = _moveSpeed * Random.Range(0.8f, 1.2f);
        yield return new WaitForSeconds(_waitTime * (Random.value / 2 + 0.75f));
        _goUpward = !_goUpward;
        _isWaiting = false;
        _isParking = false;
    }
}