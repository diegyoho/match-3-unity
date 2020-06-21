using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class GameController : SingletonMonoBehaviour<GameController> {

    List<GemBase> matchedGems = new List<GemBase>();
    [SerializeField]
    GameData _gameData;
    public static GameData gameData {
        get { return instance._gameData; }
    }

    public float cameraWidth = 7f;
    public int sizeBoardX = 6;
    public int sizeBoardY = 6;
    public int minMatch = 3;
    public float swapSpeed = .15f;
    public float fallSpeed = .5f;
    public bool preventInitialMatches;
    public GemBase[, ] gemBoard;
    public GameObject gemPrefab;
    public override void Awake() {
        base.Awake();
        MiscellaneousUtils.SetCameraOrthographicSizeByWidth(Camera.main, cameraWidth);
        CreateBoard();
    }

    public static Vector3 GetWorldPosition(Vector2Int position) {
        return new Vector2(
            position.x - ((instance.sizeBoardX/2) - 0.5f),
            position.y - ((instance.sizeBoardY/2) - 0.5f)
        );
    }

    public void CreateBoard() {
        gemBoard = new GemBase[sizeBoardX, sizeBoardY];

        for(int i = 0; i < sizeBoardX; ++i) {
            for(int j = 0; j < sizeBoardY; ++j) {
                GemBase gem = CreateGem(i, j);
                
                if(preventInitialMatches)
                    while(GetMatchInfo(gem).isValid) {
                        gem.type = MiscellaneousUtils.Choose((GemType[]) System.Enum.GetValues(typeof(GemType)));
                    }

                StartCoroutine(gem.IEMoveTo(gem.position, fallSpeed));
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

        StartCoroutine(from.IEMoveTo(to.position, swapSpeed));
        StartCoroutine(to.IEMoveTo(from.position, swapSpeed));

        yield return new WaitForSeconds(swapSpeed);

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
        
        MatchInfo matchFrom = GetMatchInfo(from);
        MatchInfo matchTo = GetMatchInfo(to);

        if(!(matchFrom.isValid || matchTo.isValid)) {
            yield return StartCoroutine(SwapGems(from, to));
        } else {
            List<GemBase> matches = new List<GemBase>(matchFrom.matches);
            matches.AddRange(matchTo.matches);
            DestroyGems(matches);
            yield return StartCoroutine(FallGems(MatchInfo.MergeFallPositions(
                matchFrom.fallPositions,
                matchTo.fallPositions
            )));
        }
        
        TouchController.cancel = false;
    }

    IEnumerator FallGems(List<Vector3Int> fallPositions) {

        foreach(Vector3Int fall in fallPositions) {
            for(int y = fall.y + fall.z; y < instance.sizeBoardY; ++y) {
                GemBase gem = instance.gemBoard[fall.x, y];
                StartCoroutine(gem.IEMoveTo(new Vector2Int(fall.x, y - fall.z), fall.z * swapSpeed/1.5f));
                gem.SetPosition(new Vector2Int(fall.x, y - fall.z));
            }
            for(int i = fall.z; i > 0; --i) {
                GemBase gem = instance.CreateGem(
                    fall.x, instance.sizeBoardY - i,
                    GetWorldPosition(new Vector2Int(
                        fall.x, instance.sizeBoardY - i - fall.z
                    )) + Vector3.up * (Camera.main.orthographicSize + sizeBoardY/2)
                );
                StartCoroutine(gem.IEMoveTo(gem.position, fallSpeed));
            }
        }

        yield return new WaitForSeconds(.5f);
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
        }

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
        }

        if(matchInfo.isValid) {
            matchInfo.matches.Add(gem);
            matchInfo.CalcFallPositions();
        }

        return matchInfo;
    }

    public static void DestroyGems(List<GemBase> matches) {
        foreach(GemBase gem in matches) {
            gem.StopAllCoroutines();
            Destroy(gem.gameObject);
        }
    }
}
