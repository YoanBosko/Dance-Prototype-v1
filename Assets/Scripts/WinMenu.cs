using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

// Kelas TextAnimationSettings tetap sama
[System.Serializable]
public class TextAnimationSettings
{
    public TMP_Text textMeshElement;
    public float transitionDuration = 1.0f;
    public float delayBeforeStart = 0.2f;
    public string prefix = "";
    public string suffix = "";
    public string floatFormat = "F2";
}

public class WinMenu : MonoBehaviour
{
    [Header("Text Animation Settings")]
    public TextAnimationSettings scoreAnimationSettings;
    public TextAnimationSettings accuracyAnimationSettings;
    public TextAnimationSettings perfectHitsAnimationSettings;
    public TextAnimationSettings goodHitsAnimationSettings;
    public TextAnimationSettings badHitsAnimationSettings;
    public TextAnimationSettings missHitsAnimationSettings;

    [Header("Static Text References")]
    public TMP_Text songTitleText;
    public TMP_Text songDifficultyText;
    [Tooltip("Komponen teks untuk menampilkan grade (S, A, B, C, D).")]
    public TMP_Text gradeText;

    [Header("Post-Animation Settings")]
    [Tooltip("GameObject yang akan diaktifkan setelah semua animasi selesai.")]
    public GameObject objectToActivateAfterAnimation; // Variabel baru

    [Header("Data References")]
    public ScoreData scoreData;
    public ScoreData scoreDataCumulative;
    public BeatmapData beatmapDataAssign;

    // Properti publik untuk mengecek status animasi
    public bool IsAnimating { get; private set; } = false;

    // Variabel boolean baru untuk grade
    [HideInInspector] // Disembunyikan dari Inspector karena diatur oleh skrip
    public bool achievedGoodGrade = false;

    private int targetScore;
    private float targetAccuracy;
    private int targetPerfectHits;
    private int targetGoodHits;
    private int targetBadHits;
    private int targetMissHits;

    void OnEnable()
    {
        // Pastikan objek dinonaktifkan di awal
        if (objectToActivateAfterAnimation != null)
        {
            objectToActivateAfterAnimation.SetActive(false);
        }

        StoreTargetValues();
        UpdateStaticTexts();
        UpdateGradeDisplay();
        StopAllCoroutines();
        StartCoroutine(StartAllConfiguredAnimations());
    }

    /// <summary>
    /// Fungsi publik untuk melewati semua animasi dan langsung menampilkan hasil akhir.
    /// </summary>
    public void SkipAllAnimations()
    {
        if (!IsAnimating) return;

        Debug.Log("Melewati animasi skor...");
        StopAllCoroutines();

        // Langsung atur semua teks ke nilai akhirnya
        SetFinalTextValue(scoreAnimationSettings, targetScore.ToString());
        SetFinalTextValue(accuracyAnimationSettings, targetAccuracy.ToString(accuracyAnimationSettings.floatFormat));
        SetFinalTextValue(perfectHitsAnimationSettings, targetPerfectHits.ToString());
        SetFinalTextValue(goodHitsAnimationSettings, targetGoodHits.ToString());
        SetFinalTextValue(badHitsAnimationSettings, targetBadHits.ToString());
        SetFinalTextValue(missHitsAnimationSettings, targetMissHits.ToString());

        UpdateGradeDisplay();

        IsAnimating = false;

        // Aktifkan GameObject setelah skip
        if (objectToActivateAfterAnimation != null)
        {
            objectToActivateAfterAnimation.SetActive(true);
        }
    }

    private void SetFinalTextValue(TextAnimationSettings settings, string value)
    {
        if (settings != null && settings.textMeshElement != null)
        {
            settings.textMeshElement.SetText(settings.prefix + value + settings.suffix);
        }
    }

    void StoreTargetValues()
    {
        if (scoreData == null) { Debug.LogError("ScoreData belum di-assign!"); return; }

        targetScore = scoreData.score;
        targetAccuracy = scoreData.accuracy;
        targetPerfectHits = scoreData.perfectHits;
        targetGoodHits = scoreData.goodHits;
        targetBadHits = scoreData.badHits;
        targetMissHits = scoreData.missHits;

        if (scoreDataCumulative != null)
        {
            scoreDataCumulative.score += scoreData.score;
        }
    }

