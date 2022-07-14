using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScriptTest.Ammo
{
    public class TemplateAmmo
    {
        public int Speed { get; private set; } = 10;
        public int Damage { get; private set; } = 5;
        public int Weight { get; private set; } = 1;
        public bool HasGravity { get; init; } = false;

        protected string _ammoName = "Template Ammo";
        protected bool _isExplosive = false;
        protected int _explodeRadius = 0;

        public static int AmmoCounter = 0;

        public TemplateAmmo(int speed, int damage, int weight)
        {
            Speed = speed;
            Damage = damage;
            Weight = weight;
            ++ AmmoCounter;
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
            Console.WriteLine($"Name: {this._ammoName}:\n" + 
                              $"This is the {TemplateAmmo.AmmoCounter}th bullet.\n" + 
                              $"Speed: {Speed}; Damage: {Damage}; Weight: {Weight};\n" + 
                              $"Is it explosive? {this._isExplosive}, so explosion has a radius of {this.ExplodeRadius()};\n" + 
                              $"Is it affected by gravity? {HasGravity};");
        }
    }

    public class CommonBullet: TemplateAmmo
    {
        public CommonBullet(int speed, int damage, int weight): base(speed, damage, weight)
        {
            this._ammoName = "Common Bullet";
        }
    }

    public class ExplosivePayload: TemplateAmmo
    {
        public ExplosivePayload(int speed, int damage, int weight, int radius): base(speed, damage, weight)
        {
            this._ammoName = "Explosive Payload";
            this._isExplosive = true;
            this._explodeRadius = radius;
            HasGravity = true;
        }
    }
}
