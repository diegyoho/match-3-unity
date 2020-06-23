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

    public float cameraWidth;
    public GameObject bg;
    
    public float swapSpeed;
    public float fallSpeed;
    public bool preventInitialMatches;
    public override void Awake() {
        base.Awake();
    }

    void Start() {
        cameraWidth = BoardController.width + 1;
        MiscellaneousUtils.SetCameraOrthographicSizeByWidth(Camera.main, cameraWidth);
        float bgHeight = bg.GetComponent<SpriteRenderer>().sprite.bounds.size.y;
        bg.transform.localScale = Vector3.one * (Camera.main.orthographicSize * 2 / bgHeight);
        
        BoardController.CreateBoard();
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.Space)) {
            StartCoroutine(BoardController.instance.ShuffleBoard());
        }

        if(Input.GetKeyDown(KeyCode.H)) {
            if(!HintController.isShowing)
                HintController.ShowHint();
            else
                HintController.StopCurrentHint();
        }
    }
}
