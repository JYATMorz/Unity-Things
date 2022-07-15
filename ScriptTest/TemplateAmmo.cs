using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScriptTest.Ammo
{
    public class TemplateAmmo
    {
        public int Speed { get; private set; } = 100;
        public int Damage { get; private set; } = 5;
        public int Weight { get; private set; } = 5;

        protected string _ammoName = "Template Ammo";
        protected bool _isExplosive = false;
        protected int _explodeRadius = 0;

        public TemplateAmmo() {}
        public TemplateAmmo(int speed, int damage, int weight)
        {
            Speed = speed;
            Damage = damage;
            Weight = weight;
        }

        public void SelfDestroy()
        {
            if (this._isExplosive) Console.WriteLine($"{this._ammoName} exploded.");
            else Console.WriteLine($"{this._ammoName} destroyed.");
        }

        public int ExplodeRadius()
        {
            if (this._isExplosive) return this._explodeRadius;
            else return 0;
        }

        public void AmmoInfo()
        {
            Console.WriteLine($"------\nAmmo Name: {this._ammoName}\n" + 
                              $"Speed: {Speed}; Damage: {Damage}; Weight: {Weight}\n" + 
                              $"Is it explosive? {this._isExplosive}, so explosion has a radius of {this.ExplodeRadius()}\n");
        }
    }

    public class CommonBullet: TemplateAmmo
    {
        public CommonBullet(): this(200, 10, 5) {}
        public CommonBullet(int speed, int damage, int weight): base(speed, damage, weight)
        {
            this._ammoName = "Common Bullet";
        }
    }

    public class LaserBeam: TemplateAmmo
    {
        public LaserBeam(): this(500, 5, 1) {}
        public LaserBeam(int speed, int damage, int weight): base(speed, damage, weight)
        {
            this._ammoName = "Laser Beam";
        }
    }

    public class ExplosivePayload: TemplateAmmo
    {
        public ExplosivePayload(): this(100, 15, 10, 20) {}
        public ExplosivePayload(int speed, int damage, int weight, int radius): base(speed, damage, weight)
        {
            this._ammoName = "Explosive Payload";
            this._isExplosive = true;
            this._explodeRadius = radius;
        }
    }
}
