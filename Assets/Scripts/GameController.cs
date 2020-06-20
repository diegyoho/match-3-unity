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

                // while(Match(gem.position, gem.type).Count > 0) {
                //     gem.type = MiscellaneousUtils.Choose((GemType[]) System.Enum.GetValues(typeof(GemType)));
                // }
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

        
        gem.MoveTo(new Vector2Int(x, y));
        gem.type = MiscellaneousUtils.Choose((GemType[]) System.Enum.GetValues(typeof(GemType)));

        return gem;
    }

    public static void SwapGems(GemBase from, GemBase to) {

        Vector2Int fromPosition = from.position;
        from.MoveTo(to.position);
        to.MoveTo(fromPosition);
    }
    
    // public static List<GemBase> MatchLine(Vector2Int position, GemType type, Vector2Int direction) {
        
    //     List<GemBase> matches = new List<GemBase>();
        
    //     int maxGems = (instance.minMatch * 2) - 1;

    //     for(int i = 0; i < maxGems; ++i) {
            
    //         GemBase current = GetGem(position + direction * (i -(instance.minMatch - 1)));
            
    //         if(current && current.type == type) {
    //             matches.Add(current);        
    //         } else if(matches.Count < instance.minMatch) {
    //             matches.Clear();
                
    //             if((maxGems - i) < instance.minMatch)
    //                 break;
    //         }
    //     }

    //     return matches;
    // }

    // public static List<GemBase> Match(Vector2Int position, GemType type) {
        
    //     List<GemBase> matches = new List<GemBase>();
        
    //     matches.AddRange(MatchLine(position, type, Vector2Int.right));
    //     matches.AddRange(MatchLine(position, type, Vector2Int.up));

    //     return matches;
    // }

    // public static void DestroyGems(List<GemBase> matches) {
    //     foreach(GemBase gem in matches) {
    //         instance.gemBoard[gem.position.x, gem.position.y] = null;
    //         Destroy(gem.gameObject);
    //     }
    // }

    // public static void GemFall(Vector2Int position) {
    //     GemBase gem = GetGem(position);
    //     if(gem) {
    //         instance.gemBoard[gem.position.x, gem.position.y] = null;
    //         gem.SetPosition(position + Vector2Int.down);
    //         GemFall(position + Vector2Int.up);
    //     }
    // }
}
