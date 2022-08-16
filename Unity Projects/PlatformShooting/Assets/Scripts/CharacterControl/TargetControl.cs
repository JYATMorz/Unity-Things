using UnityEngine;
using System.Collections;

public class TargetControl : MonoBehaviour
{
    private const float _seekInterval = 1f;
    private const float _seekRange = 30f;

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

    void OnCollisionEnter(Collision other)
    {
        GameObject contact = other.gameObject;

        if (contact.CompareTag(ConstantSettings.bulletTag))
        {
            if (!_characterControl.IsPlayer) SwitchTarget(contact.GetComponentsInParent<Rigidbody>()[2]);

        }
    }
    
    private IEnumerator SeekEnemy()
    {
        while(true && !_healthControl.IsDead)
        {
            if(!_characterControl.IsNeutral)
            {
                if (TargetCharacter == null) SearchTarget();
                else
                {
                    Vector3 targetPos = TargetCharacter.transform.position;
                    if (!ConstantSettings.TargetInRange(targetPos, transform.position, _seekRange))
                        TargetCharacter = null;
                    else
                        _characterControl.ChaseMode = true;
                }
            }
            yield return new WaitForSeconds(_seekInterval);
        }
    }

    private void SearchTarget()
    {
        foreach (Collider target in Physics.OverlapSphere(transform.position, _seekRange, EnemyLayer))
        {
            if (!ConstantSettings.ObstacleBetween(target.transform.position, transform.position))
            {
                TargetCharacter = target.gameObject;
                break;
            }
        }
    }

    public void SwitchTarget(Rigidbody suspect)
    {
        Vector3 suspectPosition = suspect.position;

        if (_characterControl.IsNeutral) // I'm a neutral character
        {
            if (TargetCharacter == null) // Currently has no target
                TargetCharacter = suspect.gameObject;
            else if (TargetCharacter.CompareTag(ConstantSettings.neutralTag)) // Currently has neutral target
            {
                if (suspect.CompareTag(ConstantSettings.neutralTag))
                    // 50% chance to switch from one neutral target to another
                    TargetCharacter = (UnityEngine.Random.value < 0.5f) ? suspect.gameObject : TargetCharacter;
                else
                    // 100% chance to switch from one neutral target to team target
                    TargetCharacter = suspect.gameObject;
            } else if (!suspect.CompareTag(ConstantSettings.neutralTag)) // Currently has team target & suspect is not neutral
            {
                if (!ConstantSettings.TargetInRange(TargetCharacter.transform.position, transform.position, _seekRange)) // Current target is out of seek range
                {
                    TargetCharacter = suspect.gameObject;
                } else if (!ConstantSettings.TargetInRange(TargetCharacter.transform.position, transform.position, _weaponControl.shootRange)) // Current target is out of shoot range
                {
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

        if (TargetCharacter != null) _characterControl.ChaseMode = true;
    }

    public void ResetTarget()
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