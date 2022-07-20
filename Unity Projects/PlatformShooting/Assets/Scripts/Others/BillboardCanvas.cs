using UnityEngine;

// attach it to canvas
public class BillboardCanvas : MonoBehaviour
{
    public Transform mainCamera;

    void LateUpdate()
    {
        transform.LookAt(transform.position + mainCamera.forward);
    }
}