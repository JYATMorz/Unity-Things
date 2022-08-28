using UnityEngine;
using System.Collections;

public class MainCamera : MonoBehaviour
{
    public Transform player;

    public static bool IsGameOver { get; private set;} = false;

    private const float _limitPositionX = 45f;

    private Vector3 _cameraOffset = new (0, 1, -8);
    private Vector3 _endGamePos = Vector3.zero;
    private string _playerTag;
    private string _enemyTag;
    private static bool _onPosition = true;
    private bool _waitDeath = false;

    void Awake()
    {
        _playerTag = player.tag;
        _enemyTag = player.CompareTag(ConstantSettings.blueTeamTag)
                    ? ConstantSettings.redTeamTag
                    : ConstantSettings.blueTeamTag;
    }

    void Start()
    {
        IsGameOver = false;
        transform.position = player.position + _cameraOffset;
        transform.LookAt(player);
        _onPosition = true;
    }

    void LateUpdate()
    {
        if (_onPosition)
        {
            if (player is null || player.CompareTag("Dead"))
            {
                _onPosition = false;
                _waitDeath = true;

                if (_endGamePos == Vector3.zero) StartCoroutine(ChangePlayer());
                else if (!CameraIsClose(transform.position, _endGamePos + _cameraOffset))
                {
                    transform.position = 
                        Vector3.MoveTowards(transform.position, _endGamePos + _cameraOffset, 5 * Time.deltaTime);
                }
                return;
            }

            FocusOnPlayer();

        } else if (!_waitDeath)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position + _cameraOffset, 20 * Time.deltaTime);

            if (CameraIsClose(transform.position, player.position))
            {
                _onPosition = true;
                if (!IsGameOver) player.GetComponent<CharacterControl>().BecomePlayer();
            }

            transform.LookAt(player);

        }
    }

    IEnumerator ChangePlayer()
    {
        yield return new WaitForSeconds(1f);

        GameObject nextPlayer = GameObject.FindWithTag(_playerTag);
        if (nextPlayer is null)
        {
            // transform camera to alive enemy
            nextPlayer = GameObject.FindWithTag(_enemyTag);
            if (nextPlayer is not null)
            {
                _playerTag = _enemyTag;
                player = nextPlayer.transform;
            } else
            {
                _endGamePos = new Vector3(0, 5, 0);
            }
        } else
        {
            player = nextPlayer.transform;
        }

        _waitDeath = false;
    }

    private void FocusOnPlayer()
    {
        if (Mathf.Abs(player.position.x) < _limitPositionX)
        {
            transform.position = player.position + _cameraOffset;
            transform.LookAt(player);
        } else
        {
            transform.position = new Vector3(Mathf.Sign(player.position.x) * _limitPositionX, player.position.y, 0) + _cameraOffset;
        }
    }

    private bool CameraIsClose(Vector3 current, Vector3 target, float limit = 0.1f)
    {
        if ((current - target - _cameraOffset).sqrMagnitude < limit) return true;
        else return false;
    }

    public static void GameIsOver()
    {
        IsGameOver = true;
        _onPosition = false;
    }
}