using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class GameController : SingletonMonoBehaviour<GameController> {

    [Header("Camera Settings")]
    public float cameraWidth = 7;
    public bool autoCameraWidth;
    public GameObject bg;


    [Header("Game Settings")]
    
    [SerializeField]
    GameData _gameData;
    public static GameData gameData {
        get { return instance._gameData; }
    }
    public float swapSpeed;
    public float fallSpeed;
    public bool preventInitialMatches;

    [Header("Score Data")]

    [SerializeField]
    int _score;
    public static int score {
        get { return instance._score; }
        set {
            instance._score = value;
            UIController.UpdateScore(instance._score);
        }
    }

    void Start() {
        if(autoCameraWidth)
            cameraWidth = BoardController.width + 1;
        
        MiscellaneousUtils.SetCameraOrthographicSizeByWidth(Camera.main, cameraWidth);
        float bgHeight = bg.GetComponent<SpriteRenderer>().sprite.bounds.size.y;
        bg.transform.localScale = Vector3.one * (Camera.main.orthographicSize * 2 / bgHeight);

        UIController.UpdateScore(score);
        
        BoardController.CreateBoard();
    }

#if UNITY_EDITOR

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

#endif

}
