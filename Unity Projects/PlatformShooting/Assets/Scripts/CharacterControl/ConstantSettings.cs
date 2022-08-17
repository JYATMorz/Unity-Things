using System;
using UnityEngine;

public class ConstantSettings
{
    public const string bulletTag = "Bullet";
    public const string deadTag = "Dead";
    public const string neutralTag = "Neutral";
    public const string blueTeamTag = "BlueTeam";
    public const string redTeamTag = "RedTeam";
    public const string floorTag = "Floor";
    public const string elevatorTag = "Elevator";

    public const string blueColor = "<#B4BED1>";
    public const string redColor = "<#D0BDC7>";
    public const string purpleColor = "<#C6A2D1>";
    public const string whiteColor = "<#FFFFFF>";

    public const int shootRange = 7;
    public const int seekRange = 12;
    public const float seekInterval = 1f;
    public const float speedScaler = 5f;
    public const float jumpScaler = 20f;
    public const int barrelRotateSpeed = 45;
    public const int initHealth = 100;

    public static readonly int floorLayer = LayerMask.GetMask(floorTag, elevatorTag);
    public static readonly int deadLayer = LayerMask.NameToLayer(deadTag);
    public static readonly string[] floorTags = { floorTag, elevatorTag };

    public static bool TargetInRange(Vector3 targetPos, Vector3 currentPos, float range)
        => (targetPos - currentPos).sqrMagnitude < range * range;

    public static bool ObstacleBetween(Vector3 targetPos, Vector3 currentPos, int layer = -1)
    {
        if (layer == -1) layer = floorLayer;
        return Physics.Linecast(currentPos, targetPos, layer);
    }
}
