using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Special Item Match all Gems in same Row and Column
public class BlenderGem : SpecialGem {
    
    public override Func<BaseGem, bool> validateGem {
        get { return _ => true; }
    }

    public override int minMatch {
        get { return 0; }
    }
}
