using UnityEngine;
using UnityEngine.VFX;

public struct AmmoData
{
    public Rigidbody AmmoPrefab { get; set; }
    public VisualEffect ShootEffect { get; set; }

    public string Tag { get; private set; }
    public int AmmoSpeed { get; private set; }
    public float FireInterval { get; private set; }
    public bool IsParabola { get; private set; }
    public bool RequireCharge { get; private set; }

    public AmmoData(string tag, int speed, float interval, bool parabola = false, bool charge = false)
    {
        Tag = tag;
        AmmoSpeed = speed;
        FireInterval = interval;
        IsParabola = parabola;
        RequireCharge = charge;
        AmmoPrefab = null;
        ShootEffect = null;
    }

    public override string ToString()
    {
        return $"Ammo Name: {Tag}.";
    }
}
