using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public enum GemType {
    Milk,
    Apple,
    Orange,
    Bread,
    Lettuce,
    Coconut,
    Flower
}

[System.Serializable]
public class GemData {
    public GemType type;
    public Sprite sprite;
    public int minMatch = 3;
}

[System.Serializable]
public class AudioClipInfo {
    public string name;
    public AudioClip clip;
}

[CreateAssetMenu(fileName = "GameData", menuName = "Match3/GameData", order = 1)]
public class GameData : SingletonScriptableObject<GameData> {
    public List<GemData> gems = new List<GemData>();
    public List<AudioClipInfo> audioClipInfos = new List<AudioClipInfo>();

    public static GemData RandomGem() {
        return Miscellaneous.Choose(instance.gems);
    }

    public static AudioClip GetAudioClip(string name) {
        AudioClipInfo audioClipInfo = instance.audioClipInfos.Find(aci => aci.name == name);

        if(audioClipInfo != null)
            return audioClipInfo.clip;

        return null;
    }
}
