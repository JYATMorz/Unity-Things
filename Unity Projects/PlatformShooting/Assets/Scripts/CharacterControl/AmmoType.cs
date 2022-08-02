using UnityEngine;
using UnityEngine.VFX;

namespace AmmoType
{
    public struct AmmoData
    {
        public Rigidbody AmmoPrefab { get; set; }
        public VisualEffect ShootEffect { get; set; }

        public int AmmoSpeed { get; private set; }
        public float FireInterval { get; private set; }
        public float AmmoSpread { get; private set; }

        public AmmoData(int speed, float interval, float spread)
        {
            AmmoSpeed = speed;
            FireInterval = interval;
            AmmoSpread = spread;
            AmmoPrefab = null;
            ShootEffect = null;
        }

        public override string ToString()
        {
            return $"Ammo Info:\nAmmo Speed: {AmmoSpeed}; Fire Interval: {FireInterval}; Ammo Spread: {AmmoSpread}.";
        }
    }
}