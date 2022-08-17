using UnityEngine;
using System.Collections;

public class TargetControl : MonoBehaviour
{
    private CharacterControl _characterControl;
    private WeaponControl _weaponControl;
    private HealthControl _healthControl;

    public GameObject TargetCharacter { get; private set; } = null;
    public Vector3 TargetPosition { get; private set; }
    public int EnemyLayer { get; set; } = -1;

    void Awake()
    {
        _characterControl = GetComponent<CharacterControl>();
        _weaponControl = GetComponent<WeaponControl>();
        _healthControl = GetComponent<HealthControl>();

        if (!_characterControl.IsNeutral)
        {
            string enemyTag = CompareTag(ConstantSettings.blueTeamTag) ? ConstantSettings.redTeamTag : ConstantSettings.blueTeamTag;
            EnemyLayer = LayerMask.GetMask(enemyTag, ConstantSettings.neutralTag);
        }
    }

    void Start()
    {
        StartCoroutine(SeekEnemy());
    }
    
    void FixedUpdate()
    {        
        if (TargetCharacter == null) 
            _weaponControl.IsBarrelIdle = true;
        else if (TargetCharacter.CompareTag(ConstantSettings.deadTag) || CompareTag(TargetCharacter.tag))
            TargetCharacter = null;
        else if (!ConstantSettings.ObstacleBetween(TargetCharacter.transform.position, transform.position))
        {
            _weaponControl.IsBarrelIdle = false;
            TargetPosition = TargetCharacter.transform.position;
        }
    }

    IEnumerator SeekEnemy()
    {
        while(true && !_healthControl.IsDead)
        {
            if(!_characterControl.IsNeutral)
            {
                if (TargetCharacter == null) yield return StartCoroutine(SearchTarget());
                else
                {
                    Vector3 targetPos = TargetCharacter.transform.position;
                    if (!ConstantSettings.TargetInRange(targetPos, transform.position, ConstantSettings.seekRange))
                        TargetCharacter = null;
                    else if (!ConstantSettings.TargetInRange(targetPos, transform.position, ConstantSettings.shootRange))
                    {
                        TargetPosition = targetPos;
                        _characterControl.ChaseMode = true;
                    } else if (ConstantSettings.ObstacleBetween(targetPos, transform.position))
                    {
                        if (ConstantSettings.TargetInRange(targetPos, transform.position, 0.5f * ConstantSettings.shootRange))
                            TargetCharacter = null;
                        else
                        {
                            TargetPosition = targetPos;
                            _characterControl.ChaseMode = true;
                        }
                    }
                }
            }
            yield return new WaitForSeconds(ConstantSettings.seekInterval);
        }
    }

    IEnumerator SearchTarget()
    {
        foreach (Collider target in Physics.OverlapSphere(transform.position, ConstantSettings.seekRange, EnemyLayer))
        {
            if (!ConstantSettings.ObstacleBetween(target.transform.position, transform.position))
            {
                TargetCharacter = target.gameObject;
                break;
            }
            yield return null;
        }
    }

    public void SwitchTarget(Rigidbody suspect)
    {
        Vector3 suspectPosition = suspect.position;
        GameObject previousTarget = TargetCharacter;

        if (_characterControl.IsNeutral) // I'm a neutral character
        {
            if (TargetCharacter == null) // Currently has no target
                TargetCharacter = suspect.gameObject;
            else if (TargetCharacter.CompareTag(ConstantSettings.neutralTag)) // Currently has neutral target
            {
                if (suspect.CompareTag(ConstantSettings.neutralTag))
                    // 50% chance to switch from one neutral target to another
                    TargetCharacter = (Random.value < 0.5f) ? suspect.gameObject : TargetCharacter;
                else
                    // 100% chance to switch from one neutral target to team target
                    TargetCharacter = suspect.gameObject;
            } else if (!suspect.CompareTag(ConstantSettings.neutralTag)) // Currently has team target & suspect is not neutral
            {
                if (!ConstantSettings.TargetInRange(TargetCharacter.transform.position, transform.position, ConstantSettings.seekRange))
                { // Current target is out of seek range
                    TargetCharacter = suspect.gameObject;
                } else if (!ConstantSettings.TargetInRange(TargetCharacter.transform.position, transform.position, ConstantSettings.shootRange))
                { // Current target is out of shoot range
                    TargetCharacter = ConstantSettings.ObstacleBetween(suspectPosition, transform.position)
                        ? TargetCharacter : suspect.gameObject;
                }
            }
        } else // I'm a team character
        {
            if (CompareTag(suspect.tag)) return; // Attack by teammate

            if (TargetCharacter == null) TargetCharacter = suspect.gameObject;
            else if (!suspect.CompareTag(ConstantSettings.neutralTag))
            {
                if (TargetCharacter.CompareTag(ConstantSettings.neutralTag)) TargetCharacter = suspect.gameObject;
                else TargetCharacter =
                        (ConstantSettings.ObstacleBetween(TargetCharacter.transform.position, transform.position)
                        && !ConstantSettings.ObstacleBetween(suspectPosition, transform.position))
                        ? suspect.gameObject : TargetCharacter;
            }
        }

        if (TargetCharacter != null && previousTarget != TargetCharacter)
        {
            TargetPosition = TargetCharacter.transform.position;
            _characterControl.ChaseMode = true;
        }
    }

    public void BecomeTeamMember()
    {
        TargetCharacter.GetComponent<HealthControl>().FullHealth();

        if (TargetCharacter.CompareTag(ConstantSettings.blueTeamTag))
        {
            _characterControl.SwitchToTeamLayer(ConstantSettings.blueTeamTag, ConstantSettings.redTeamTag);
        } else
        {
            _characterControl.SwitchToTeamLayer(ConstantSettings.redTeamTag, ConstantSettings.blueTeamTag);
        }

        TargetCharacter = null;
    }
}