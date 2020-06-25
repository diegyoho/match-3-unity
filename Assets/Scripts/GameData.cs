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
    Flower,
    Special
}

[System.Serializable]
public class GemData {
    public GemType type;
    public Sprite sprite;
    public int minMatch = 3;
}

[System.Serializable]
public class SpecialGemData {
    public string name;
    public GameObject prefab;
}

[System.Serializable]
public class AudioClipInfo {
    public string name;
    public AudioClip clip;
}

[CreateAssetMenu(fileName = "GameData", menuName = "Match3/GameData", order = 1)]
public class GameData : SingletonScriptableObject<GameData> {
    
    [SerializeField]
    List<GemData> gems = new List<GemData>();
    [SerializeField]
    List<SpecialGemData> specialGems = new List<SpecialGemData>();
    [SerializeField]
    List<AudioClipInfo> audioClipInfos = new List<AudioClipInfo>();
    [SerializeField]
    string[] comboMessages;
    public static int maxCombo {
        get { return instance.comboMessages.Length; }
    }

    public static GemData GemOfType(GemType type) {
        return instance.gems.Find(gem => gem.type == type);
    }

    public static GemData RandomGem() {
        return Miscellaneous.Choose(instance.gems);
    }

    public static GameObject GetSpecialGem(string name) {
        SpecialGemData sgd = instance.specialGems.Find(gem => gem.name == name);
        if(sgd != null)
            return sgd.prefab;

        return null;
    }

    public static AudioClip GetAudioClip(string name) {
        AudioClipInfo audioClipInfo = instance.audioClipInfos.Find(
            aci => aci.name == name
        );

        if(audioClipInfo != null)
            return audioClipInfo.clip;

        return null;
    }

    public static string GetComboMessage(int combo) {
        return instance.comboMessages[combo];
    }
}
