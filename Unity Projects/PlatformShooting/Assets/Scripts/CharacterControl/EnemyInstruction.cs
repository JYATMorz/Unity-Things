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
    private bool _inRange = false;

    void Start()
    {
        _directionRing = GetComponentInChildren<Image>();
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy || _enemyCollider is null) 
        {
            if (!Mathf.Approximately(_directionRing.color.a, 0f))
            {
                Color transparent = _directionRing.color;
                transparent.a = 0f;
                _directionRing.color = transparent;
            }
            return;
        }

        if (!Mathf.Approximately(_directionRing.color.a, 0.8f))
        {
            Color opaque = _directionRing.color;
            opaque.a = 0.8f;
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
    }

    IEnumerator FindClosestEnemy()
    {
        while (gameObject.activeInHierarchy)
        {
            _inRange = false;
            for (int range = 1; range <=  2 * ConstantSettings.seekRange; range ++)
            {
                if (Physics.OverlapSphereNonAlloc(transform.position, range, _targetColliders, _enemyLayer) > 0)
                {
                    _enemyCollider = _targetColliders[0];
                    _inRange = true;
                    break;
                } else yield return new WaitForFixedUpdate();
            }

            if (!_inRange) _enemyCollider = null;

            yield return new WaitForSeconds(1f);
        }
    }

    public void StartDirectionRing()
    {
        string enemyTag = GetComponentInParent<Collider>()
            .CompareTag(ConstantSettings.blueTeamTag) ? ConstantSettings.redTeamTag : ConstantSettings.redTeamTag;
        _enemyLayer = LayerMask.GetMask(enemyTag);

        StartCoroutine(FindClosestEnemy());
    }
}
