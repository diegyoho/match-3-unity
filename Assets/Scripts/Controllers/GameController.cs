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
    Coroutine gameOver;

    [Header("Camera Settings")]
    public float cameraWidth = 7;
    public bool autoCameraWidth;
    public GameObject bg;
    public GameObject gemMenu;
    BaseGem gem;


    [Header("Game Settings")]
    public float swapSpeed;
    public float fallSpeed;
    public bool preventInitialMatches;

    [Header("Score Data")]

    [SerializeField]
    int _score;
    public static int score {
        get { return instance._score; }
        set {
            UIController.UpdateComboScore(
                value - instance._score, BoardController.matchCounter
            );
            instance._score = value;
            UIController.UpdateScore(instance._score);

            if(value > highscore)
                highscore = value;
        }
    }

    public static int highscore {
        get { return PlayerPrefs.GetInt("match3-highscore", 0); }
        set {
            PlayerPrefs.SetInt("match3-highscore", value);
            UIController.UpdateHighScore(value);
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

    [SerializeField]
    GameData gameData;

    void Start() {
        if(autoCameraWidth)
            cameraWidth = BoardController.width + (Camera.main.aspect * 2);
        
        Miscellaneous.SetCameraOrthographicSizeByWidth(Camera.main, cameraWidth);
        float bgWidth = bg.GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        float bgHeight = bg.GetComponent<SpriteRenderer>().sprite.bounds.size.y;

        bg.transform.localScale = Vector3.one * (
            Mathf.Max(
                cameraWidth / bgWidth,
                Camera.main.orthographicSize * 2 / bgHeight
            )
        );

        gemMenu.transform.localScale = Vector3.one * 2 * (cameraWidth / 7f);
        gem = gemMenu.GetComponentInChildren<BaseGem>();

        UIController.ShowMainScreen();
        
        SoundController.PlayMusic(GameData.GetAudioClip("bgm"), 1);
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
        BoardController.matchCounter = 0;
        UIController.ShowGameScreen();
        yield return new WaitForSeconds(1f);

        TouchController.cancel = true;
        yield return new WaitForSeconds(BoardController.CreateBoard());
        state = GameState.Playing;
        BoardController.UpdateBoard();
    }

    void GameOver() {
        if(gameOver == null)
            gameOver = StartCoroutine(IEGameOver());
    }

    IEnumerator IEGameOver() {

        yield return new WaitUntil(() => !BoardController.updatingBoard);

        if(timeLeft > 0) {
            gameOver = null;
            yield break;
        }

        TouchController.cancel = true;
        state = GameState.Menu;
        HintController.StopCurrentHint();
        HintController.StopHinting();
        UIController.ShowMsg("Game Over");
        yield return new WaitForSeconds(BoardController.DestroyGems() + .5f);
        UIController.ShowMainScreen();
        gameOver = null;
    }

    public static void ShowGemMenu(bool show = true) {
        if(show) {
            instance.changeGem = instance.StartCoroutine(instance.IEChangeGem());
        } else {
            instance.gem.Matched();
            if(instance.changeGem != null) {
                instance.StopCoroutine(instance.changeGem);
                instance.changeGem = null;            }
        }
    }

    IEnumerator IEChangeGem() {
        gemMenu.gameObject.SetActive(true);
        gem.SetType(GameData.RandomGem());
        
        yield return new WaitForSeconds(gem.Creating() * 3);

        yield return new WaitForSeconds(gem.Matched());
        
        changeGem = StartCoroutine(IEChangeGem());
    }
}
