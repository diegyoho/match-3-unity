using System.Collections;
using System.Collections.Generic;
using Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : SingletonMonoBehaviour<UIController> {

    [SerializeField]
    TextMeshProUGUI scoreText;

    public static void UpdateScore(int score) {
        instance.scoreText.text = $"Score: { score }";
    }
}