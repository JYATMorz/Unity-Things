using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ScriptTest.Ammo;

namespace ScriptTest.Weapon
{
    public class TemplateWeapon
    {
        public string Name { get; init; } = "Template Weapon";
        public int DistributionDegree { get; private set; } = 0;
        public int FiringRate { get; private set; } = 100;
        public int BulletPerShot { get; private set; } = 1;
        
        public TemplateWeapon(int degree, int rate, int shot)
        {
            DistributionDegree = degree;
            FiringRate = rate;
            BulletPerShot = shot;
        }

        public void WeaponInfo()
        {
            Console.WriteLine($"Weapon Name: {Name}\n" + 
                              $"Distribution Degree: {DistributionDegree}\n" + 
                              $"Rate of Fire: {FiringRate}\n" + 
                              $"Bullet Per Shot: {BulletPerShot}\n-------");
        }

        public virtual void BulletInfo()
        {
            Console.WriteLine("Empty Bullet Info:");
        }
    }

    public class Pistol: TemplateWeapon
    {
        private CommonBullet _bulletType = new CommonBullet(5,5,5);

        public Pistol(int degree, int rate, int shot): base(degree, rate, shot)
        {
            Name = "Pistol";
        }

        public override void BulletInfo()
        {
            Console.WriteLine("Show Bullet Info:");
            Console.WriteLine("-----------------");
            _bulletType.AmmoInfo();
            Console.WriteLine("-----------------");
        }
    }
}