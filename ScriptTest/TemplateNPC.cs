using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScriptTest.Ammo;
using ScriptTest.Weapon;

namespace ScriptTest.NPC
{
    public class TemplateNPC
    {
        public int Health { get; private set; } = 100;
        public int Speed { get; private set; } = 100;
        public int Weight { get; private set; } = 50;

        protected int _reviveChance = 0;
        protected bool _isDead = false;

        public TemplateNPC(int health, int speed, int weight)
        {
            Health = health;
            Speed = speed;
            Weight = weight;
        }

        public void ReceiveDamage(int damage)
        {
            Health -= damage;
            if (Health < 0) ZeroHealth();
            else Console.WriteLine($"Ouch! {Health} HP left.");
        }

        public void ZeroHealth()
        {
            if (this._reviveChance <= 0)
            {
                this._isDead = true;
                Console.WriteLine("No revive chance, Time to die.");
                // Unity remove this object
            } else
            {
                this._isDead = false;
                this._reviveChance --;
                Health = 100;
                Console.WriteLine("I'm back. I can do this all day.");
            }
        }

        public void Shoot<T, U>(ref T weapon, ref U ammo)
        {
            if (!this._isDead)
            {
                weapon.WeaponInfo();
                weapon.ShootBullet(ammo);
            }
        }

        protected static Type RandomWeaponType()
        {
            Random dice = new Random();
            string weaponType = "TemplateWeapon";
            switch (dice.Next(6))
            {
                case 0:
                case 1:
                    weaponType = "Pistol";
                    break;
                case 2:
                    weaponType = "Rifle";
                    break;
                case 3:
                    weaponType = "Shotgun";
                    break;
                case 4:
                case 5:
                    weaponType = "LightBlade";
                    break;
                default:
                    weaponType = "TemplateWeapon";
                    break;
            }
            // Type t = Type.GetType(weaponType);
            Type t = Type.GetType("Pistol");
            return t;
        }
    }

    public class BadGuy: TemplateNPC
    {
        private TemplateWeapon _weapon 
            = Activator.CreateInstance(base.RandomWeaponType(), 5, 500, 1) as TemplateWeapon;
        private CommonBullet _bullet = new CommonBullet(500, 10, 5);

        public BadGuy(int health, int speed, int weight): base(health, speed, weight)
        {
            this._reviveChance = 1;
        }
    }
}
