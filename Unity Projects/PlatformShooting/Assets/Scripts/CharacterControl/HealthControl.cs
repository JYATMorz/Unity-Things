using UnityEngine;

public class HealthControl : MonoBehaviour
{
    private const int _healthScalar = 2;

    private HealthBar _healthBar;
    private WeaponControl _weaponControl;
    private CharacterControl _characterControl;
    private TargetControl _targetControl;
    private IMenuUI _gameMenu;
    private int _currentHealth = ConstantSettings.initHealth;
    private int _fullHealth = ConstantSettings.initHealth;

    public bool IsDead { get; private set; } = false;

    [Header("Game Scene Elements")]
    public GameObject sceneMenu;

    void Awake()
    {
        _weaponControl = GetComponent<WeaponControl>();
        _targetControl = GetComponent<TargetControl>();
        _characterControl = GetComponent<CharacterControl>();

        _gameMenu = sceneMenu.GetComponent<IMenuUI>();

    }
    void Start()
    {
        _healthBar = GetComponentInChildren<HealthBar>();
        if (!_characterControl.IsNeutral)
        {
            _currentHealth = _healthScalar * ConstantSettings.initHealth;
            _fullHealth = _healthScalar * ConstantSettings.initHealth;
        }
    }

    public void ReceiveDamage(int damage, Rigidbody attacker)
    {
        if (attacker is null)
        {
            ZeroHealth();
            return;
        } else if (attacker.CompareTag(ConstantSettings.deadTag)) return;

        if (!_characterControl.IsPlayer
                && ConstantSettings.TargetInRange(attacker.position, transform.position, ConstantSettings.seekRange))
            _targetControl.SwitchTarget(attacker);

        if (!attacker.CompareTag(tag)) _currentHealth -= Mathf.Clamp(damage, 0, 30);

        GeneralAudioControl.Instance.PlayAudio(
            ConstantSettings.hurtTag, transform.position, _characterControl.IsPlayer ? float.NaN : 0.2f);

        if (_currentHealth <= 0)
        {
            _healthBar.SetHealthValue(0);
            ZeroHealth();
        } else
        {
            _healthBar.SetHealthValue(_currentHealth / (float) _fullHealth);
        }
    }

    public void FullHealth()
    {
        _currentHealth = _healthScalar * ConstantSettings.initHealth;
        _fullHealth = _healthScalar * ConstantSettings.initHealth;
        _healthBar.SetMaxHealth();

        GeneralAudioControl.Instance.PlayAudio(ConstantSettings.reviveTag, transform.position, 0.2f);
    }

   private void ZeroHealth()
    {
        if (!_characterControl.IsNeutral)
        {
            KillCharacter();
            return;
        }

        if (_targetControl.TargetCharacter is not null)
        {
            if (_targetControl.TargetCharacter.CompareTag(ConstantSettings.neutralTag))
            {
                KillCharacter();
                return;
            }

            FullHealth();
            _gameMenu.TeamChanged(_targetControl.TargetCharacter.tag);
            _targetControl.BecomeTeamMember();

        } else KillCharacter();
    }

    private void KillCharacter()
    {
        IsDead = true;

        _gameMenu.CharacterDie(tag);
        _weaponControl.StopShoot();
        _weaponControl.StopAllCoroutines();

        GeneralAudioControl.Instance.PlayAudio(
            ConstantSettings.deadTag, transform.position, _characterControl.IsPlayer ? float.NaN : 0.2f);

        DeadTagAndLayer();

        _characterControl.BecomeDead();
    }

    private void DeadTagAndLayer()
    {
        tag = ConstantSettings.deadTag;
        gameObject.layer = ConstantSettings.deadLayer;
    }
}
