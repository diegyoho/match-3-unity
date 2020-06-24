using System.Collections;
using System.Collections.Generic;
using Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : SingletonMonoBehaviour<UIController> {

    [Header("Screens")]
    [SerializeField]
    CanvasGroup mainScreen;
    [SerializeField]
    CanvasGroup gameScreen;

    [Header("Game Screen")]

    [SerializeField]
    TextMeshProUGUI scoreText;

    [SerializeField]
    TextMeshProUGUI goalScoreText;
    [SerializeField]
    TextMeshProUGUI timeLeftText;

    CanvasGroup currentScreen;

    public static void ShowMainScreen() {
        instance.StartCoroutine(instance.IEChangeScreen(instance.mainScreen));
    }

    public static void ShowGameScreen() {
        
        UpdateScore(GameController.score);
        UpdateGoalScore(GameController.currentGoalScore);
        UpdateTimeLeft(GameController.timeLeft);
        instance.StartCoroutine(instance.IEChangeScreen(instance.gameScreen));
    }

    public static void UpdateScore(int score) {
        instance.scoreText.text = $"{ score }";
    }

    public static void UpdateGoalScore(int goalScore) {
        instance.goalScoreText.text = $"/{ goalScore }";
    }

    public static void UpdateTimeLeft(float timeLeft) {
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(timeLeft);
        instance.timeLeftText.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
    }

    IEnumerator IEChangeScreen(CanvasGroup screen) {

        screen.alpha = 0;
        screen.gameObject.SetActive(false);

        if(currentScreen) {
            while(currentScreen.alpha > 0) {
                currentScreen.alpha -= Time.deltaTime * 2;
                yield return null;
            }
            currentScreen.gameObject.SetActive(false);
        }

        currentScreen = screen;
        currentScreen.gameObject.SetActive(true);

        while(currentScreen.alpha < 1) {
            currentScreen.alpha += Time.deltaTime * 2;
            yield return null;
        }

        yield return null;
    }
}