    void UpdateStaticTexts()
    {
        if (beatmapDataAssign != null)
        {
            if (songTitleText != null) songTitleText.text = beatmapDataAssign.songTitle;
            if (songDifficultyText != null) songDifficultyText.text = beatmapDataAssign.songDifficulty;
        }
    }

    void UpdateGradeDisplay()
    {
        achievedGoodGrade = false;

        if (gradeText == null) return;

        string grade = "";

        if (targetAccuracy >= 90f)
        {
            grade = "S";
            achievedGoodGrade = true;
        }
        else if (targetAccuracy >= 80f)
        {
            grade = "A";
            achievedGoodGrade = true;
        }
        else if (targetAccuracy >= 70f)
        {
            grade = "B";
            achievedGoodGrade = true;
        }
        else if (targetAccuracy >= 60f)
        {
            grade = "C";
            achievedGoodGrade = true;
        }
        else
        {
            grade = "F";
        }

        gradeText.text = grade;
    }

    IEnumerator StartAllConfiguredAnimations()
    {
        IsAnimating = true;

        var allSettings = new List<TextAnimationSettings>
        {
            scoreAnimationSettings, accuracyAnimationSettings, perfectHitsAnimationSettings,
            goodHitsAnimationSettings, badHitsAnimationSettings, missHitsAnimationSettings
        };

        float maxAnimationTime = 0f;

        StartCoroutine(AnimateIntNumberCoroutine(scoreAnimationSettings, targetScore));
        StartCoroutine(AnimateFloatNumberCoroutine(accuracyAnimationSettings, targetAccuracy));
        StartCoroutine(AnimateIntNumberCoroutine(perfectHitsAnimationSettings, targetPerfectHits));
        StartCoroutine(AnimateIntNumberCoroutine(goodHitsAnimationSettings, targetGoodHits));
        StartCoroutine(AnimateIntNumberCoroutine(badHitsAnimationSettings, targetBadHits));
        StartCoroutine(AnimateIntNumberCoroutine(missHitsAnimationSettings, targetMissHits));

        foreach (var setting in allSettings)
        {
            if (setting != null && setting.textMeshElement != null)
            {
                float totalTime = setting.delayBeforeStart + setting.transitionDuration;
                if (totalTime > maxAnimationTime)
                {
                    maxAnimationTime = totalTime;
                }
            }
        }

        yield return new WaitForSeconds(maxAnimationTime);

        // Aktifkan GameObject setelah animasi terlama selesai
        if (objectToActivateAfterAnimation != null)
        {
            objectToActivateAfterAnimation.SetActive(true);
        }

        IsAnimating = false;
        Debug.Log("Semua animasi skor selesai.");
    }

    IEnumerator AnimateIntNumberCoroutine(TextAnimationSettings settings, int targetValue)
    {
        if (settings == null || settings.textMeshElement == null) yield break;
        yield return new WaitForSeconds(settings.delayBeforeStart);

        TMP_Text textMesh = settings.textMeshElement;
        float transitionDuration = settings.transitionDuration;
        string prefix = settings.prefix;

        float currentValue = 0;
        textMesh.SetText(prefix + "0");

        float timer = 0f;
        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / transitionDuration);
            currentValue = Mathf.Lerp(0, targetValue, progress);
            textMesh.SetText(prefix + Mathf.RoundToInt(currentValue).ToString());
            yield return null;
        }

        textMesh.SetText(prefix + targetValue.ToString());
    }

    IEnumerator AnimateFloatNumberCoroutine(TextAnimationSettings settings, float targetValue)
    {
        if (settings == null || settings.textMeshElement == null) yield break;
        yield return new WaitForSeconds(settings.delayBeforeStart);

        TMP_Text textMesh = settings.textMeshElement;
        float transitionDuration = settings.transitionDuration;
        string format = settings.floatFormat;
        string prefix = settings.prefix;
        string suffix = settings.suffix;

        float currentValue = 0f;
        textMesh.SetText(prefix + currentValue.ToString(format) + suffix);

        float timer = 0f;
        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / transitionDuration);
            currentValue = Mathf.Lerp(0f, targetValue, progress);
            textMesh.SetText(prefix + currentValue.ToString(format) + suffix);
            yield return null;
        }

        textMesh.SetText(prefix + targetValue.ToString(format) + suffix);
    }


// Fungsi GetHits tetap sama
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
