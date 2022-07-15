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
        public int FiringInterval { get; private set; } = 1000;
        public int BulletPerShot { get; private set; } = 1;
        
        public TemplateWeapon() {}
        public TemplateWeapon(int spreadDegree, int timeGap, int shotCount)
        {
            DistributionDegree = spreadDegree;
            FiringInterval = timeGap;
            BulletPerShot = shotCount;
        }

        public void WeaponInfo()
        {
            Console.WriteLine($"------\nWeapon Name: {Name}\n" + 
                              $"Distribution Degree: {DistributionDegree}\n" + 
                              $"Rate of Fire: {FiringInterval}\n" + 
                              $"Bullet Per Shot: {BulletPerShot}");
        }

        public void ShootBullet<T>(ref T ammo) where T: TemplateAmmo
        {
            // spawn bullet in order in Unity according to given ammo
            // use BulletPerShot to control spawn number
            ammo.AmmoInfo();
        }
    }

    public class Pistol: TemplateWeapon
    {
        public Pistol(): this(5, 500, 1){}
        public Pistol(int spreadDegree, int timeGap, int shotCount): base(spreadDegree, timeGap, shotCount)
        {
            Name = "Pistol";
        }
    }

    public class Rifle: TemplateWeapon
    {
        public Rifle(): this(2, 200, 3){}
        public Rifle(int spreadDegree, int timeGap, int shotCount): base(spreadDegree, timeGap, shotCount)
        {
            Name = "Rifle";
        }
    }

    public class Shotgun: TemplateWeapon
    {
        public Shotgun(): this(25, 1000, 5){}
        public Shotgun(int spreadDegree, int timeGap, int shotCount): base(spreadDegree, timeGap, shotCount)
        {
            Name = "Shotgun";
        }
    }

    public class Grenade: TemplateWeapon
    {
        public Grenade(): this(0, 2000, 1) {}
        public Grenade(int spreadDegree, int timeGap, int shotCount): base(spreadDegree, timeGap, shotCount)
        {
            Name = "Grenade";
        }
    }
}