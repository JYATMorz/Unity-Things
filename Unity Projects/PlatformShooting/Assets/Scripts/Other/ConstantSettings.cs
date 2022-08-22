using UnityEngine;

public class ConstantSettings
{
    // Tags for character statuses management
    public const string bulletTag = "Bullet";
    public const string deadTag = "Dead";
    public const string neutralTag = "Neutral";
    public const string blueTeamTag = "BlueTeam";
    public const string redTeamTag = "RedTeam";
    public const string floorTag = "Floor";
    public const string elevatorTag = "Elevator";

    // Color suffixes for notification text
    public const string blueColor = "<#B4BED1>";
    public const string redColor = "<#D0BDC7>";
    public const string purpleColor = "<#C6A2D1>";
    public const string whiteColor = "<#FFFFFF>";

    // Tags for ammo types management & ammo launching audios management
    public const string commonTag = "CommonBullet";
    public const string laserTag = "LaserBeam";
    public const string grenadeTag = "GrenadeLauncher";
    public const string explosiveTag = "ExplosivePayload";

    // Tags for audio management (haven't been reused)
    public const string jumpTag = "Jump";
    public const string explodeTag = "Explode";
    public const string hurtTag = "Hurt";
    public const string reviveTag = "Revive";
    public const string teleportTag = "Teleport";
    public const string themeTag = "Theme";
    public const string endTag = "GameOver";

    // Const number variables for character setting
    public const int barrelRotateSpeed = 45;
    public const int initHealth = 100;
    public const int shootRange = 7;
    public const int seekRange = 12;
    public const float seekInterval = 1f;
    public const float speedScalar = 5f;
    public const float speedOnNav = 3.5f;
    public const float jumpScalar = 20f;

    public const int ammoSelfDestruction = 10;

    public static readonly Vector3 rightIdlePosition = new (44.5f, 14.75f, 0);
    public static readonly Vector3 leftIdlePosition = new (4f, 8.5f, 0);
    public static readonly int floorLayer = LayerMask.GetMask(floorTag, elevatorTag);
    public static readonly int characterLayer = LayerMask.GetMask(deadTag, neutralTag, redTeamTag, blueTeamTag);
    public static readonly int deadLayer = LayerMask.NameToLayer(deadTag);
    public static readonly string[] floorTags = { floorTag, elevatorTag };
    public static readonly string[] aliveTags = { neutralTag, redTeamTag, blueTeamTag };
    public static readonly string[] characterTags = { deadTag, neutralTag, redTeamTag, blueTeamTag };
    public static readonly string[] tipsText = {
        "<b>You can climb the wall !</b>\n - Try to keep pressing the space bar when on it.",
        "<b>Kill a yellow (neutral) character and turn it into your teammate.</b>",
        "<b>You will \"respawn\" after being killed !\n - As long as you have a teammate.</b>",
        "<b>Use the scroll wheel or number buttons or click the button to switch weapons.</b>",
        "<b>Make good use of teleportation and elevators!</b>\n - NPCs are not skilled at those (maybe :p)."
    };

    public static bool TargetInRange(Vector3 targetPos, Vector3 currentPos, float range)
        => (targetPos - currentPos).sqrMagnitude < range * range;

    public static bool ObstacleBetween(Vector3 targetPos, Vector3 currentPos, int layer = -1)
    {
        if (layer == -1) layer = floorLayer;
        return Physics.Linecast(currentPos, targetPos, layer);
    }

    public static int ExplosionDamage(int damage, Vector3 explodePos, Vector3 targetPos, float radius)
    {
        return Mathf.FloorToInt(damage * (1 - (explodePos - targetPos).sqrMagnitude / (radius * radius)));
    }

    public static bool AreBothNeutral(GameObject contact, Rigidbody owner)
    {
        return contact.CompareTag(neutralTag) && owner.CompareTag(neutralTag);
    }

    public static bool AreBothNeutral(Collider contact, Rigidbody owner)
    {
        return contact.CompareTag(neutralTag) && owner.CompareTag(neutralTag);
    }
}
