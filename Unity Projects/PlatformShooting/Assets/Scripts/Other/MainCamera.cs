using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public Transform player;

    public static bool IsGameOver { get; private set;} = false;

    private const float _limitPositionX = 45f;

    private Vector3 _cameraOffset = new(0, 1, -8);
    private Vector3 _endGamePos = Vector3.zero;
    private string _playerTag;
    private string _enemyTag;
    private bool _onPosition = true;

    void Awake()
    {
        _playerTag = player.tag;
        _enemyTag = (_playerTag == "BlueTeam") ? "RedTeam" : "BlueTeam";
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
        if (IsGameOver)
        {
            if (player != null && !player.CompareTag("Dead")) FocusOnPlayer();

            if (_onPosition) return;

            if (!CameraIsClose(transform.position, _endGamePos))
                transform.position = Vector3.MoveTowards(transform.position, _endGamePos + _cameraOffset, 5 * Time.deltaTime);
            else _onPosition = true;

            return;
        }

        if (_onPosition)
        {
            if (player == null || player.CompareTag("Dead"))
            {
                _onPosition = false;
                ChangePlayer();
                return;
            }

            FocusOnPlayer();

        } else
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position + _cameraOffset, 20 * Time.deltaTime);

            if (CameraIsClose(transform.position, player.position))
            {
                _onPosition = true;
                player.GetComponent<CharacterControl>().BecomePlayer();
            }

            transform.LookAt(player);

        }
    }

    private void ChangePlayer()
    {
        GameObject nextPlayer = GameObject.FindWithTag(_playerTag);

        if (nextPlayer == null)
        {
            // transform camera to alive team members
            GameObject[] aliveEnemies = GameObject.FindGameObjectsWithTag(_enemyTag);
            foreach (GameObject aliveEnemy in aliveEnemies)
            {
                _endGamePos += aliveEnemy.transform.position;
            }
            _endGamePos /= aliveEnemies.Length;

        } else {
            player = nextPlayer.transform;
        }
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
    }
}