using UnityEngine;

// attach it to main camera, and move camera to outside
public class MainCamera : MonoBehaviour
{
    public Transform player;

    private Vector3 _cameraOffset = new(0, 1, -8);

    void LateUpdate()
    {
        if (player != null)
        {
            transform.position = player.position + _cameraOffset;
            transform.LookAt(player);
        }
    }
}