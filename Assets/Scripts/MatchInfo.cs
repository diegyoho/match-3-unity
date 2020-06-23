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
        get { return new List<GemBase>(_matches); }
    }

    Vector2Int _pivot;
    public GemBase pivot {
        get {
            if(type == MatchType.Cross)
                return _matches.Find(gem => gem.position == _pivot);
            else
                return _matches[0];
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
        ValidateMatch();
    }

    void ValidateMatch() {
        type = MatchType.Invalid;
        minPosition = maxPosition = pivot.position;

        foreach(GemBase match in _matches) {
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
            
            if(horizontalLenght >= pivot.minMatch)
                type |= MatchType.Horizontal;

            if(verticalLenght >= pivot.minMatch)
                type |= MatchType.Vertical;
        }
    }

    // Join Crossed Matches from same type
    public static MatchInfo JoinCrossedMatches(MatchInfo a, MatchInfo b) {

        if(!(a.isValid && b.isValid) || a.pivot.type != a.pivot.type) {
            return new MatchInfo();
        }

        foreach(GemBase match in a._matches) {
            if(b._matches.Contains(match)) {
                a._pivot = match.position;
                b._matches.Remove(match);
                a.AddMatches(b._matches);

                return a;
            }
        }

        return new MatchInfo();
    }

    // Position(x, y) and Height(z)
    public List<Vector3Int> GetFallPositions() {
        List<Vector3Int> fallPositions = new List<Vector3Int>();

        _matches.ForEach(match => {
            int id = fallPositions.FindIndex(f => f.x == match.position.x);
            if(id > -1) {
                fallPositions[id] = new Vector3Int(
                    match.position.x,
                    (int) Mathf.Min(fallPositions[id].y, match.position.y),
                    fallPositions[id].z + 1
                );
            } else {
                fallPositions.Add(new Vector3Int(
                    match.position.x,
                    match.position.y,
                    1
                ));
            }
        });

        return fallPositions;
    }

    public static List<Vector3Int> JoinFallPositions(List<Vector3Int> matchA, List<Vector3Int> matchB) {
        List<Vector3Int> fallPositions = new List<Vector3Int>();

        if(matchA.Count == 0)
            return matchB;
        else if(matchB.Count == 0)
            return matchA;

        matchA.ForEach(currentFall => {
            int id = matchB.FindIndex(f => f.x == currentFall.x);
            if(id > -1) {
                fallPositions.Add(new Vector3Int(
                    currentFall.x,
                    (int) Mathf.Min(matchB[id].y, currentFall.y),
                    matchB[id].z + currentFall.z
                ));
                matchB.RemoveAt(id);
            } else {
                fallPositions.Add(currentFall);
            }
        });

        fallPositions.AddRange(matchB);

        return fallPositions;
    }
}
