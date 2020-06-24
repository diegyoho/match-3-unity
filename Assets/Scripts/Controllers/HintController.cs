using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

[System.Serializable]
public class HintInfo {
    public GemBase gem;
    public GemBase currentSwap;
    public List<GemBase> swaps = new List<GemBase>();

    public HintInfo(GemBase gem) {
        this.gem = gem;
    }
}

public class HintController : SingletonMonoBehaviour<HintController> {

    List<HintInfo> hints = new List<HintInfo>();
    HintInfo currentHint;
    Coroutine hinting;

    public static bool hasHints {
        get { return instance.hints.Count > 0; }
    }

    public static bool isShowing {
        get { return instance.currentHint != null; }
    }
    
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

    public static void FindHints() {
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
    }

    public static void ShowHint() {
        StopCurrentHint();
        if(hasHints) {
            HintInfo hintInfo = instance.hints[Random.Range(0, instance.hints.Count)];
            hintInfo.gem.Hint();
            hintInfo.currentSwap = hintInfo.swaps[Random.Range(0, hintInfo.swaps.Count)];
            hintInfo.currentSwap.Hint();
            instance.currentHint = hintInfo;
        } else {
            instance.currentHint = null;
        }
    }

    public static void StopCurrentHint() {
        if(instance.currentHint != null) {
            instance.currentHint.gem.Hint(false);
            instance.currentHint.currentSwap.Hint(false);
            instance.currentHint = null;
        }
    }

    public static void StartHinting() {
        if(instance.hinting != null)
            instance.StopCoroutine(instance.hinting);

        instance.hinting = instance.StartCoroutine(instance.IEStartHinting());
    }

    public static void StopHinting() {
        StopCurrentHint();
        if(instance.hinting != null)
            instance.StopCoroutine(instance.hinting);
        
        instance.hinting = null;
    }

    IEnumerator IEStartHinting() {
        
        StopCurrentHint();
        yield return new WaitForSeconds(30f);
        ShowHint();
    }
}
