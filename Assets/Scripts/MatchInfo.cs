using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MatchType {
    Invalid,
    Horizontal,
    Vertical,
    Both
}

[System.Serializable]
public class MatchInfo {

    public MatchType type;

    public bool isValid {
        get { return type != MatchType.Invalid; }
    }

    public List<GemBase> matches = new List<GemBase>();

    public Vector2Int startHorizontalPosition;
    public Vector2Int startVerticalPosition;

    public int horizontalLenght;
    public int verticalLenght;
}
