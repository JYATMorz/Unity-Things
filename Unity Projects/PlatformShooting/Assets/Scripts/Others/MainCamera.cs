using UnityEngine;

// attach it to main camera, and move camera to outside
public class MainCamera : MonoBehaviour
{
    public Transform player;

    private Vector3 _cameraOffset = new(0, 1, -8);
    private string _playerTag;

    void Awake()
    {
        _playerTag = player.tag;
    }

    void LateUpdate()
    {
        if (player != null && !player.CompareTag("Dead"))
        {
            // TODO: Shaking Camera, please only used during switching player
            // transform.position = Vector3.MoveTowards(transform.position, player.position + _cameraOffset, 10 * Time.deltaTime);

            transform.position = player.position + _cameraOffset;
            transform.LookAt(player);
        } else
        {
            GameObject nextPlayer = GameObject.FindWithTag(_playerTag);
            if (nextPlayer == null)
            {
                Debug.Log("Did Enemy team win?");
            } else {
                player = nextPlayer.transform;
                // TODO: implement method to control the selected character
            }
        }
    }
}