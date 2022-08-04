using UnityEngine;
using UnityEngine.Events;

public class MainCamera : MonoBehaviour
{
    public Transform player;
    public UnityEvent gameOverEvent;

    public static bool IsGameOver = false;

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

    void LateUpdate()
    {
        if (IsGameOver)
        {
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

            // FIXME: Try implement damping
            if (!CameraIsClose(transform.position, player.position, 1f))
                transform.position = player.position + _cameraOffset;
        } else
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position + _cameraOffset, 5 * Time.deltaTime);

            if (CameraIsClose(transform.position, player.position))
            {
                _onPosition = true;
                player.SendMessage("BecomePlayer");
            }
        }
        transform.LookAt(player);
    }

    private void ChangePlayer()
    {
        GameObject nextPlayer = GameObject.FindWithTag(_playerTag);

        if (nextPlayer == null)
        {
            Debug.Log("Game Over. Did Enemy team win?");

            // transform camera to alive team members
            GameObject[] aliveEnemies = GameObject.FindGameObjectsWithTag(_enemyTag);
            foreach (GameObject aliveEnemy in aliveEnemies)
            {
                _endGamePos += aliveEnemy.transform.position;
            }
            _endGamePos /= aliveEnemies.Length;

            IsGameOver = true;
            gameOverEvent.Invoke();

        } else {
            player = nextPlayer.transform;
        }
    }

    private bool CameraIsClose(Vector3 current, Vector3 target, float limit = 0.1f)
    {
        if ((current - target - _cameraOffset).sqrMagnitude < limit) return true;
        else return false;
    }
}