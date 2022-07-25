using UnityEngine;
using UnityEngine.VFX;

namespace AmmoType
{
    public struct AmmoType
    {
        public Rigidbody AmmoPrefab { get; private set; } = null;
        public VisualEffect ShootEffect { get; private set; } = null;

        public int AmmoSpeed { get; init; } = 20;
        public float FireInterval { get; init; } = 0.5f;
        public float AmmoSpread { get; init; } = 0f;

        public AmmoType(int speed, float interval, float spread)
        {
            AmmoSpeed = speed;
            FireInterval = interval;
            AmmoSpread = spread;
        }

        public override string ToString()
        {
            return $"Ammo Info:\nAmmo Speed: {AmmoSpeed}; Fire Interval: {FireInterval}; Ammo Spread: {AmmoSpread}.";
        }
    }
}