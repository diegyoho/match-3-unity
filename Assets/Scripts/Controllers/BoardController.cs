using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class BoardController : SingletonMonoBehaviour<BoardController> {
    public int sizeBoardX = 6;
    public int sizeBoardY = 6;
    public int minMatch = 3;
    public GemBase[, ] gemBoard;
    public GameObject gemPrefab;

    public static Vector3 GetWorldPosition(Vector2Int position) {
        return new Vector2(
            position.x - ((instance.sizeBoardX/2) - 0.5f),
            position.y - ((instance.sizeBoardY/2) - 0.5f)
        );
    }

    public static void CreateBoard() {
        instance.gemBoard = new GemBase[instance.sizeBoardX, instance.sizeBoardY];

        for(int i = 0; i < instance.sizeBoardX; ++i) {
            for(int j = 0; j < instance.sizeBoardY; ++j) {
                GemBase gem = instance.CreateGem(i, j);

                if(GameController.instance.preventInitialMatches)
                    while(GetMatchInfo(gem).isValid) {
                        gem.type = MiscellaneousUtils.Choose((GemType[]) System.Enum.GetValues(typeof(GemType)));
                    }

                instance.StartCoroutine(gem.MoveTo(GetWorldPosition(gem.position), GameController.instance.fallSpeed));
            }
        }
    }

    GemBase CreateGem(int x, int y) {
        return CreateGem(
            x, y,
            GetWorldPosition(new Vector2Int(x, y)) + Vector3.up * (Camera.main.orthographicSize + sizeBoardY/2)
        );
    }

    GemBase CreateGem(int x, int y, Vector3 worldPosition) {

        GemBase gem = Instantiate(
            gemPrefab,
            worldPosition,
            Quaternion.identity,
            transform
        ).GetComponent<GemBase>();

        gem.SetPosition(new Vector2Int(x, y));
        gem.type = MiscellaneousUtils.Choose((GemType[]) System.Enum.GetValues(typeof(GemType)));
        return gem;
    }

    IEnumerator SwapGems(GemBase from, GemBase to) {

        StartCoroutine(from.MoveTo(GetWorldPosition(to.position), GameController.instance.swapSpeed));
        StartCoroutine(to.MoveTo(GetWorldPosition(from.position), GameController.instance.swapSpeed));

        yield return new WaitForSeconds(GameController.instance.swapSpeed);

        Vector2Int fromPosition = from.position;
        from.SetPosition(to.position);
        to.SetPosition(fromPosition);
    }

    public static void TryMatch(GemBase from, GemBase to) {
        instance.StartCoroutine(instance.IETryMatch(from, to));
    }

    IEnumerator IETryMatch(GemBase from, GemBase to) {
        TouchController.cancel = true;
        yield return StartCoroutine(SwapGems(from, to));
        
        if(from.type == to.type) {
            yield return StartCoroutine(SwapGems(from, to));
            TouchController.cancel = false;
            yield break;
        }

        MatchInfo matchFrom = GetMatchInfo(from);
        MatchInfo matchTo = GetMatchInfo(to);

        if(!(matchFrom.isValid || matchTo.isValid)) {
            yield return StartCoroutine(SwapGems(from, to));
        } else {
            List<GemBase> matches = new List<GemBase>(matchFrom.matches);
            matches.AddRange(matchTo.matches);
            
            yield return StartCoroutine(DestroyGems(matches));
            yield return StartCoroutine(FallGems(MatchInfo.MergeFallPositions(
                matchFrom.fallPositions,
                matchTo.fallPositions
            )));

            yield return StartCoroutine(FindChainMatches());
        }
        
        TouchController.cancel = false;
    }

    IEnumerator FindChainMatches() {
        List<GemBase> gems = MiscellaneousUtils.GetList(gemBoard);
        List<MatchInfo> matchInfos = new List<MatchInfo>();

        while(gems.Count > 0) {
            GemBase current = gems[0];
            gems.Remove(current);
            
            MatchInfo matchInfo = GetMatchInfo(current);
            if(matchInfo.isValid) {
                matchInfo.matches.ForEach( gem => gems.Remove(gem));
                matchInfos.Add(matchInfo);
            }
        }

        if(matchInfos.Count > 0) {
            
            List<Vector3Int> fallPositions = new List<Vector3Int>();
            List<GemBase> matchesToDestroy = new List<GemBase>();
            foreach(MatchInfo matchInfo in matchInfos) {
                matchesToDestroy.AddRange(matchInfo.matches);
                fallPositions = MatchInfo.MergeFallPositions(fallPositions, matchInfo.fallPositions);
            }

            yield return StartCoroutine(DestroyGems(matchesToDestroy));
            yield return StartCoroutine(FallGems(fallPositions));
        } else {
            yield break;
        }

        StartCoroutine(FindChainMatches());
    }

    IEnumerator FallGems(List<Vector3Int> fallPositions) {

        foreach(Vector3Int fall in fallPositions) {
            for(int y = fall.y + fall.z; y < instance.sizeBoardY && instance.gemBoard[fall.x, y]; ++y) {
                GemBase gem = instance.gemBoard[fall.x, y];
                StartCoroutine(gem.MoveTo(
                    GetWorldPosition(new Vector2Int(fall.x, y - fall.z)),
                    fall.z * GameController.instance.swapSpeed/1.5f
                ));
                gem.SetPosition(new Vector2Int(fall.x, y - fall.z));
            }
            for(int i = fall.z; i > 0; --i) {
                GemBase gem = instance.CreateGem(
                    fall.x, instance.sizeBoardY - i,
                    GetWorldPosition(new Vector2Int(
                        fall.x, instance.sizeBoardY - i - (instance.sizeBoardY - fall.z)
                    )) + Vector3.up * (Camera.main.orthographicSize + sizeBoardY/2)
                );
                StartCoroutine(gem.MoveTo(GetWorldPosition(gem.position), GameController.instance.fallSpeed));
            }
        }
        yield return new WaitForSeconds(GameController.instance.fallSpeed);
    }
    
    List<GemBase> GetHorizontalMatches(GemBase gem) {
        
        List<GemBase> matches = new List<GemBase>();
        
        int id = gem.position.x - 1;

        while(id >= 0 && gemBoard[id, gem.position.y] && gemBoard[id, gem.position.y].type == gem.type) {
            matches.Add(gemBoard[id, gem.position.y]);
            id--;
        }

        id = gem.position.x + 1;

        while(id < sizeBoardX && gemBoard[id, gem.position.y] && gemBoard[id, gem.position.y].type == gem.type) {
            matches.Add(gemBoard[id, gem.position.y]);
            id++;
        }

        return matches;
    }

    List<GemBase> GetVerticalMatches(GemBase gem) {
        
        List<GemBase> matches = new List<GemBase>();
        
        int id = gem.position.y - 1;

        while(id >= 0 && gemBoard[gem.position.x, id] && gemBoard[gem.position.x, id].type == gem.type) {
            matches.Add(gemBoard[gem.position.x, id]);
            id--;
        }

        id = gem.position.y + 1;

        while(id < sizeBoardY && gemBoard[gem.position.x, id] && gemBoard[gem.position.x, id].type == gem.type) {
            matches.Add(gemBoard[gem.position.x, id]);
            id++;
        }

        return matches;
    }

    public static MatchInfo GetMatchInfo(GemBase gem) {
        
        List<GemBase> matches = new List<GemBase>();
        
        List<GemBase> horizontalMatches = instance.GetHorizontalMatches(gem);
        List<GemBase> verticalMatches = instance.GetVerticalMatches(gem);

        MatchInfo matchInfo = new MatchInfo();
        
        int crossCheck = 0;
        do {
            if(horizontalMatches.Count + 1 >= instance.minMatch) {
                matchInfo.type = MatchType.Horizontal;
                
                matchInfo.startHorizontalPosition = gem.position;
                horizontalMatches.ForEach(g => {
                    if(g.position.x < matchInfo.startHorizontalPosition.x)
                        matchInfo.startHorizontalPosition = g.position;
                });

                matchInfo.horizontalLenght = horizontalMatches.Count + 1;
                matchInfo.startVerticalPosition = matchInfo.startHorizontalPosition;
                matchInfo.verticalLenght = 1;

                matchInfo.matches.AddRange(horizontalMatches);
            } else if (verticalMatches.Count + 1 >= instance.minMatch) {
                horizontalMatches = instance.GetHorizontalMatches(verticalMatches[crossCheck]);
            } else {
                break;
            }
            crossCheck++;
        } while(!matchInfo.isValid && crossCheck < verticalMatches.Count);
        
        crossCheck = 0;
        do {
            if(verticalMatches.Count + 1 >= instance.minMatch) {
                matchInfo.type = matchInfo.type == MatchType.Horizontal ? MatchType.Both : MatchType.Vertical;
                
                matchInfo.startVerticalPosition = gem.position;
                verticalMatches.ForEach(g => {
                    if(g.position.y < matchInfo.startVerticalPosition.y)
                        matchInfo.startVerticalPosition = g.position;
                });

                matchInfo.verticalLenght = verticalMatches.Count + 1;
                
                if(matchInfo.type == MatchType.Vertical) {
                    matchInfo.startHorizontalPosition = matchInfo.startVerticalPosition;
                    matchInfo.horizontalLenght = 1;
                }
                
                matchInfo.matches.AddRange(verticalMatches);
            } else if (horizontalMatches.Count + 1 >= instance.minMatch) {
                verticalMatches = instance.GetVerticalMatches(horizontalMatches[crossCheck]);
            } else {
                break;
            }
            crossCheck++;
        } while((!matchInfo.isValid || matchInfo.type == MatchType.Horizontal) && crossCheck < horizontalMatches.Count);
        
        if(matchInfo.isValid) {
            matchInfo.matches.Add(gem);
            matchInfo.CalcFallPositions();
        }

        return matchInfo;
    }

    IEnumerator DestroyGems(List<GemBase> matches) {
        foreach(GemBase gem in matches) {
            gem.StopAllCoroutines();
            instance.gemBoard[gem.position.x, gem.position.y] = null;
            gem.GetComponent<SpriteRenderer>().sortingOrder = 1;
            gem.transform.localScale *= 1.2f;
        }
        yield return new WaitForSeconds(GameController.instance.swapSpeed);
        foreach(GemBase gem in matches) {
            StartCoroutine(gem.MoveTo(
                new Vector3(
                    gem.transform.position.x,
                    -(Camera.main.orthographicSize + .5f) + gem.transform.position.y - sizeBoardY/2
                ),
                GameController.instance.fallSpeed
            ));
            Destroy(gem.gameObject, GameController.instance.fallSpeed);
        }
    }
}
