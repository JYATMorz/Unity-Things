using UnityEngine;
using System.Collections;

public class BrickElevator : MonoBehaviour
{
    private const int _moveSpeed = 5;
    private const float _waitTime = 3f;
    private const float _parkTime = 3f;
    // TODO: Use actual position to replace these positions
    private const float _targetTopPosition = 20.5f;
    private const float _targetDownPosition = 2.5f;
    private const string _floorTag = "Floor";

    private bool _goUpward = true;
    private bool _isParking = false;
    private bool _isWaiting = false;
    private float _parkSpeed = 0f;
    private Rigidbody brickRB;

    void Start()
    {
        brickRB = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!_isWaiting && !_isParking) MoveElevator();
        if (!_isWaiting && _isParking) ParkElevator();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_floorTag) && !_isParking)
        {
            _isParking = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(_floorTag) && _isParking)
        {
            _isParking = false;
        }
    }

    private void MoveElevator()
    {
        if (_goUpward) brickRB.position += new Vector3(0, _moveSpeed * Time.deltaTime, 0);
        else brickRB.position -= new Vector3(0, _moveSpeed * Time.deltaTime, 0);
    }

    private void ParkElevator()
    {
        if (_goUpward)
        {
            float newPosition = Mathf.SmoothDamp(transform.position.y, _targetTopPosition, ref _parkSpeed, _parkTime);
            brickRB.position = new Vector3(transform.position.x, newPosition, transform.position.z);

            Debug.Log(_parkSpeed);
        } else
        {
            float newPosition = Mathf.SmoothDamp(transform.position.y, _targetDownPosition, ref _parkSpeed, _parkTime);
            brickRB.position = new Vector3(transform.position.x, newPosition, transform.position.z);
        }

        if ((Mathf.Abs(transform.position.y - _targetTopPosition) < 0.01f) || (Mathf.Abs(transform.position.y - _targetDownPosition) < 0.01f))
        {
            _isWaiting = true;
            StartCoroutine(WaitBeforeMove());
            _parkSpeed = 0f;
        }
    }

    IEnumerator WaitBeforeMove()
    {
        yield return new WaitForSeconds(_waitTime * (Random.value / 2 + 0.75f));
        _goUpward = !_goUpward;
        _isWaiting = false;
    }
}