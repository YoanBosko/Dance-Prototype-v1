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
    public TMPro.TextMeshPro songTitle;
    public TMPro.TextMeshPro songDifficulty;
    public ScoreData scoreData;
    public BeatmapData beatmapDataAssign;

    void Start()
    {
        UpdateScoreDetails();
    }

    public void UpdateScoreDetails()
    {
        score.text = "" + scoreData.score;
        accuracy.text = "" + scoreData.accuracy + "%";
        perfectText.text = ": " + scoreData.perfectHits;
        goodText.text = ": " + scoreData.goodHits;
        badText.text = ": " + scoreData.badHits;
        missText.text = ": " + scoreData.missHits;
        songTitle.text = beatmapDataAssign.songTitle;
        songDifficulty.text = beatmapDataAssign.songDifficulty;
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