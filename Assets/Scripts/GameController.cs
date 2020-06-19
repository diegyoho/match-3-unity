using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class GameController : SingletonMonoBehaviour<GameController> {

    public float cameraWidth = 7f;
    public GameData gameData;
    public int sizeBoard = 6;
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
            }
        }
    }

    public void SwapGems(Vector2Int fromPosition, Vector2Int toPosition) {
        
        Rect board = new Rect(Vector2Int.zero, Vector2Int.one * GameController.instance.sizeBoard);

        if(!board.Contains(toPosition))
            return;
        
        GemBase from = gemBoard[fromPosition.y * sizeBoard + fromPosition.x];
        GemBase to = gemBoard[toPosition.y * sizeBoard + toPosition.x];
        
        if(from.type == to.type)
            return;

        gemBoard[fromPosition.y * sizeBoard + fromPosition.x] = to;
        gemBoard[toPosition.y * sizeBoard + toPosition.x] = from;

        from.SetPosition(toPosition.x, toPosition.y);
        to.SetPosition(fromPosition.x, fromPosition.y);

        Debug.Log($"Swap Gems: {from.type}, {to.position} <> {to.type}, {from.position}");
    }
}
