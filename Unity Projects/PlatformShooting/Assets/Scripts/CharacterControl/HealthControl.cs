using UnityEngine;

public class HealthControl : MonoBehaviour
{
    private HealthBar _healthBar;
    private WeaponControl _weaponControl;
    private CharacterControl _characterControl;
    private TargetControl _targetControl;
    private IMenuUI _gameMenu;
    private int _currentHealth = ConstantSettings.initHealth;

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
    }

    public void ReceiveDamage(int damage, Rigidbody attacker)
    {
        if (attacker == null)
        {
            Debug.LogWarning("Null Attacker Tag");
            return;
        } else if (!_characterControl.IsPlayer
                && ConstantSettings.TargetInRange(attacker.position, transform.position, ConstantSettings.seekRange))
            _targetControl.SwitchTarget(attacker);

        _currentHealth -= Mathf.Clamp(damage, 0, 25);

        GeneralAudioControl.Instance.PlayAudio(
            ConstantSettings.hurtTag, transform.position, _characterControl.IsPlayer ? float.NaN : 0.2f);

        if (_currentHealth <= 0)
        {
            _healthBar.SetHealthValue(0);
            ZeroHealth();
        } else
        {
            _healthBar.SetHealthValue(_currentHealth / (float) ConstantSettings.initHealth);
        }
    }

    public void FullHealth()
    {
        _currentHealth = ConstantSettings.initHealth;
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

        if (_targetControl.TargetCharacter != null)
        {
            if (_targetControl.TargetCharacter.CompareTag(ConstantSettings.neutralTag))
            {
                KillCharacter();
                return;
            }

            FullHealth();
            _gameMenu.TeamChanged(_targetControl.TargetCharacter.tag);
            _targetControl.BecomeTeamMember();

        } else Debug.Log("Target is null when changing team!");
    }

    private void KillCharacter()
    {
        IsDead = true;

        _gameMenu.CharacterDie(gameObject.tag);
        _weaponControl.StopShoot();
        _weaponControl.StopAllCoroutines();

        GeneralAudioControl.Instance.PlayAudio(
            ConstantSettings.deadTag, transform.position, _characterControl.IsPlayer ? float.NaN : 0.2f);

        DeadTagAndLayer();

        _characterControl.BecomeDead();
    }

    private void DeadTagAndLayer()
    {
        gameObject.tag = ConstantSettings.deadTag;
        gameObject.layer = ConstantSettings.deadLayer;
    }
}
