using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class GameController : SingletonMonoBehaviour<GameController> {

    [SerializeField]
    GameData _gameData;
    public static GameData gameData {
        get { return instance._gameData; }
    }

    public float cameraWidth = 7f;
    
    public float swapSpeed = .15f;
    public float fallSpeed = .5f;
    public bool preventInitialMatches;
    public override void Awake() {
        base.Awake();
        MiscellaneousUtils.SetCameraOrthographicSizeByWidth(Camera.main, cameraWidth);
    }

    void Start() {
        BoardController.CreateBoard();
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.Space)) {
            StartCoroutine(BoardController.instance.ShuffleBoard());
        }
    }
}
