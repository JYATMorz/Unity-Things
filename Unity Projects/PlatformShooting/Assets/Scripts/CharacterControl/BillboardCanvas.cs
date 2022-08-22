using UnityEngine;

// attach it to canvas
public class BillboardCanvas : MonoBehaviour
{
    public Transform mainCamera;

    void LateUpdate()
    {
        if (mainCamera == null) return;

        transform.LookAt(transform.position + mainCamera.forward);
    }
}