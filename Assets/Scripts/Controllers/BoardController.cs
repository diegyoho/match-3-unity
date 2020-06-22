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

                gem.StartCoroutine(gem.MoveTo(GetWorldPosition(gem.position), GameController.instance.fallSpeed));
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

        from.StartCoroutine(from.MoveTo(GetWorldPosition(to.position), GameController.instance.swapSpeed));
        to.StartCoroutine(to.MoveTo(GetWorldPosition(from.position), GameController.instance.swapSpeed));

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
            List<GemBase> matches = new List<GemBase>();
            if(matchFrom.isValid)
                matches.AddRange(matchFrom.matches);
            if(matchTo.isValid)
                matches.AddRange(matchTo.matches);
            
            yield return StartCoroutine(DestroyGems(matches));
            // yield return StartCoroutine(FallGems(MatchInfo.MergeFallPositions(
            //     matchFrom.fallPositions,
            //     matchTo.fallPositions
            // )));

            // yield return StartCoroutine(UpdateBoard());
        }
        
        TouchController.cancel = false;
    }

    IEnumerator UpdateBoard() {
        yield return StartCoroutine(FindChainMatches());
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
                // fallPositions = MatchInfo.MergeFallPositions(fallPositions, matchInfo.fallPositions);
            }

            yield return StartCoroutine(DestroyGems(matchesToDestroy));
            yield return StartCoroutine(FallGems(fallPositions));
        } else {
            yield break;
        }

        StartCoroutine(FindChainMatches());
    }

    IEnumerator FallGems(List<Vector3Int> fallPositions) {
        Debug.Log($"COUNT: {fallPositions.Count}");
        foreach(Vector3Int fall in fallPositions) {
            for(int y = fall.y + fall.z; y < instance.sizeBoardY && instance.gemBoard[fall.x, y]; ++y) {
                GemBase gem = instance.gemBoard[fall.x, y];
                gem.StartCoroutine(gem.MoveTo(
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
                gem.StartCoroutine(gem.MoveTo(GetWorldPosition(gem.position), GameController.instance.fallSpeed));
            }
        }
        yield return new WaitForSeconds(GameController.instance.fallSpeed);
    }
    
    MatchInfo GetHorizontalMatch(GemBase gem) {
        
        List<GemBase> matches = new List<GemBase>();
        
        matches.Add(gem);

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

        return new MatchInfo(matches);
    }

    MatchInfo GetVerticalMatch(GemBase gem) {
        
        List<GemBase> matches = new List<GemBase>();
        
        matches.Add(gem);
        
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

        return new MatchInfo(matches);
    }

    public static MatchInfo GetMatchInfo(GemBase gem) {
        
        List<GemBase> matches = new List<GemBase>();
        
        MatchInfo horizontal = instance.GetHorizontalMatch(gem);
        MatchInfo vertical = instance.GetVerticalMatch(gem);

        MatchInfo matchInfo = new MatchInfo();
        
        int crossCheck = 0;
        while(!horizontal.isValid && crossCheck < vertical.matches.Count) {
            if (vertical.isValid) {
                horizontal = instance.GetHorizontalMatch(vertical.matches[crossCheck]);
            } else {
                break;
            }
            crossCheck++;
        }
        
        crossCheck = 0;
        while(!vertical.isValid && crossCheck < horizontal.matches.Count) {
            if (horizontal.isValid) {
                vertical = instance.GetVerticalMatch(horizontal.matches[crossCheck]);
            } else {
                break;
            }
            crossCheck++;
        }

        MatchInfo cross = MatchInfo.JoinMatches(horizontal, vertical);

        if(!cross.isValid)
            if(horizontal.isValid) return horizontal;
            else return vertical;

        return cross;
    }

    public IEnumerator ShuffleBoard() {
        gemBoard = MiscellaneousUtils.ShuffleMatrix(gemBoard);

        for(int i = 0; i < instance.sizeBoardX; ++i) {
            for(int j = 0; j < instance.sizeBoardY; ++j) {
                gemBoard[i, j].SetPosition(new Vector2Int(i, j));
                StartCoroutine(gemBoard[i, j].MoveTo(
                    GetWorldPosition(gemBoard[i, j].position),
                    GameController.instance.fallSpeed
                ));
            }
        }
        yield return new WaitForSeconds(GameController.instance.fallSpeed * 2);
    }

    void FindHints() {
        for(int i = 0; i < instance.sizeBoardX; ++i) {
            for(int j = 0; j < instance.sizeBoardY; ++j) {
                GemBase gem =  gemBoard[i, j];
                Vector2Int originalPosition = gem.position;
                
                gem.position += Vector2Int.right;
                MatchInfo matchInfoRight = GetMatchInfo(gem);

                gem.position = originalPosition + Vector2Int.up;
                MatchInfo matchInfoUp = GetMatchInfo(gem);

                gemBoard[i, j].SetPosition(originalPosition);
            }
        }
    }

    IEnumerator DestroyGems(List<GemBase> matches) {
        foreach(GemBase gem in matches) {
            gem.StopAllCoroutines();
            instance.gemBoard[gem.position.x, gem.position.y] = null;
            gem.GetComponent<SpriteRenderer>().sortingOrder = 1;
            gem.transform.localScale *= 1.2f;
        }
        yield return new WaitForSeconds(GameController.instance.swapSpeed);
        // foreach(GemBase gem in matches) {
        //     gem.StartCoroutine(gem.MoveTo(
        //         new Vector3(
        //             gem.transform.position.x,
        //             -(Camera.main.orthographicSize + .5f) + gem.transform.position.y - sizeBoardY/2
        //         ),
        //         GameController.instance.fallSpeed
        //     ));
        //     Destroy(gem.gameObject, GameController.instance.fallSpeed);
        // }
    }
}
