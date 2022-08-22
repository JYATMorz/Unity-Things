using UnityEngine;
using System.Collections;

public class EnemyInstruction : MonoBehaviour
{
    public GameObject targetInstruction;

    private Vector3 _targetPosition;
    private int _enemyLayer = -1;

    void Start()
    {
        string enemyTag = CompareTag(ConstantSettings.blueTeamTag) ? ConstantSettings.redTeamTag : ConstantSettings.redTeamTag;
        _enemyLayer = LayerMask.NameToLayer(enemyTag);
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy) return;
    }

    IEnumerator FindClosestEnemy()
    {
        Collider[] results = Physics.OverlapSphere(transform.position, ConstantSettings.seekRange, _enemyLayer);

        // collide with sphere
        // Quaternion.RotateTowards
    }
}
