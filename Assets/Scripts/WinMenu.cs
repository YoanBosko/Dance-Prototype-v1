using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinMenu : MonoBehaviour
{
    public TMPro.TextMeshPro score;
    public TMPro.TextMeshPro accuracy;
    public TMPro.TextMeshPro perfectText;
    public TMPro.TextMeshPro goodText;
    public TMPro.TextMeshPro badText;
    public TMPro.TextMeshPro missText;
    public ScoreData scoreData;

    void Start()
    {
        UpdateScoreDetails();
    }

    public void UpdateScoreDetails()
    {
        score.text = "Total Score: " + scoreData.score;
        accuracy.text = "Accuracy: " + scoreData.accuracy;
        perfectText.text = "Perfect: " + scoreData.perfectHits;
        goodText.text = "Good: " + scoreData.goodHits;
        badText.text = "Bad: " + scoreData.badHits;
        missText.text = "Miss: " + scoreData.missHits;
    }

    int GetPerfectHits()
    {
        return typeof(ScoreManager).GetField("perfectHits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null) as int? ?? 0;
    }

    int GetGoodHits()
    {
        return typeof(ScoreManager).GetField("goodHits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null) as int? ?? 0;
    }

    int GetBadHits()
    {
        return typeof(ScoreManager).GetField("badHits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null) as int? ?? 0;
    }

    int GetMissHits()
    {
        return typeof(ScoreManager).GetField("missHits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null) as int? ?? 0;
    }
}