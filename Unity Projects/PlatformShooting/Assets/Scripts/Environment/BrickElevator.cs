using UnityEngine;
using System.Collections;

public class BrickElevator : MonoBehaviour
{
    private const int _moveSpeed = 2;
    private const float _waitTime = 3f;
    private const float _targetTopPosition = 21.25f;
    private const float _targetDownPosition = 2.25f;

    private float _randomSpeed = _moveSpeed;
    private Rigidbody brickRB;

    void Awake()
    {
        brickRB = GetComponent<Rigidbody>();
        _randomSpeed = _moveSpeed * Random.Range(0.8f, 1.2f) * Mathf.Sign(Random.value - 0.5f);
    }

    void Start()
    {  
        StartCoroutine(MoveBackAndForth());
    }

    IEnumerator MoveBackAndForth()
    {
        while (true)
        {
            if (IsCloseToStop(0.01f))
            {
                yield return StartCoroutine(WaitBeforeMove());
            }

            brickRB.position += new Vector3(0, _randomSpeed * Time.deltaTime, 0);
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator WaitBeforeMove()
    {
        _randomSpeed = _moveSpeed * Random.Range(0.8f, 1.2f) * (- Mathf.Sign(_randomSpeed));
        yield return new WaitForSeconds(_waitTime * Random.Range(0.8f, 1.2f));

        while (IsCloseToStop(2f))
        {
            brickRB.position += new Vector3(0, _randomSpeed * Time.deltaTime, 0);
            yield return new WaitForFixedUpdate();
        }
    }

    private bool IsCloseToStop(float distance)
    {
        return ((_targetTopPosition - transform.position.y) < distance) ||
                ((transform.position.y - _targetDownPosition) < distance);
    }

}