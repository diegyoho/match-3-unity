using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITouchHandler {
    
    void TouchDown();
    void TouchDrag();
    void TouchUp();
}