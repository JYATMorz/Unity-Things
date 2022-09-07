public interface IMenuUI
{
    public static bool IsPause { get; set; } = false;
    public WeaponControl CurrentWeaponControl { get; set; }

    public void CharacterDie(string tag);

    public void TeamChanged(string tag);

    public void ShowNotification(string noteType);

    public void SwitchWeaponIcon(int num);

    public void UpdateCharacterStats(TotalStats newStat, string teamTag, bool isPlayer = false);
}
