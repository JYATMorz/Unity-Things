using UnityEngine;

// attach it to main camera, and move camera to outside
public class MainCamera : MonoBehaviour
{
    public Transform player;

    private Vector3 _cameraOffset = new Vector3(0, 1, 4);

    void LateUpdate()
    {
        transform.position = player.position + _cameraOffset;
        transform.LookAt(player);
    }
}