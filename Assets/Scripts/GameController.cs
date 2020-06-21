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
    public int sizeBoard = 6;
    public int minMatch = 3;
    public bool preventInitialMatches;
    public GemBase[, ] gemBoard;
    public GameObject gemPrefab;
    public override void Awake() {
        base.Awake();
        MiscellaneousUtils.SetCameraOrthographicSizeByWidth(Camera.main, cameraWidth);
        CreateBoard();
    }

    public void CreateBoard() {
        gemBoard = new GemBase[sizeBoard, sizeBoard];

        for(int i = 0; i < sizeBoard; ++i) {
            for(int j = 0; j < sizeBoard; ++j) {
                GemBase gem = CreateGem(i, j);
                
                if(preventInitialMatches)
                    while(GetMatchInfo(gem).isValid) {
                        gem.type = MiscellaneousUtils.Choose((GemType[]) System.Enum.GetValues(typeof(GemType)));
                    }
            }
        }
    }

    GemBase CreateGem(int x, int y) {
        
        GemBase gem = Instantiate(
            gemPrefab,
            Vector2.zero,
            Quaternion.identity,
            transform
        ).GetComponent<GemBase>();

        gem.SetPosition(new Vector2Int(x, y));
        gem.MoveTo(gem.position);
        gem.type = MiscellaneousUtils.Choose((GemType[]) System.Enum.GetValues(typeof(GemType)));

        return gem;
    }

    public static void SwapGems(GemBase from, GemBase to) {

        from.MoveTo(to.position);
        to.MoveTo(from.position);

        Vector2Int fromPosition = from.position;
        from.SetPosition(to.position);
        to.SetPosition(fromPosition);
    }

    public static void TryMatch(GemBase from, GemBase to) {
        SwapGems(from, to);
        
        MatchInfo matchFrom = GetMatchInfo(from);
        MatchInfo matchTo = GetMatchInfo(to);

        if(!(matchFrom.isValid || matchTo.isValid)) {
            SwapGems(from, to);
        } else {
            List<GemBase> matches = new List<GemBase>(matchFrom.matches);
            matches.AddRange(matchTo.matches);
            DestroyGems(matches);
            FallGems(MatchInfo.MergeFallPositions(matchFrom.fallPositions, matchTo.fallPositions));
        }
    }

    public static void FallGems(List<Vector3Int> fallPositions) {

        fallPositions.ForEach(fall => {
            Debug.Log(fall);
            for(int y = fall.y + fall.z; y < instance.sizeBoard; ++y) {
                GemBase gem = instance.gemBoard[fall.x, y];
                gem.MoveTo(new Vector2Int(fall.x, y - fall.z));
                gem.SetPosition(new Vector2Int(fall.x, y - fall.z));
            }
            for(int i = fall.z; i > 0; --i) {
                instance.CreateGem(fall.x, instance.sizeBoard - i);
            }
        });
    }
    
    List<GemBase> GetHorizontalMatches(GemBase gem) {
        
        List<GemBase> matches = new List<GemBase>();
        
        int id = gem.position.x - 1;

        while(id >= 0 && gemBoard[id, gem.position.y] && gemBoard[id, gem.position.y].type == gem.type) {
            matches.Add(gemBoard[id, gem.position.y]);
            id--;
        }

        id = gem.position.x + 1;

        while(id < sizeBoard && gemBoard[id, gem.position.y] && gemBoard[id, gem.position.y].type == gem.type) {
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

        while(id < sizeBoard && gemBoard[gem.position.x, id] && gemBoard[gem.position.x, id].type == gem.type) {
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
        }

        if(matchInfo.isValid) {
            matchInfo.matches.Add(gem);
            matchInfo.matches.AddRange(horizontalMatches);
            matchInfo.matches.AddRange(verticalMatches);
            matchInfo.CalcFallPositions();
        }

        return matchInfo;
    }

    public static void DestroyGems(List<GemBase> matches) {
        foreach(GemBase gem in matches) {
            Destroy(gem.gameObject);
        }
    }
}
