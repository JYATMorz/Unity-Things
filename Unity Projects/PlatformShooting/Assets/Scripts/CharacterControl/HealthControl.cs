using UnityEngine;

public class HealthControl : MonoBehaviour
{
    private const int _initHealth = 100;

    private HealthBar _healthBar;
    private WeaponControl_TempBugFix _weaponControl;
    private CharacterControl_TempBugFix _characterControl;
    private TargetControl _targetControl;
    private IMenuUI _gameMenu;
    private int _currentHealth = _initHealth;

    public bool IsDead { get; private set; } = false;

    [Header("Game Scene Elements")]
    public GameObject sceneMenu;

    void Awake()
    {
        _weaponControl = GetComponent<WeaponControl_TempBugFix>();
        _targetControl = GetComponent<TargetControl>();
        _characterControl = GetComponent<CharacterControl_TempBugFix>();

        _gameMenu = sceneMenu.GetComponent<IMenuUI>();

    }
    void Start()
    {
        _healthBar = GetComponentInChildren<HealthBar>();
    }

    public void ReceiveDamage(int damage, Rigidbody attacker = null)
    {
        if (!_characterControl.IsPlayer && attacker != null) _targetControl.SwitchTarget(attacker);

        _currentHealth -= Mathf.Clamp(damage, 0, 25);

        if (_currentHealth <= 0)
        {
            _healthBar.SetHealthValue(0);
            ZeroHealth();
        } else
        {
            _healthBar.SetHealthValue(_currentHealth / (float)_initHealth);
        }
    }

    public void FullHealth()
    {
        _currentHealth = _initHealth;
        _healthBar.SetMaxHealth();
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
                KillCharacter(); // FIXME: add neutral tag in GameMenu.cs
                return;
            }

            FullHealth();
            _gameMenu.TeamChanged(gameObject.tag);

            _targetControl.ResetTarget();

        } else Debug.Log("Target is null when changing team!");
    }

    private void KillCharacter()
    {
        IsDead = true;

        _gameMenu.CharacterDie(gameObject.tag);
        _weaponControl.StopShoot();
        _weaponControl.StopAllCoroutines();

        DeadTagAndLayer();

        _characterControl.BecomeDead();
    }

    private void DeadTagAndLayer()
    {
        gameObject.tag = ConstantSettings.deadTag;
        gameObject.layer = ConstantSettings.deadLayer;
    }
}
