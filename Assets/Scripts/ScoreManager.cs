﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public AudioSource hitSFX;
    public AudioSource missSFX;
    public TMPro.TextMeshPro comboText;
    public TMPro.TextMeshPro scoreText;
    public TMPro.TextMeshPro resultText;
    public TMPro.TextMeshPro accuracyText;
    public Slider slider;
    public UnityEvent onHealthZero;
    public ScoreData scoreData;
    
    static int comboScore;
    static string result;
    static int totalScore = 0;
    static int healthBar = 600;

    static int totalBeats = 0;  // Total beat dalam permainan
    public static int successfulHits;  // Jumlah hit yang masuk kategori Perfect atau Good
    static float scoreMultiplier = 1.0f; // Default multiplier

    static int perfectHits = 0;
    static int goodHits = 0;
    static int badHits = 0;
    static int missHits = 0;

    
    void Awake()
    {
        // if (Instance == null)
        // {
        //     Instance = this;
        //     DontDestroyOnLoad(gameObject); // <--- ini penting
        // }
        // else
        // {
        //     Destroy(gameObject); // Kalau sudah ada instance, hancurkan yang baru
        // }
    }
    void Start()
    {
        Instance = this;
        comboScore = 0;
        result = "";
        totalBeats = SongManager.Instance.GetTotalBeats(); // Ambil total beat dari SongManager
        successfulHits = 0;
        healthBar = 600;

        scoreData.ResetScore();
    }

    public static void UpdateMultiplier()
    {
        scoreMultiplier = 1.0f + (comboScore / 100.0f * 0.25f);
    }

#region HitScoreManager
    public static void Perfect()
    {
        result = "Perfect";
        comboScore += 1;
        perfectHits++;
        totalBeats++;
        healthBar += 10;

        totalScore += Mathf.RoundToInt(100 * scoreMultiplier); // 100 poin untuk Perfect
        UpdateMultiplier();
        Instance.hitSFX.Play();
    }

    public static void Good()
    {
        result = "Good";
        comboScore += 1;
        goodHits++;
        totalBeats++;
        healthBar += 5;

        totalScore += Mathf.RoundToInt(75 * scoreMultiplier); // 75 poin untuk Good
        UpdateMultiplier();
        Instance.hitSFX.Play();
    }

    public static void Bad()
    {
        result = "Bad";
        comboScore = 0;  // Reset combo saat kena Bad
        badHits++;
        totalBeats++;
        healthBar -= 15;

        totalScore += Mathf.RoundToInt(50 * scoreMultiplier); // 50 poin untuk Bad
        UpdateMultiplier();
        Instance.hitSFX.Play();
    }

    public static void Miss()
    {
        result = "Miss";
        comboScore = 0;  // Reset combo saat Miss
        missHits++;
        totalBeats++;
        healthBar -= 70;

        UpdateMultiplier();
        Instance.missSFX.Play();
    }
#endregion
#region HoldScoreManager

    public IEnumerator HoldCoroutine()
    {
        while (true)
        {
            Perfect();
            yield return new WaitForSeconds(0.2f);
        }
    }

    public IEnumerator ReleaseCoroutine()
    {
        while (true)
        {
            Miss();
            yield return new WaitForSeconds(0.2f);
        }
    }
#endregion
    public static float GetAccuracy()
    {
        // if (totalBeats == 0) return 100f; // Hindari pembagian dengan nola

        // float accuracy = ((perfectHits * 1.0f) + (goodHits * 0.75f) + (badHits * 0.5f) + (missHits * 0.0f)) / totalBeats * 100f;
        // return accuracy;


        // Update formula untuk include hold notes
        float score = (perfectHits * 1.0f) + 
                     (goodHits * 0.75f) + 
                     (badHits * 0.5f);
        
        float totalPossible = totalBeats;
        return (score / totalPossible) * 100f;
    }

    private void Update()
    {
        resultText.text = result;
        comboText.text = comboScore.ToString() + "X";
        accuracyText.text = "Akurasi: " + GetAccuracy().ToString("F2") + "%"; // Update UI Akurasi
        scoreText.text = totalScore.ToString(); // Menampilkan total skor

        healthBar = Mathf.Clamp(healthBar, 0, 1000);
        slider.value = healthBar;
        if (slider.value == 0)
        {
            onHealthZero?.Invoke();
        }
    }

    public void ScoreDataUpdate()
    {
        scoreData.score = totalScore;
        scoreData.accuracy = GetAccuracy();
        scoreData.perfectHits = perfectHits;
        scoreData.goodHits = goodHits;
        scoreData.badHits = badHits;
        scoreData.missHits = missHits;
    }

    
}
