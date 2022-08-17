using UnityEngine;
using System.Collections;

public class BrickElevator : MonoBehaviour
{
    private const int _moveSpeed = 2;
    private const float _waitTime = 3f;
    private const float _targetTopPosition = 21.25f;
    private const float _targetDownPosition = 2.25f;

    private float _randomSpeed;
    private Rigidbody brickRB;

    void Start()
    {
        brickRB = GetComponent<Rigidbody>();
        _randomSpeed = _moveSpeed * Random.Range(0.8f, 1.2f) * Mathf.Sign(Random.value - 0.5f);

        StartCoroutine(MoveBackAndForth());
    }

    IEnumerator MoveBackAndForth()
    {
        while (true)
        {
            if ((Mathf.Abs(transform.position.y - _targetTopPosition) < 0.01f) ||
                (Mathf.Abs(transform.position.y - _targetDownPosition) < 0.01f))
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
    }

}