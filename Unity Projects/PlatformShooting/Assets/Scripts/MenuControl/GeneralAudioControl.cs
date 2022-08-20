using UnityEngine;
using System.Collections.Generic;

public class GeneralAudioControl : MonoBehaviour
{
    public static GeneralAudioControl Instance { get; private set; } = null;

    public AudioData[] audioClips;

    private readonly Dictionary<string, AudioData> _audioInfos = new();

    void Awake()
    {
        if (Instance == null) Instance = this;

        for (int i = 0; i < audioClips.Length; i ++)
        {
            if (audioClips[i].isLoop)
            {
                audioClips[i].source = gameObject.AddComponent<AudioSource>();

                audioClips[i].source.clip = audioClips[i].clip;
                audioClips[i].source.volume = audioClips[i].volume;
                audioClips[i].source.loop = audioClips[i].isLoop;
                audioClips[i].source.playOnAwake = audioClips[i].onAwake;
            }

            _audioInfos.Add(audioClips[i].tag, audioClips[i]);
        }
    }

    public void PlayAudio(string audioTag, float wait = float.NaN)
    {
        if (!_audioInfos.TryGetValue(audioTag, out AudioData audio))
        {
            Debug.LogWarning("Trying To Use Wrong Audio Tag !");
            return;
        }

        if (audio.isLoop && !audio.source.isPlaying)
        {
            if (audio.source.time != 0f) audio.source.UnPause();
            else if (float.IsNaN(wait)) audio.source.Play();
            else audio.source.PlayDelayed(wait);
        }
        else Debug.LogWarning("Trying To play a non-looping / non-playing audio clip !");
    }

    public void PlayAudio(string audioTag, Vector3 audioPos, float volume = float.NaN)
    {
        if (!_audioInfos.TryGetValue(audioTag, out AudioData audio))
        {
            Debug.LogWarning("Trying To Use Wrong Audio Tag !");
            return;
        }

        if (float.IsNaN(volume)) volume = audio.volume;
        
        if (!audio.isLoop) AudioSource.PlayClipAtPoint(audio.clip, audioPos, volume);
        else Debug.LogWarning("Trying To play a looping audio clip !");
    }

    public void PauseAudio(string audioTag)
    {
        if (!_audioInfos.TryGetValue(audioTag, out AudioData audio))
        {
            Debug.LogWarning("Trying To Use Wrong Audio Tag !");
            return;
        }

        if (audio.isLoop) audio.source.Pause();
        else Debug.LogWarning("Trying To pause a non-looping audio clip !");
    }

    public void StopAudio(string audioTag)
    {
        if (!_audioInfos.TryGetValue(audioTag, out AudioData audio))
        {
            Debug.LogWarning("Trying To Use Wrong Audio Tag !");
            return;
        }
        
        if (audio.isLoop) audio.source.Stop();
        else Debug.LogWarning("Trying To stop a non-looping audio clip !");
    }
}
