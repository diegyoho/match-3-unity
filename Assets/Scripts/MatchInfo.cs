using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum MatchType {
    Invalid,
    Horizontal,
    Vertical,
    Cross
}

public class MatchInfo {
    Vector2Int minPosition;
    Vector2Int maxPosition;

    int horizontalLenght {
        get { return (maxPosition.x - minPosition.x) + 1; }
    }

    int verticalLenght {
        get { return (maxPosition.y - minPosition.y) + 1; }
    }

    public MatchType type;

    public bool isValid {
        get { return type != MatchType.Invalid; }
    }

    List<GemBase> _matches = new List<GemBase>();
    public List<GemBase> matches {
        get { return _matches; }
    }

    Vector2Int _pivot;
    public GemBase pivot {
        get {
            if(type == MatchType.Cross)
                return matches.Find(gem => gem.position == _pivot);
            else
                return matches[0];
        }
    }

    public MatchInfo(List<GemBase> matches = null) {
        if(matches != null)
            AddMatches(matches);
    }

    public bool TypeIs(MatchType type) {
        return (this.type & type) != MatchType.Invalid;
    }

    void AddMatches(List<GemBase> matches) {
        _matches.AddRange(matches);
        CalcBoundaries();
    }

    void CalcBoundaries() {
        type = MatchType.Invalid;
        minPosition = maxPosition = pivot.position;

        foreach(GemBase match in matches) {
            int x = minPosition.x;
            int y = minPosition.y;

            if(match.position.x < minPosition.x)
                x = match.position.x;
            if(match.position.y < minPosition.y)
                y = match.position.y;
            
            minPosition = new Vector2Int(x, y);

            x = maxPosition.x;
            y = maxPosition.y;
            
            if(match.position.x > maxPosition.x)
                x = match.position.x;
            if(match.position.y > maxPosition.y)
                y = match.position.y;
            
            maxPosition = new Vector2Int(x, y);
            
            if(!TypeIs(MatchType.Horizontal) && horizontalLenght >= BoardController.instance.minMatch) {
                type |= MatchType.Horizontal;
            }

            if(!TypeIs(MatchType.Vertical) && verticalLenght >= BoardController.instance.minMatch)
                type |= MatchType.Vertical;
        }
    }

    public static MatchInfo JoinMatches(MatchInfo a, MatchInfo b) {

        if(!(a.isValid && b.isValid) || a.pivot.type != a.pivot.type) {
            return new MatchInfo();
        }

        a.matches.ForEach(match => {
            if(b.matches.Contains(match)) {
                a._pivot = match.position;
                b.matches.Remove(match);
            }
        });

        a.AddMatches(b.matches);

        return a;
    }

    public List<Vector3Int> GetFallPositions() {
        List<Vector3Int> fallPositions = new List<Vector3Int>();



        return fallPositions;
    }
}
