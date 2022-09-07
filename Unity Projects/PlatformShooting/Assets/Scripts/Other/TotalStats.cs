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

    public void NewKill() => TotalKill ++;

    public void NewDamage(int damage) => TotalDamage += damage;

    public void NewAmmo() => TotalAmmo ++;

    public void Clear()
    {
        TotalAmmo = 0;
        TotalDamage = 0;
        TotalDamage = 0;
    }

    public override string ToString()
    {
        return "Total Enemies Killed: " + TotalKill +
            "\n Total Damages Caused: " + TotalDamage +
            "\n Total Ammo Fired: " + TotalAmmo;
    }
}