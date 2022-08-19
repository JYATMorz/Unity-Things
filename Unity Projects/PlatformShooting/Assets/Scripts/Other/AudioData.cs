using System;
using UnityEngine;

[Serializable]
public struct AudioData
{
    public string tag;
    [Range(0f, 1f)] public float volume;
    public bool isLoop;
    public bool onAwake;
    public AudioClip clip;
    [HideInInspector] public AudioSource source;
}
