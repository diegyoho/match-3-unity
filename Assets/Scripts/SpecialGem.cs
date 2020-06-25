using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public abstract class SpecialGem : BaseGem {
    
    public abstract Func<BaseGem, bool> validateGem {
        get;
    }
}
