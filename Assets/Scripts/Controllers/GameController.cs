using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public enum GameState {
    Menu,
    Playing
}

public class GameController : SingletonMonoBehaviour<GameController> {

    Coroutine changeGem;

    [Header("Camera Settings")]
    public float cameraWidth = 7;
    public bool autoCameraWidth;
    public GameObject bg;
    public GameObject gemMenu;
    GemBase gem;


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

    [SerializeField]
    int _currentGoalScore;

    public static int currentGoalScore {
        get { return instance._currentGoalScore; }
        set {
            instance._currentGoalScore = value;
            UIController.UpdateGoalScore(instance._currentGoalScore);
        }
    }

    [SerializeField]
    float _timeLeft;
    public static float timeLeft {
        get { return instance._timeLeft; }
        set {
            instance._timeLeft = Mathf.Max(value, 0);
            UIController.UpdateTimeLeft(instance._timeLeft);
        }
    }

    public static GameState state = GameState.Menu;

    void Start() {
        if(autoCameraWidth)
            cameraWidth = BoardController.width + 1;
        
        MiscellaneousUtils.SetCameraOrthographicSizeByWidth(Camera.main, cameraWidth);
        float bgHeight = bg.GetComponent<SpriteRenderer>().sprite.bounds.size.y;
        bg.transform.localScale = Vector3.one * (Camera.main.orthographicSize * 2 / bgHeight);

        gemMenu.transform.localScale = Vector3.one * 2 * (cameraWidth / 7f);
        gem = gemMenu.GetComponentInChildren<GemBase>();

        UIController.ShowMainScreen();
        
        SoundController.PlayMusic("bgm", 1);
    }

    void Update() {
        if(state == GameState.Playing) {
            timeLeft -= Time.deltaTime;
            if(score >= currentGoalScore) {
                currentGoalScore += currentGoalScore + currentGoalScore/2;
                timeLeft = 120;
            }

            if(timeLeft <= 0) {
                GameOver();
            }
        }
    #if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.Space)) {
            StartCoroutine(BoardController.instance.ShuffleBoard());
        }

        if(Input.GetKeyDown(KeyCode.H)) {
            if(!HintController.isShowing)
                HintController.ShowHint();
            else
                HintController.StopCurrentHint();
        }
    #endif
    }

    public void StartGame() {
        StartCoroutine(IEStartGame());
    }

    IEnumerator IEStartGame() {
        score = 0;
        currentGoalScore = 50;
        timeLeft = 120;
        UIController.ShowGameScreen();
        yield return new WaitForSeconds(1f);

        
        TouchController.cancel = true;
        yield return new WaitForSeconds(BoardController.CreateBoard());
        state = GameState.Playing;
        BoardController.UpdateBoard();
    }

    void GameOver() {
        StartCoroutine(IEGameOver());
    }

    IEnumerator IEGameOver() {
        TouchController.cancel = true;
        state = GameState.Menu;
        HintController.StopCurrentHint();
        HintController.StopHinting();
        yield return new WaitForSeconds(BoardController.DestroyGems() + .5f);
        UIController.ShowMainScreen();
    }

    public static void ShowGemMenu(bool show = true) {
        if(show) {
            instance.changeGem = instance.StartCoroutine(instance.IEChangeGem());
        } else {
            instance.gem.animator.SetTrigger("matched");
            if(instance.changeGem != null) {
                instance.StopCoroutine(instance.changeGem);
                instance.changeGem = null;            }
        }
    }

    IEnumerator IEChangeGem() {
        gemMenu.gameObject.SetActive(true);
        gem.SetType(gameData.RandomGem());
        SoundController.PlaySfx("match");
        instance.gem.animator.SetTrigger("creating");
        yield return new WaitForSeconds(3);

        gem.animator.SetTrigger("matched");
        yield return new WaitForSeconds(.5f);
        
        changeGem = StartCoroutine(IEChangeGem());
    }
}
