using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class BoardController : SingletonMonoBehaviour<BoardController> {
    
    [SerializeField]
    int _width = 6;
    public static int width {
        get { return instance._width; }
        set { instance._width = value; }
    }

    [SerializeField]
    int _height = 6;
    public static int height {
        get { return instance._height; }
        set { instance._height = value; }
    }

    public static GemBase[, ] gemBoard;
    public GameObject gemPrefab;

    public static Vector3 GetWorldPosition(Vector2Int position) {
        return new Vector2(
            position.x - ((width/2) - 0.5f),
            position.y - ((height/2) - 0.5f)
        );
    }

    public static Vector3 GetWorldPosition(int x, int y) {
        return GetWorldPosition(new Vector2Int(x, y));
    }

    public static void CreateBoard() {
        gemBoard = new GemBase[width, height];

        for(int i = 0; i < width; ++i) {
            for(int j = 0; j < height; ++j) {
                GemBase gem = instance.CreateGem(i, j);

                if(GameController.instance.preventInitialMatches)
                    while(GetCrossMatch(gem).isValid) {
                        gem.SetType(GameController.gameData.RandomGem());
                    }

                gem.StartCoroutine(gem.MoveTo(GetWorldPosition(gem.position), GameController.instance.fallSpeed));
            }
        }
    }

    GemBase CreateGem(int x, int y, Vector3 worldPosition) {

        GemBase gem = Instantiate(
            gemPrefab,
            worldPosition,
            Quaternion.identity,
            transform
        ).GetComponent<GemBase>();

        gem.SetPosition(new Vector2Int(x, y));
        gem.SetType(GameController.gameData.RandomGem());
        return gem;
    }

    // Create Gem on Top Screen
    GemBase CreateGem(int x, int y) {
        return CreateGem(
            x, y,
            GetWorldPosition(new Vector2Int(x, y)) + Vector3.up * (Camera.main.orthographicSize + 1 + height/2)
        );
    }

    public static GemBase GetGem(int x, int y) {
        if(x < 0 || x >= width || y < 0 || y >= height)
            return null;

        return gemBoard[x, y];
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

        MatchInfo matchFrom = GetCrossMatch(from);
        MatchInfo matchTo = GetCrossMatch(to);

        if(!(matchFrom.isValid || matchTo.isValid)) {
            yield return StartCoroutine(SwapGems(from, to));
        } else {
            List<GemBase> matches = new List<GemBase>();
            List<Vector3Int> fallPositions = new List<Vector3Int>();
            
            if(matchFrom.isValid) {
                matches.AddRange(matchFrom.matches);
                fallPositions.AddRange(matchFrom.GetFallPositions());
            }

            if(matchTo.isValid) {
                matches.AddRange(matchTo.matches);
                fallPositions.AddRange(matchTo.GetFallPositions());
            }
            
            yield return StartCoroutine(DestroyGems(matches));
            yield return StartCoroutine(FallGems(fallPositions));

            yield return StartCoroutine(UpdateBoard());
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
            
            MatchInfo matchInfo = GetCrossMatch(current);
            if(matchInfo.isValid) {
                matchInfo.matches.ForEach( gem => gems.Remove(gem));
                MatchInfo matchInfoSameType = matchInfos.Find(mi => mi.pivot.type == matchInfo.pivot.type);
                if(matchInfoSameType != null)
                    matchInfoSameType = MatchInfo.JoinCrossedMatches(matchInfoSameType, matchInfo);
                else
                    matchInfos.Add(matchInfo);
            }
        }

        if(matchInfos.Count > 0) {
            
            List<Vector3Int> fallPositions = new List<Vector3Int>();
            List<GemBase> matchesToDestroy = new List<GemBase>();
            foreach(MatchInfo matchInfo in matchInfos) {
                matchesToDestroy.AddRange(matchInfo.matches);
                fallPositions.AddRange(matchInfo.GetFallPositions());
            }

            yield return StartCoroutine(DestroyGems(matchesToDestroy));
            yield return StartCoroutine(FallGems(fallPositions));
        } else {
            yield break;
        }

        yield return StartCoroutine(FindChainMatches());
    }

    IEnumerator FallGems(List<Vector3Int> fallPositions) {
        int[] heights = new int[width];
        heights.Populate(0);

        foreach(Vector3Int fall in fallPositions) {
            
            GemBase gem = GetGem(fall.x, fall.y + fall.z);
            while(gem) {
                int y = gem.position.y;
                gem.StartCoroutine(gem.MoveTo(
                    GetWorldPosition(new Vector2Int(fall.x, y - (fall.z + heights[fall.x]))),
                    (fall.z + heights[fall.x]) * GameController.instance.swapSpeed/1.5f
                ));

                gem.SetPosition(new Vector2Int(fall.x, y - (fall.z + heights[fall.x])));
                gem = GetGem(fall.x, y + 1);
            }

            heights[fall.x] += fall.z;
        }
        for(int x = 0; x < heights.Length; ++x) {
            for(int y = height - heights[x]; y < height; ++y) {
                GemBase newGem = instance.CreateGem(
                    x, y,
                    GetWorldPosition(new Vector2Int(
                        x, y - (height - heights[x])
                    )) + Vector3.up * (Camera.main.orthographicSize + height/2)
                );
                newGem.StartCoroutine(newGem.MoveTo(GetWorldPosition(newGem.position), GameController.instance.fallSpeed));
            }
        }
        yield return new WaitForSeconds(GameController.instance.fallSpeed);
    }
    
    public static MatchInfo GetHorizontalMatch(GemBase gem) {
        
        List<GemBase> matches = new List<GemBase>();
        
        matches.Add(gem);

        GemBase gemToCheck = GetGem(gem.position.x - 1, gem.position.y);
        
        while(gemToCheck && gemToCheck.type == gem.type) {
            matches.Add(gemToCheck);
            gemToCheck = GetGem(gemToCheck.position.x - 1, gemToCheck.position.y);
        }

        gemToCheck = GetGem(gem.position.x + 1, gem.position.y);
        
        while(gemToCheck && gemToCheck.type == gem.type) {
            matches.Add(gemToCheck);
            gemToCheck = GetGem(gemToCheck.position.x + 1, gemToCheck.position.y);
        }

        return new MatchInfo(matches);
    }

    public static MatchInfo GetVerticalMatch(GemBase gem) {
        
        List<GemBase> matches = new List<GemBase>();
        
        matches.Add(gem);
        
        GemBase gemToCheck = GetGem(gem.position.x, gem.position.y - 1);
        
        while(gemToCheck && gemToCheck.type == gem.type) {
            matches.Add(gemToCheck);
            gemToCheck = GetGem(gemToCheck.position.x, gemToCheck.position.y - 1);
        }

        gemToCheck = GetGem(gem.position.x, gem.position.y + 1);
        
        while(gemToCheck && gemToCheck.type == gem.type) {
            matches.Add(gemToCheck);
            gemToCheck = GetGem(gemToCheck.position.x, gemToCheck.position.y + 1);
        }

        return new MatchInfo(matches);
    }

    public static MatchInfo GetCrossMatch(GemBase gem) {
        
        List<GemBase> matches = new List<GemBase>();
        
        MatchInfo horizontal = GetHorizontalMatch(gem);
        MatchInfo vertical = GetVerticalMatch(gem);

        MatchInfo matchInfo = new MatchInfo();
        
        int crossCheck = 0;
        while(!horizontal.isValid && crossCheck < vertical.matches.Count) {
            if (vertical.isValid) {
                horizontal = GetHorizontalMatch(vertical.matches[crossCheck]);
            } else {
                break;
            }
            crossCheck++;
        }
        
        crossCheck = 0;
        while(!vertical.isValid && crossCheck < horizontal.matches.Count) {
            if (horizontal.isValid) {
                vertical = GetVerticalMatch(horizontal.matches[crossCheck]);
            } else {
                break;
            }
            crossCheck++;
        }

        MatchInfo cross = MatchInfo.JoinCrossedMatches(horizontal, vertical);

        if(!cross.isValid)
            if(horizontal.isValid) return horizontal;
            else return vertical;

        return cross;
    }

    public IEnumerator ShuffleBoard() {
        gemBoard = MiscellaneousUtils.ShuffleMatrix(gemBoard);

        for(int i = 0; i < width; ++i) {
            for(int j = 0; j < height; ++j) {
                gemBoard[i, j].SetPosition(new Vector2Int(i, j));
                StartCoroutine(gemBoard[i, j].MoveTo(
                    GetWorldPosition(gemBoard[i, j].position),
                    GameController.instance.fallSpeed
                ));
            }
        }
        yield return new WaitForSeconds(GameController.instance.fallSpeed);
        StartCoroutine(FindChainMatches());
    }

    void FindHints() {
        for(int i = 0; i < width; ++i) {
            for(int j = 0; j < height; ++j) {
                GemBase gem =  gemBoard[i, j];
                Vector2Int originalPosition = gem.position;
                
                gem.position += Vector2Int.right;
                MatchInfo matchInfoRight = GetCrossMatch(gem);

                gem.position = originalPosition + Vector2Int.up;
                MatchInfo matchInfoUp = GetCrossMatch(gem);

                gemBoard[i, j].SetPosition(originalPosition);
            }
        }
    }

    IEnumerator DestroyGems(List<GemBase> matches) {
        foreach(GemBase gem in matches) {
            gem.StopAllCoroutines();
            gemBoard[gem.position.x, gem.position.y] = null;
            gem.Matched();
        }
        yield return new WaitForSeconds(GameController.instance.swapSpeed);
    }
}
