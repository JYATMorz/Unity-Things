using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyInstruction : MonoBehaviour
{
    public GameObject targetInstruction;

    private readonly Collider[] _targetCollider = new Collider[1];

    private const float _angularSpeed = 60f;

    private Transform _targetTransform = null;
    private Vector3 _targetDirection;
    private Image _directionRing;
    private int _enemyLayer = -1;

    void Start()
    {
        _directionRing = GetComponentInChildren<Image>();

        string enemyTag = CompareTag(ConstantSettings.blueTeamTag) ? ConstantSettings.redTeamTag : ConstantSettings.redTeamTag;
        _enemyLayer = LayerMask.NameToLayer(enemyTag);

        // FIXME: rotation test
        StartCoroutine(FindClosestEnemy());
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy || _targetTransform == null) 
        {
            if (!Mathf.Approximately(_directionRing.color.a, 0f))
            {
                Color transparent = _directionRing.color;
                transparent.a = 0;
                _directionRing.color = transparent;
            }
            return;
        }

        if (!Mathf.Approximately(_directionRing.color.a, 1f))
        {
            Color opaque = _directionRing.color;
            opaque.a = 1;
            _directionRing.color = opaque;
        }

        // BUG: Wrong Computation
        _targetDirection = _targetTransform.position - transform.position;
        _targetDirection.z = 0;
        targetInstruction.transform.rotation = 
            Quaternion.LookRotation(
                Vector3.RotateTowards(transform.up, _targetDirection.normalized, _angularSpeed * Time.deltaTime * Mathf.Deg2Rad, 0f)
            );
    }

    IEnumerator FindClosestEnemy()
    {
        while (gameObject.activeInHierarchy)
        {
            for (int range = 3; range <= ConstantSettings.seekRange; range ++)
            {
                if (Physics.OverlapSphereNonAlloc(transform.position, range, _targetCollider, _enemyLayer) > 0)
                {
                    _targetTransform = _targetCollider[0].transform;
                    break;
                } else
                {
                    _targetTransform = null;
                    yield return new WaitForFixedUpdate();
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }

    public void StartDirectionRing()
    {
        StartCoroutine(FindClosestEnemy());
    }
}
