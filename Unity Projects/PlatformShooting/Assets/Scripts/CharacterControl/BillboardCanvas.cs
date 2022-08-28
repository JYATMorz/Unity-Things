using UnityEngine;

public class BillboardCanvas : MonoBehaviour
{
    public Transform mainCamera;

    void LateUpdate()
    {
        if (mainCamera is null) return;

        transform.LookAt(transform.position + mainCamera.forward);
    }
}