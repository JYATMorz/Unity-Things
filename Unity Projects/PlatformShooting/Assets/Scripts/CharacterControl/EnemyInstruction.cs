using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyInstruction : MonoBehaviour
{
    private readonly Collider[] _targetColliders = new Collider[1];

    private const float _angularSpeed = 60f;

    private Collider _enemyCollider = null;
    private Vector3 _targetDirection;
    private Image _directionRing;
    private int _enemyLayer;

    void Start()
    {
        _directionRing = GetComponentInChildren<Image>();

        // FIXME: string enemyTag = CompareTag(ConstantSettings.blueTeamTag) ? ConstantSettings.redTeamTag : ConstantSettings.redTeamTag;
        string enemyTag = ConstantSettings.redTeamTag;
        _enemyLayer = LayerMask.NameToLayer(enemyTag);

        // FIXME: rotation test, enable when become player
        StartCoroutine(FindClosestEnemy());
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy || _enemyCollider == null) 
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

        _targetDirection = _enemyCollider.transform.position - transform.position;
        _targetDirection.z = 0;
        transform.rotation = 
            Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.FromToRotation(Vector3.up, _targetDirection.normalized),
                ConstantSettings.barrelRotateSpeed * Time.deltaTime * 5f
            );
        // FIXME: Debug.Log(_targetTransform.position); (0,-10,0) Reset Plane
    }

    IEnumerator FindClosestEnemy()
    {
        while (gameObject.activeInHierarchy)
        {
            for (int range = 1; range <=  2 * ConstantSettings.seekRange; range ++)
            {
                // BUG: What is the layer used for?
                if (Physics.OverlapSphereNonAlloc(transform.position, range, _targetColliders, _enemyLayer) > 0)
                {
                    _enemyCollider = _targetColliders[0];
                    break;
                } else
                    yield return new WaitForFixedUpdate();
            }

            yield return new WaitForSeconds(1f);
        }
    }

    public void StartDirectionRing()
    {
        StartCoroutine(FindClosestEnemy());
    }
}
