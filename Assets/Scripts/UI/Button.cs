using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Button : MonoBehaviour {
    
    public TextMeshProUGUI buttonName;

    public void Pressed() {
        buttonName.rectTransform.offsetMin = new Vector2(
            buttonName.rectTransform.offsetMin.x, -20
        );
    }

    public void Released() {
        buttonName.rectTransform.offsetMin = new Vector2(
            buttonName.rectTransform.offsetMin.x, 10
        );
    }
}
