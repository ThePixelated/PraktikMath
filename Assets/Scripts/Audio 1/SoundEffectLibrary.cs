using System.Collections.Generic;
using UnityEngine;

public class SoundEffectLibrary : MonoBehaviour
{
    [SerializeField] private SoundEfectGroup[] soundEffectGroups;
    private Dictionary<string, List<AudioClip>> soundDictionary;

    private void Awake()
    {
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        soundDictionary = new Dictionary<string, List<AudioClip>>();
        foreach (var soundEffectGroup in soundEffectGroups)
        {
            soundDictionary[soundEffectGroup.Name] = soundEffectGroup.AudioClips;
        }
    }

    public AudioClip GetRandomClip(string name)
    {
        if (soundDictionary.ContainsKey(name))
        {
            List<AudioClip> audioclip = soundDictionary[name];
            if (audioclip.Count > 0)
            {
                return audioclip[Random.Range(0, audioclip.Count)];
            }
        }

        return null;
    }
}

[System.Serializable]
public class SoundEfectGroup
{
    public string Name;
    public List<AudioClip> AudioClips;

}
