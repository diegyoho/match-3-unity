﻿using System.Collections;
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

    float timePulse;

    public static void ShowMainScreen() {
        instance.StartCoroutine(instance.IEChangeScreen(instance.mainScreen, executeAfter: () => {
            GameController.ShowGemMenu();
        }));
    }

    public static void ShowGameScreen() {
        
        UpdateScore(GameController.score);
        UpdateGoalScore(GameController.currentGoalScore);
        UpdateTimeLeft(GameController.timeLeft);
        instance.StartCoroutine(instance.IEChangeScreen(instance.gameScreen, () => {
            GameController.ShowGemMenu(false);
        }));
    }

    public static void UpdateScore(int score) {
        instance.scoreText.text = $"{ score }";
        instance.scoreText.transform.parent.GetComponent<Animator>().SetTrigger("pulse");
    }

    public static void UpdateGoalScore(int goalScore) {
        instance.goalScoreText.text = $"/{ goalScore }";
        instance.goalScoreText.GetComponent<Animator>().SetTrigger("pulse");
    }

    public static void UpdateTimeLeft(float timeLeft) {
        if(timeLeft <= 30) {
            if(Time.time - instance.timePulse > 1f) {
                instance.timeLeftText.GetComponent<Animator>().SetTrigger("pulse");
                instance.timePulse = Time.time;
                SoundController.PlaySfx("click");
            }
        } else {
            instance.timePulse = 0;
        }
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(timeLeft);
        instance.timeLeftText.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
    }

    IEnumerator IEChangeScreen(CanvasGroup screen, System.Action executeBefore = null, System.Action executeAfter = null) {
        if(executeBefore != null)
            executeBefore();

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

        if(executeAfter != null)
            executeAfter();
    }
}