using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class BoardController : SingletonMonoBehaviour<BoardController> {

    Coroutine updateBoard = null;
    
    [Header("Board Dimensions")]
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
    int _matchCounter;
    public static int matchCounter {
        get { return instance._matchCounter; }
        set {
            instance._matchCounter = Mathf.Min(value, GameData.maxCombo);
        }
    }

    public static bool updatingBoard;

    // Calculate Board Position into World
    public static Vector3 GetWorldPosition(Vector2Int position) {
        return new Vector2(
            position.x - ((width/2f) - 0.5f),
            position.y - ((height/2f) - 0.5f)
        );
    }

    public static Vector3 GetWorldPosition(int x, int y) {
        return GetWorldPosition(new Vector2Int(x, y));
    }

    public static float CreateBoard() {
        gemBoard = new GemBase[width, height];
        float maxDuration = 0;
        float delayLine = 0;
        for(int j = height - 1; j >= 0; --j) {
            for(int i = 0; i < width; ++i) {
                GemBase gem = instance.CreateRandomGem(
                    i, j, GetWorldPosition(new Vector2Int(i, j + 1)),
                    delayLine
                );


                if(GameController.instance.preventInitialMatches)
                    while(GetCrossMatch(gem).isValid) {
                        gem.SetType(GameData.RandomGem());
                    }

                float duration = gem.MoveTo(
                    GetWorldPosition(gem.position),
                    GameController.instance.fallSpeed,
                    delayLine
                );

                if(duration > maxDuration)
                    maxDuration = duration;
            }

            delayLine = maxDuration;
        }
        return maxDuration;
    }

    GemBase CreateGem(
        int x, int y, GemData type,
        Vector3 worldPosition, float delay,
        out float creatingDuration
    ) {

        GemBase gem = Instantiate(
            Resources.Load<GameObject>("Prefabs/gemPrefab"),
            worldPosition,
            Quaternion.identity,
            transform
        ).GetComponent<GemBase>();

        gem.SetPosition(new Vector2Int(x, y));
        gem.SetType(type);

        creatingDuration = gem.Creating(delay);

        return gem;
    }

    

    GemBase CreateRandomGem(
        int x, int y, Vector3 worldPosition,
        float delay, out float creatingDuration
    ) {
        return CreateGem(x, y, GameData.RandomGem(), worldPosition, delay, out creatingDuration);
    }

    

    GemBase CreateRandomGem(
        int x, int y, Vector3 worldPosition,
        float delay
    ) {
        return CreateRandomGem(x, y, worldPosition, delay, out float _);
    }

    GemBase CreateRandomGem(
        int x, int y, Vector3 worldPosition
    ) {
        return CreateRandomGem(x, y, worldPosition, 0);
    }

    // Check if position is valid, then returns a Gem
    public static GemBase GetGem(int x, int y) {
        if(x < 0 || x >= width || y < 0 || y >= height)
            return null;

        return gemBoard[x, y];
    }

    public static GemBase GetGem(Vector2Int position) {
        return GetGem(position.x, position.y);
    }

    // Swap position Gems
    public static void SwapGems(GemBase from, GemBase to) {
        Vector2Int fromPosition = from.position;
        from.SetPosition(to.position);
        to.SetPosition(fromPosition);
    }

    IEnumerator IESwapGems(GemBase from, GemBase to) {

        float durationFrom = from.MoveTo(
            GetWorldPosition(to.position), GameController.instance.swapSpeed
        );
        float durationTo = to.MoveTo(
            GetWorldPosition(from.position), GameController.instance.swapSpeed
        );

        yield return new WaitForSeconds(Mathf.Max(durationFrom, durationTo));

        SwapGems(from, to);
    }

    // Check if Swap results in a Match
    public static void TryMatch(GemBase from, GemBase to) {
        instance.StartCoroutine(instance.IETryMatch(from, to));
    }

    IEnumerator IETryMatch(GemBase from, GemBase to) {
        EnableUpdateBoard(true);
        yield return StartCoroutine(IESwapGems(from, to));
        
        if(from.type == to.type) {
            yield return StartCoroutine(IESwapGems(from, to));
            EnableUpdateBoard(false);
            yield break;
        }

        MatchInfo matchFrom = GetCrossMatch(from);
        MatchInfo matchTo = GetCrossMatch(to);

        if(!(matchFrom.isValid || matchTo.isValid)) {
            yield return StartCoroutine(IESwapGems(from, to));
            EnableUpdateBoard(false);
        } else {
            HintController.StopCurrentHint();
            HintController.StopHinting();

            List<MatchInfo> matches = new List<MatchInfo>();
            List<Vector2Int> fallPositions = new List<Vector2Int>();
            
            if(matchFrom.isValid) {
                matches.Add(matchFrom);
                fallPositions = MatchInfo.JoinFallPositions(
                    fallPositions, matchFrom.GetFallPositions()
                );
            }

            if(matchTo.isValid) {
                matches.Add(matchTo);
                fallPositions = MatchInfo.JoinFallPositions(
                    fallPositions, matchTo.GetFallPositions()
                );
            }
            
            yield return StartCoroutine(DestroyMatchedGems(matches));
            yield return StartCoroutine(FallGems(fallPositions));

            UpdateBoard();
        }
    }

    public static void UpdateBoard() {
        if(instance.updateBoard != null)
            instance.StopCoroutine(instance.updateBoard);

        instance.updateBoard = instance.StartCoroutine(instance.IEUpdateBoard());
    }

    IEnumerator IEUpdateBoard() {
        EnableUpdateBoard(true);

        yield return StartCoroutine(FindChainMatches());

        if(GameController.timeLeft <= 0) {
            EnableUpdateBoard(false);
            yield break;
        }

        HintController.FindHints();
        if(!HintController.hasHints) {
            yield return StartCoroutine(ShuffleBoard());
            UpdateBoard();
        } else {
            EnableUpdateBoard(false);
            matchCounter = 0;
            HintController.StartHinting();
        }
    }

    static void EnableUpdateBoard(bool enable) {
        updatingBoard = enable;
        HintController.paused = enable;
        TouchController.cancel = enable;
    }

    // Check for matches in all Board
    IEnumerator FindChainMatches() {
        List<GemBase> gems = gemBoard.GetList();
        List<MatchInfo> matchInfos = new List<MatchInfo>();

        while(gems.Count > 0) {
            GemBase current = gems[0];
            gems.Remove(current);
            
            MatchInfo matchInfo = GetCrossMatch(current);
            if(matchInfo.isValid) {
                matchInfo.matches.ForEach( gem => gems.Remove(gem));
                
                MatchInfo matchInfoSameType = matchInfos.Find(
                    mi => mi.pivot.type == matchInfo.pivot.type
                );

                if(matchInfoSameType != null) {
                    matchInfoSameType = MatchInfo.JoinCrossedMatches(
                        matchInfoSameType, matchInfo
                    );

                    if(matchInfoSameType.isValid) {
                        matchInfos.Add(matchInfoSameType);
                        continue;
                    }
                }

                matchInfos.Add(matchInfo);
            }
        }

        if(matchInfos.Count > 0) {
            
            List<Vector2Int> fallPositions = new List<Vector2Int>();
            List<MatchInfo> matchesToDestroy = new List<MatchInfo>();
            foreach(MatchInfo matchInfo in matchInfos) {
                matchesToDestroy.Add(matchInfo);
                fallPositions = MatchInfo.JoinFallPositions(
                    fallPositions, matchInfo.GetFallPositions()
                );
            }

            yield return StartCoroutine(DestroyMatchedGems(matchesToDestroy));
            yield return StartCoroutine(FallGems(fallPositions));
            yield return StartCoroutine(FindChainMatches());
        }
    }

    // Update position of Gems and create new ones
    IEnumerator FallGems(List<Vector2Int> fallPositions) {
        float maxDuration = 0;
        foreach(Vector3Int fall in fallPositions) {
            int fallY = 0;
            for(int y = fall.y; y < height; ++y) {
                GemBase gem = GetGem(fall.x, y);
                if(gem) {
                    float duration = gem.MoveTo(
                        GetWorldPosition(new Vector2Int(fall.x, y - fallY)),
                        GameController.instance.fallSpeed
                    );

                    gem.SetPosition(new Vector2Int(fall.x, y - fallY));

                    if(duration > maxDuration)
                        maxDuration = duration;
                } else {
                    fallY++;
                }
            }
            float delay = 0;
            for(int y = height - 1; y >= height - fallY; --y) {
                GemBase newGem = instance.CreateRandomGem(
                    fall.x, y,
                    GetWorldPosition(new Vector2Int(
                        fall.x, y + 1
                    )), delay
                );
                
                float duration = newGem.MoveTo(
                    GetWorldPosition(newGem.position),
                    GameController.instance.fallSpeed,
                    delay
                );
                delay = duration;
                if(duration > maxDuration)
                    maxDuration = duration;
            }
        }

        yield return new WaitForSeconds(maxDuration);
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
        yield return new WaitForSeconds(.25f);
        gemBoard = Miscellaneous.ShuffleMatrix(gemBoard);
        float maxDuration = 0;
        for(int j = 0; j < height; ++j) {
            for(int i = 0; i < width; ++i) {
                gemBoard[i, j].SetPosition(new Vector2Int(i, j));
                float duration = gemBoard[i, j].MoveTo(
                    GetWorldPosition(gemBoard[i, j].position),
                    GameController.instance.fallSpeed * (
                        gemBoard[i, j].transform.position -
                        GetWorldPosition(gemBoard[i, j].position)
                    ).magnitude/4
                );

                if(duration > maxDuration)
                    maxDuration = duration;
            }
        }

        yield return new WaitForSeconds(maxDuration);
    }

    IEnumerator DestroyMatchedGems(List<MatchInfo> matches) {
        float maxDuration = 0;
        int score = 0;
        
        foreach(MatchInfo matchInfo in matches) {
                
            float duration = DestroyGems(matchInfo.matches, matchInfo.pivot);
            
            if(matchInfo.type == MatchType.Cross) {
                float newGemDuration;
                GemBase newGem = CreateRandomGem(
                    matchInfo.pivot.position.x,
                    matchInfo.pivot.position.y,
                    GetWorldPosition(matchInfo.pivot.position + Vector2Int.up),
                    0, out newGemDuration
                );

                newGem.MoveTo(
                    GetWorldPosition(newGem.position),
                    GameController.instance.fallSpeed
                );

                duration += newGemDuration;
                Debug.Log($"Created { newGem.type } at { newGem.position }");
            }
                
            if(duration > maxDuration)
                maxDuration = duration;

            matchCounter++;
            score += matchInfo.GetScore();

        }

        GameController.score += score * matchCounter;
        UIController.ShowMsg($"{ GameData.GetComboMessage(matchCounter - 1) }");
        SoundController.PlaySfx(GameData.GetAudioClip("match"));
        
        yield return new WaitForSeconds(maxDuration/2);
    }

    public static float DestroyGems(List<GemBase> matches = null, bool moveToPivot = false) {
        Vector3 pivotPosition = Vector3.zero;
        
        if(matches == null) {
            matches = gemBoard.GetList();
            moveToPivot = false;
        } else if (moveToPivot && matches.Count > 0) {
            pivotPosition = GetWorldPosition(matches[0].position);
        }

        float maxDuration = 0;

        foreach(GemBase gem in matches) {
            gemBoard[gem.position.x, gem.position.y] = null;
            float duration = gem.Matched();

            if(moveToPivot)
                duration = Mathf.Max(duration, gem.MoveTo(
                    pivotPosition,
                    GameController.instance.fallSpeed
                ));

            if(duration > maxDuration)
                maxDuration = duration;
            
            Destroy(gem.gameObject, maxDuration);
        }
        
        return maxDuration;
    }
}
