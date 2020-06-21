using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MatchType {
    Invalid,
    Horizontal,
    Vertical,
    Both
}

public class MatchInfo {

    public MatchType type;

    public bool isValid {
        get { return type != MatchType.Invalid; }
    }

    public List<GemBase> matches = new List<GemBase>();

    // Posição (x, y), Altura (z)
    public List<Vector3Int> fallPositions = new List<Vector3Int>();

    public Vector2Int startHorizontalPosition;
    public Vector2Int startVerticalPosition;

    public int horizontalLenght;
    public int verticalLenght;

    public void CalcFallPositions() {

        for(int x = startHorizontalPosition.x; x < startHorizontalPosition.x + horizontalLenght; ++x) {
            
            bool isVertical = type != MatchType.Horizontal && x == startVerticalPosition.x;

            int y = isVertical ? startVerticalPosition.y : startHorizontalPosition.y;

            int height = isVertical ? verticalLenght : 1;

            fallPositions.Add(new Vector3Int(x, y, height));
        }
    }

    public static List<Vector3Int> MergeFallPositions(MatchInfo matchA, MatchInfo matchB) {
        
        if(!matchA.isValid && !matchB.isValid )
            return new List<Vector3Int>();
        else if(!matchA.isValid)
            return matchB.fallPositions;
        else if(!matchB.isValid)
            return matchA.fallPositions;

        List<Vector3Int> fallA = matchA.fallPositions;
        List<Vector3Int> fallB = matchB.fallPositions;
        
        if(fallA.Count == 0)
            return fallB;

        fallB.ForEach(fB => {
            
            int id = fallA.FindIndex(fA => fA.x == fB.x);

            if(id >= 0) {
                int diffY = fB.y - fallA[id].y;

                if(Mathf.Abs(diffY) == 1) {
                    fallA[id] = new Vector3Int(
                        fallA[id].x,
                        (int) Mathf.Min(fallA[id].y, fB.y),
                        fallA[id].z + fB.z
                    );
                } else {
                    if(diffY > 0) {

                        fB = new Vector3Int(
                            fB.x,
                            fB.y - fallA[id].z,
                            fB.z + fallA[id].z
                        );

                        fallA.Add(fB);

                    } else {

                        fallA.Insert(0, fB);

                        fallA[id] = new Vector3Int(
                            fallA[id].x,
                            fallA[id].y - fB.z,
                            fallA[id].z + fB.z
                        );
                    }
                }
            } else {
                Debug.Log("Aqui!");
                if(fB.x < fallA[0].x)
                    fallA.Insert(0, fB);
                else
                    fallA.Add(fB);
            }
        });
        
        return fallA;
    }
}
