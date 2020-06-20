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
    public GemBase[] gemBoard;
    public GameObject gemPrefab;
    public override void Awake() {
        base.Awake();
        MiscellaneousUtils.SetCameraOrthographicSizeByWidth(Camera.main, cameraWidth);
        CreateBoard();
    }

    public void CreateBoard() {
        gemBoard = new GemBase[sizeBoard * sizeBoard];

        for(int i = 0; i < sizeBoard; ++i) {
            for(int j = 0; j < sizeBoard; ++j) {

                GemBase gem = Instantiate(
                    gemPrefab,
                    Vector2.zero,
                    Quaternion.identity,
                    transform
                ).GetComponent<GemBase>();

                gem.SetPosition(i, j);
                gemBoard[j * sizeBoard + i] = gem;

                do {
                    gem.type = MiscellaneousUtils.Choose((GemType[]) System.Enum.GetValues(typeof(GemType)));
                } while(HasMatch(gem.position, gem.type));
                // List<GemBase> matches = Match(gem.position, gem.type);
            }
        }
    }

    public static GemBase GetGem(Vector2Int position) {
        if(!MiscellaneousUtils.RectContains(position, Vector2.zero, Vector2.one * instance.sizeBoard))
            return null;

        return instance.gemBoard[position.y * instance.sizeBoard + position.x];
    }

    public static bool SwapGems(Vector2Int fromPosition, Vector2Int toPosition) {
        
        if(!MiscellaneousUtils.RectContains(toPosition, Vector2.zero, Vector2.one * instance.sizeBoard))
            return false;
        
        GemBase from = instance.gemBoard[fromPosition.y * instance.sizeBoard + fromPosition.x];
        GemBase to = instance.gemBoard[toPosition.y * instance.sizeBoard + toPosition.x];

        instance.gemBoard[fromPosition.y * instance.sizeBoard + fromPosition.x] = to;
        instance.gemBoard[toPosition.y * instance.sizeBoard + toPosition.x] = from;

        from.SetPosition(toPosition.x, toPosition.y);
        to.SetPosition(fromPosition.x, fromPosition.y);

        // Debug.Log($"Swap Gems: {from.type}, {to.position} <> {to.type}, {from.position}");

        return true;
    }
    
    public static List<GemBase> MatchLine(Vector2Int position, GemType type, Vector2Int direction) {
        
        List<GemBase> matches = new List<GemBase>();
        
        int maxGems = (instance.minMatch * 2) - 1;

        for(int i = 0; i < maxGems; ++i) {
            
            GemBase current = GetGem(position + direction * (i -(instance.minMatch - 1)));
            
            if(current && current.type == type) {
                matches.Add(current);        
            } else if(matches.Count < instance.minMatch) {
                matches.Clear();
                
                if((maxGems - i) < instance.minMatch)
                    break;
            }
        }

        return matches;
    }

    public static List<GemBase> Match(Vector2Int position, GemType type) {
        
        List<GemBase> matches = new List<GemBase>();
        
        matches.AddRange(MatchLine(position, type, Vector2Int.right));
        matches.AddRange(MatchLine(position, type, Vector2Int.up));

        return matches;
    }

    public static bool HasMatch(Vector2Int position, GemType type) {
        return Match(position, type).Count > 0;
    }
}
