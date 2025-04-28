using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinMenu : MonoBehaviour
{
    public TMPro.TextMeshPro perfectText;
    public TMPro.TextMeshPro goodText;
    public TMPro.TextMeshPro badText;
    public TMPro.TextMeshPro missText;

    void Start()
    {
        UpdateScoreDetails();
    }

    public void UpdateScoreDetails()
    {
        if (ScoreManager.Instance == null)
        {
            Debug.LogWarning("ScoreManager instance tidak ditemukan!");
            return;
        }

        perfectText.text = "Perfect: " + GetPerfectHits();
        goodText.text = "Good: " + GetGoodHits();
        badText.text = "Bad: " + GetBadHits();
        missText.text = "Miss: " + GetMissHits();
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