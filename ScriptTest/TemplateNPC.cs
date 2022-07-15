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
        public int Speed { get; private set; } = 10;
        public int Weight { get; private set; } = 50;

        protected int _reviveChance = 0;
        protected int _initHealth = 100;
        protected bool _isDead = false;

        public TemplateNPC() {}
        public TemplateNPC(int health, int speed, int weight, int revive)
        {
            Health = health;
            Speed = speed;
            Weight = weight;
            this._initHealth = health;
            this._reviveChance = revive;
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
                // tell Unity remove this object
            } else
            {
                this._isDead = false;
                this._reviveChance --;
                Health = this._initHealth;
                Console.WriteLine($"I'm back with {this._initHealth} HP. I can do this all day.");
            }
        }

        protected static TemplateWeapon RandomWeaponType()
        {
            Random dice = new Random();
            switch (dice.Next(6))
            {
                case 0:
                case 1:
                    return new Pistol();
                case 2:
                    return new Rifle();
                case 3:
                    return new Shotgun();
                case 4:
                case 5:
                    return new Grenade();
                default:
                    return new TemplateWeapon();
            }
        }

        protected static TemplateAmmo RandomAmmoType()
        {
            Random dice = new Random();
            switch (dice.Next(4))
            {
                case 0:
                case 1:
                    return new CommonBullet();
                case 2:
                    return new LaserBeam();
                case 3:
                    return new ExplosivePayload();
                default:
                    return new TemplateAmmo();
            }
        }
    }

    public class BadGuy: TemplateNPC
    {
        private int _seekRange = 20;
        private TemplateWeapon _weapon = TemplateNPC.RandomWeaponType();
        private TemplateAmmo _bullet = TemplateNPC.RandomAmmoType();

        public BadGuy(): this(50, 10, 50, 0) {}
        public BadGuy(int health, int speed, int weight, int revive): base(health, speed, weight, revive)
        {
            // Do Something special here if necessary
        }

        public void NPCShoot(int[] enemyPosition)
        {
            if (!this._isDead)
            {
                _weapon.WeaponInfo();
                _weapon.ShootBullet(ref _bullet);
                // need to control aim according to input enemy position{x,y,z}
                // transform the angle of weapon object
                // and call weapon.ShootBullet to fire bullet(s)
            }
        }

        public void SeekEnemy(int[] enemyPosition)
        {
            // take enemy position{x,y,z} as input
            // compute the straight distance between enemy and this
            // move to ideal distance (_seekDistance), accept tolerance +-5
        }
    }

    public class GoodGuy: TemplateNPC
    {
        public GoodGuy(): this(100, 15, 40, 3) {}
        public GoodGuy() (int health, int speed, int weight, int revive): base(health, speed, weight, revive)
        {
            // Do Something special here if necessary
        }
    }
}
