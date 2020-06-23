using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

[System.Serializable]
public class HintInfo {
    public GemBase gem;
    public List<GemBase> swaps = new List<GemBase>();

    public HintInfo(GemBase gem) {
        this.gem = gem;
    }
}

public class HintController : SingletonMonoBehaviour<HintController> {

    public List<HintInfo> hints = new List<HintInfo>();
    
    HintInfo GetHint(GemBase gem, GemBase otherGem) {
        if(!(gem && otherGem))
            return null;

        HintInfo hintInfo = null;

        HintInfo hintA = hints.Find(h => h.gem == gem);
        HintInfo hintB = hints.Find(h => h.gem == otherGem);

        BoardController.SwapGems(gem, otherGem);

        MatchInfo matchA = BoardController.GetCrossMatch(gem);
        MatchInfo matchB = BoardController.GetCrossMatch(otherGem);

        if(matchA.isValid) {
            hintInfo = hintA != null ? hintA : new HintInfo(gem);
            hintInfo.swaps.Add(otherGem);
        } else if(matchB.isValid) {
            hintInfo = hintB != null ? hintB : new HintInfo(otherGem);
            hintInfo.swaps.Add(gem);
        }

        BoardController.SwapGems(gem, otherGem);

        return hintInfo;
    }

    public static bool FindHints() {
        instance.hints.Clear();

        for(int j = 0; j < BoardController.height; ++j) {
            for(int i = 0; i < BoardController.width; ++i) {
                GemBase gem = BoardController.GetGem(i, j);
                
                // Swap Right
                GemBase otherGem = BoardController.GetGem(i + 1, j);
                if(otherGem && otherGem.type != gem.type) {
                    HintInfo hintInfo = instance.GetHint(gem, otherGem);

                    if(hintInfo != null && !instance.hints.Contains(hintInfo))
                        instance.hints.Add(hintInfo);
                }

                // Swap Right
                otherGem = BoardController.GetGem(i, j + 1);
                if(otherGem && otherGem.type != gem.type) {
                    HintInfo hintInfo = instance.GetHint(gem, otherGem);

                    if(hintInfo != null && !instance.hints.Contains(hintInfo))
                        instance.hints.Add(hintInfo);
                }
            }
        }

        return instance.hints.Count > 0;
    }
}
