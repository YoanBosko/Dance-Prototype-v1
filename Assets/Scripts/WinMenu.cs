using UnityEngine;
using System.Collections;
using TMPro; // Pastikan namespace TextMeshPro diimpor

// Kelas untuk menyimpan pengaturan animasi per elemen
[System.Serializable]
public class TextAnimationSettings
{
    public TMP_Text textMeshElement; // Diubah ke TMP_Text
    public float transitionDuration = 1.0f;
    public float delayBeforeStart = 0.2f;
    public string prefix = "";
    public string suffix = "";
    public string floatFormat = "F2";

    // Konstruktor default jika diperlukan
    public TextAnimationSettings(TMP_Text element) // Diubah ke TMP_Text
    {
        textMeshElement = element;
    }
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
    public TMP_Text songTitleText; // Diubah ke TMP_Text
    public TMP_Text songDifficultyText; // Diubah ke TMP_Text

    [Header("Data References")]
    public ScoreData scoreData;
    public ScoreData scoreDataCumulative;
    public BeatmapData beatmapDataAssign;

    private int targetScore;
    private float targetAccuracy;
    private int targetPerfectHits;
    private int targetGoodHits;
    private int targetBadHits;
    private int targetMissHits;

    void OnEnable()
    {
        StoreTargetValues();
        UpdateStaticTexts();
        StopAllCoroutines(); // Hentikan coroutine sebelumnya jika ada
        StartCoroutine(StartAllConfiguredAnimations());
    }

    void StoreTargetValues()
    {
        if (scoreData == null)
        {
            Debug.LogError("ScoreData belum di-assign di WinMenu!");
            return;
        }

        targetScore = scoreData.score;
        targetAccuracy = scoreData.accuracy;
        targetPerfectHits = scoreData.perfectHits;
        targetGoodHits = scoreData.goodHits;
        targetBadHits = scoreData.badHits;
        targetMissHits = scoreData.missHits;

        if (scoreDataCumulative != null)
        {
            // Pastikan ini hanya terjadi sekali per 'kemenangan' jika scoreDataCumulative adalah data persisten
            // Anda mungkin perlu logika tambahan di sini jika OnEnable bisa terpanggil beberapa kali untuk hasil yang sama
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
        else
        {
            Debug.LogError("BeatmapDataAssign belum di-assign di WinMenu!");
        }
    }

    IEnumerator StartAllConfiguredAnimations()
    {
        // Score
        if (scoreAnimationSettings != null && scoreAnimationSettings.textMeshElement != null)
        {
            yield return new WaitForSeconds(scoreAnimationSettings.delayBeforeStart);
            StartCoroutine(AnimateIntNumberCoroutine(
                scoreAnimationSettings.textMeshElement,
                targetScore,
                scoreAnimationSettings.transitionDuration,
                scoreAnimationSettings.prefix
            ));
        }

        // Accuracy
        if (accuracyAnimationSettings != null && accuracyAnimationSettings.textMeshElement != null)
        {
            yield return new WaitForSeconds(accuracyAnimationSettings.delayBeforeStart);
            StartCoroutine(AnimateFloatNumberCoroutine(
                accuracyAnimationSettings.textMeshElement,
                targetAccuracy,
                accuracyAnimationSettings.transitionDuration,
                accuracyAnimationSettings.floatFormat,
                accuracyAnimationSettings.prefix,
                accuracyAnimationSettings.suffix
            ));
        }

        // Perfect Hits
        if (perfectHitsAnimationSettings != null && perfectHitsAnimationSettings.textMeshElement != null)
        {
            yield return new WaitForSeconds(perfectHitsAnimationSettings.delayBeforeStart);
            StartCoroutine(AnimateIntNumberCoroutine(
                perfectHitsAnimationSettings.textMeshElement,
                targetPerfectHits,
                perfectHitsAnimationSettings.transitionDuration,
                perfectHitsAnimationSettings.prefix
            ));
        }

        // Good Hits
        if (goodHitsAnimationSettings != null && goodHitsAnimationSettings.textMeshElement != null)
        {
            yield return new WaitForSeconds(goodHitsAnimationSettings.delayBeforeStart);
            StartCoroutine(AnimateIntNumberCoroutine(
                goodHitsAnimationSettings.textMeshElement,
                targetGoodHits,
                goodHitsAnimationSettings.transitionDuration,
                goodHitsAnimationSettings.prefix
            ));
        }

        // Bad Hits
        if (badHitsAnimationSettings != null && badHitsAnimationSettings.textMeshElement != null)
        {
            yield return new WaitForSeconds(badHitsAnimationSettings.delayBeforeStart);
            StartCoroutine(AnimateIntNumberCoroutine(
                badHitsAnimationSettings.textMeshElement,
                targetBadHits,
                badHitsAnimationSettings.transitionDuration,
                badHitsAnimationSettings.prefix
            ));
        }

        // Miss Hits
        if (missHitsAnimationSettings != null && missHitsAnimationSettings.textMeshElement != null)
        {
            yield return new WaitForSeconds(missHitsAnimationSettings.delayBeforeStart);
            StartCoroutine(AnimateIntNumberCoroutine(
                missHitsAnimationSettings.textMeshElement,
                targetMissHits,
                missHitsAnimationSettings.transitionDuration,
                missHitsAnimationSettings.prefix
            ));
        }
    }

    IEnumerator AnimateIntNumberCoroutine(TMP_Text textMesh, int targetValue, float transitionDuration, string prefix = "")
    {
        if (textMesh == null) yield break;

        float currentValue = 0;
        // Menggunakan SetText untuk kompatibilitas yang lebih baik dan performa potensial
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

    IEnumerator AnimateFloatNumberCoroutine(TMP_Text textMesh, float targetValue, float transitionDuration, string format = "F2", string prefix = "", string suffix = "")
    {
        if (textMesh == null) yield break;

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
