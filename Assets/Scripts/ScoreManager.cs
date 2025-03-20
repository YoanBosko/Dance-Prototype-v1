using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public AudioSource hitSFX;
    public AudioSource missSFX;
    public TMPro.TextMeshPro scoreText;
    public TMPro.TextMeshPro resultText;
    static int comboScore;
    static string result;
    void Start()
    {
        Instance = this;
        comboScore = 0;
        result = "";
    }
    public static void Perfect()
    {
        result = "Perfect";
        comboScore += 1;
        Instance.hitSFX.Play();
    }
    public static void Good()
    {
        result = "Godo";
        comboScore += 1;
        Instance.hitSFX.Play();
    }
    public static void Bad()
    {
        result = "Bad";
        comboScore = 0;
        Instance.hitSFX.Play();
    }
    public static void Miss()
    {
        result = "Miss";
        comboScore = 0;
        Instance.missSFX.Play();    
    }
    private void Update()
    {
        resultText.text = result;
        scoreText.text = comboScore.ToString();
    }
}
