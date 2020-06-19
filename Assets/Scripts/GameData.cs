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
}

[CreateAssetMenu(fileName = "GameData", menuName = "Match3/GameData", order = 1)]
public class GameData : ScriptableObject {
    public List<GemData> gems = new List<GemData>();

    public Sprite GetGemSprite(GemType gemType) {
        return gems.Find(gem => gem.type == gemType).sprite;
    }
}
