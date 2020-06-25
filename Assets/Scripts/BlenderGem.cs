using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Special Gem (Blender) Match all Gems in same Row and Column
// Future improvement create generic class for Special Gems
public class BlenderGem : BaseGem {

    List<BlenderGem> blenders = new List<BlenderGem>();

    public bool activated;
    
    public override GemType type {
        get { return GemType.Special; }
    }

    public override int minMatch {
        get { return 0; }
    }

    public override Func<BaseGem, bool> validateGem {
        get {
            return (gem) => {
                if(gem is BlenderGem) {
                    if(!(gem as BlenderGem).activated) {
                        (gem as BlenderGem).activated = true;
                        blenders.Add(gem as BlenderGem);
                    }
                    
                    return false;
                }

                return true;
            };
        }
    }

    void OnEnable() {
        BoardController.EndUpdatingBoard += ResetSpecialGem;
    }


    void OnDisable() {
        BoardController.EndUpdatingBoard -= ResetSpecialGem;
    }

    public override MatchInfo GetMatch() {
        MatchInfo matchInfo = BoardController.GetCrossMatch(this, validateGem);
        List<MatchInfo> matchInfosChain = new List<MatchInfo>();
        
        activated = true;

        foreach(var blender in blenders) {
            MatchInfo blenderChain = blender.GetMatch();
            blenderChain.RemoveMatches(matchInfo.matches);
            matchInfosChain.ForEach(m => blenderChain.RemoveMatches(m.matches));
            matchInfosChain.Add(blenderChain);
            matchInfosChain.AddRange(blenderChain.specialMatches);
        }

        matchInfo.specialMatches.AddRange(matchInfosChain);
        
        return matchInfo;
    }

    public void ResetSpecialGem() {
        blenders.Clear();
        activated = false;
    }
}
