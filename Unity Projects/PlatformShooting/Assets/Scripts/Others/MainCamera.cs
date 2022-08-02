using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public Transform player;

    private Vector3 _cameraOffset = new(0, 1, -8);
    private string _playerTag;
    private bool _onPlayer = true;

    void Awake()
    {
        _playerTag = player.tag;
    }

    void LateUpdate()
    {
        if (_onPlayer)
        {
            if (player == null || player.CompareTag("Dead"))
            {
                _onPlayer = false;
                ChangePlayer();
                return;
            }

            transform.position = player.position + _cameraOffset;
        } else
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, player.position + _cameraOffset, 5 * Time.deltaTime);
            transform.position = newPosition;

            if ((newPosition - player.position - _cameraOffset).sqrMagnitude < 0.1f)
            {
                _onPlayer = true;
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
        } else {
            player = nextPlayer.transform;
        }
    }
}