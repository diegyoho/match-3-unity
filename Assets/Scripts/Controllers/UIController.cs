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

    CanvasGroup currentScreen;

    public static void ShowMainScreen() {
        instance.StartCoroutine(instance.IEChangeScreen(instance.mainScreen));
    }

    public static void ShowGameScreen() {
        
        UpdateScore(GameController.score);
        instance.StartCoroutine(instance.IEChangeScreen(instance.gameScreen));
    }

    public static void UpdateScore(int score) {
        instance.scoreText.text = $"Score: { score }";
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