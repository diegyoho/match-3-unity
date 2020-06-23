using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITouchable {
    
    void TouchDown();
    void TouchDrag();
    void TouchUp();
}