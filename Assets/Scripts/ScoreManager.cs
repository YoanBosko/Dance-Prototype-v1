using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public AudioSource hitSFX;
    public AudioSource missSFX;
    public TMPro.TextMeshPro comboText;
    public TMPro.TextMeshPro scoreText;
    public TMPro.TextMeshPro resultText;
    public TMPro.TextMeshPro accuracyText;
    
    static int comboScore;
    static string result;
    static int totalScore = 0;

    static int totalBeats;  // Total beat dalam permainan
    public static int successfulHits;  // Jumlah hit yang masuk kategori Perfect atau Good
    static float scoreMultiplier = 1.0f; // Default multiplier

    static int perfectHits = 0;
    static int goodHits = 0;
    static int badHits = 0;
    static int missHits = 0;
    
    void Start()
    {
        Instance = this;
        comboScore = 0;
        result = "";
        totalBeats = SongManager.Instance.GetTotalBeats(); // Ambil total beat dari SongManager
        successfulHits = 0;
    }

    public static void UpdateMultiplier()
    {
        scoreMultiplier = 1.0f + (comboScore / 100.0f * 0.25f);
    }

    public static void Perfect()
    {
        result = "Perfect";
        comboScore += 1;
        perfectHits++;
        totalBeats++;

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

        UpdateMultiplier();
        Instance.missSFX.Play();
    }

    public static float GetAccuracy()
    {
        if (totalBeats == 0) return 100f; // Hindari pembagian dengan nola

        float accuracy = ((perfectHits * 1.0f) + (goodHits * 0.75f) + (badHits * 0.5f) + (missHits * 0.0f)) / totalBeats * 100f;
        return accuracy;
    }

    private void Update()
    {
        resultText.text = result;
        comboText.text = comboScore.ToString() + "X";
        accuracyText.text = "Akurasi: " + GetAccuracy().ToString("F2") + "%"; // Update UI Akurasi
        scoreText.text = totalScore.ToString(); // Menampilkan total skor
    }
}
