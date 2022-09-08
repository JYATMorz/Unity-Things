public struct TotalStats
{
    public int TotalKill { get; private set; }
    public int TotalDamage { get; private set; }
    public int TotalAmmo { get; private set; }

    public TotalStats(int kill = 0, int damage = 0, int ammo = 0)
    {
        TotalKill = kill;
        TotalDamage = damage;
        TotalAmmo = ammo;
    }

    public TotalStats(TotalStats oldStat)
    {
        TotalKill = oldStat.TotalKill;
        TotalDamage = oldStat.TotalDamage;
        TotalAmmo = oldStat.TotalAmmo;
    }

    public void NewKill() => TotalKill ++;

    public void NewDamage(int damage) => TotalDamage += damage;

    public void NewAmmo() => TotalAmmo ++;

    public void Clear()
    {
        TotalKill = 0;
        TotalDamage = 0;
        TotalAmmo = 0;
    }

    public void Max(TotalStats newStat)
    {
        TotalKill = TotalKill > newStat.TotalKill ? TotalKill : newStat.TotalKill;
        TotalDamage = TotalDamage > newStat.TotalDamage ? TotalDamage : newStat.TotalDamage;
        TotalAmmo = TotalAmmo > newStat.TotalAmmo ? TotalAmmo : newStat.TotalAmmo;
    }

    public static TotalStats operator + (TotalStats leftStat, TotalStats rightStat)
    {
        return new TotalStats(
            leftStat.TotalKill + rightStat.TotalKill,
            leftStat.TotalDamage + rightStat.TotalDamage,
            leftStat.TotalAmmo + rightStat.TotalAmmo
        );
    }

    public override string ToString()
    {
        return "Total Enemies Killed: " + TotalKill +
            "\n Total Damages Caused: " + TotalDamage +
            "\n Total Ammo Fired: " + TotalAmmo;
    }
}