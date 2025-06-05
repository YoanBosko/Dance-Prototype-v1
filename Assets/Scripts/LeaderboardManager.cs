using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.UI;

// Struktur data LeaderboardEntry tetap sama
[System.Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public int score;

    public LeaderboardEntry(string name, int scr)
    {
        playerName = name;
        score = scr;
    }
}

public class LeaderboardManager : MonoBehaviour
{
    // Singleton dan DontDestroyOnLoad dihapus
    public static LeaderboardManager Instance { get; private set; }

    private List<LeaderboardEntry> allLeaderboardEntries = new List<LeaderboardEntry>();
    private const int MaxLeaderboardEntriesToDisplay = 10;
    private const string LeaderboardFileName = "leaderboard.txt";

    private string filePath;
    private int lastPlayerNumberFromFile = 0;

    [Header("UI References")]
    public CanvasGroup leaderboardCanvasGroup; // Hubungkan CanvasGroup dari UI Leaderboard
    public Text[] rankUiTexts;
    public Text[] nameUiTexts;
    public Text[] scoreUiTexts;

    [Header("Interval and Fade Settings")]
    public float initialDelayBeforeFirstCycle = 2.0f; // Waktu tunda sebelum siklus pertama dimulai
    public float timeLeaderboardHidden = 15.0f; // Waktu leaderboard disembunyikan antar tampilan
    public float timeLeaderboardVisible = 10.0f; // Waktu leaderboard terlihat setelah fade-in
    public float fadeDuration = 0.5f;       // Durasi untuk fade in/out

    private Coroutine leaderboardCycleCoroutine; // Untuk melacak coroutine siklus interval
    private Coroutine activeFadeCoroutine;     // Untuk melacak coroutine fade yang sedang berjalan
    private bool isIntervalSystemActive = false;

    public ScoreData scoreDataCummulative;

    void Awake()
    {
        Instance = this;
        // Pengaturan path file dan memuat leaderboard
        filePath = Path.Combine(Application.persistentDataPath, LeaderboardFileName);
        Debug.Log("Leaderboard file path: " + filePath);
        LoadLeaderboard();

        // Pastikan CanvasGroup ada dan di-set menjadi tidak terlihat pada awalnya
        if (leaderboardCanvasGroup != null)
        {
            leaderboardCanvasGroup.alpha = 0f; // Mulai dengan transparan
            leaderboardCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            Debug.LogError("Leaderboard CanvasGroup belum di-assign di Inspector pada LeaderboardManager!");
        }
    }

    void Start()
    {
        // Aktifkan sistem interval secara default
        ActivateIntervalDisplay();
    }

    // Fungsi untuk mengaktifkan sistem interval tampilan leaderboard
    public void ActivateIntervalDisplay()
    {
        if (!isIntervalSystemActive)
        {
            Debug.Log("Mengaktifkan sistem interval Leaderboard.");
            isIntervalSystemActive = true;
            if (leaderboardCycleCoroutine != null)
            {
                StopCoroutine(leaderboardCycleCoroutine);
            }
            leaderboardCycleCoroutine = StartCoroutine(LeaderboardIntervalCycle());
        }
    }

    // Fungsi untuk menonaktifkan sistem interval tampilan leaderboard
    public void DeactivateIntervalDisplay()
    {
        if (isIntervalSystemActive)
        {
            Debug.Log("Menonaktifkan sistem interval Leaderboard.");
            isIntervalSystemActive = false;
            if (leaderboardCycleCoroutine != null)
            {
                StopCoroutine(leaderboardCycleCoroutine);
                leaderboardCycleCoroutine = null;
            }
            // Hentikan fade yang mungkin sedang berjalan dan pastikan leaderboard fade out
            if (activeFadeCoroutine != null)
            {
                StopCoroutine(activeFadeCoroutine);
                activeFadeCoroutine = null;
            }
            if (leaderboardCanvasGroup != null && leaderboardCanvasGroup.alpha > 0)
            {
                // Langsung mulai fade out tanpa menyimpannya ke activeFadeCoroutine
                // karena ini adalah aksi final untuk menyembunyikan.
                StartCoroutine(PerformFade(0f, true)); // true untuk memastikan blocksRaycasts di-set
            }
        }
    }

    // Coroutine utama untuk siklus interval penampilan leaderboard
    private IEnumerator LeaderboardIntervalCycle()
    {
        yield return new WaitForSeconds(initialDelayBeforeFirstCycle); // Tunda sebelum siklus pertama

        while (isIntervalSystemActive)
        {
            // 1. Waktu leaderboard disembunyikan
            if (leaderboardCanvasGroup.alpha > 0) // Jika masih terlihat dari siklus sebelumnya (seharusnya tidak jika Deactivate benar)
            {
                 if (activeFadeCoroutine != null) StopCoroutine(activeFadeCoroutine);
                 activeFadeCoroutine = StartCoroutine(PerformFade(0f));
                 yield return activeFadeCoroutine; // Tunggu fade out selesai
                 activeFadeCoroutine = null;
            }
            yield return new WaitForSeconds(timeLeaderboardHidden);

            if (!isIntervalSystemActive) break; // Keluar jika sistem dinonaktifkan saat menunggu

            // 2. Update teks dan Fade In
            UpdateLeaderboardUITexts();
            if (activeFadeCoroutine != null) StopCoroutine(activeFadeCoroutine);
            activeFadeCoroutine = StartCoroutine(PerformFade(1f));
            yield return activeFadeCoroutine; // Tunggu fade in selesai
            activeFadeCoroutine = null;

            if (!isIntervalSystemActive) break;

            // 3. Waktu leaderboard terlihat
            yield return new WaitForSeconds(timeLeaderboardVisible);

            if (!isIntervalSystemActive) break;

            // 4. Fade Out (jika sistem masih aktif)
            if (leaderboardCanvasGroup.alpha > 0) // Hanya fade out jika memang terlihat
            {
                if (activeFadeCoroutine != null) StopCoroutine(activeFadeCoroutine);
                activeFadeCoroutine = StartCoroutine(PerformFade(0f));
                yield return activeFadeCoroutine; // Tunggu fade out selesai
                activeFadeCoroutine = null;
            }
        }
        leaderboardCycleCoroutine = null; // Bersihkan referensi coroutine saat loop berakhir
    }


    // Fungsi GetNextPlayerName, LoadLeaderboard, SaveLeaderboard, AddScore (kedua versi), GetTopLeaderboardEntries tetap sama
    public string GetNextPlayerName()
    {
        lastPlayerNumberFromFile++;
        return lastPlayerNumberFromFile.ToString("D3");
    }

    public void LoadLeaderboard()
    {
        allLeaderboardEntries.Clear();
        int maxNumberFound = 0;

        if (File.Exists(filePath))
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] parts = line.Split(',');
                    if (parts.Length == 2)
                    {
                        string name = parts[0].Trim();
                        if (int.TryParse(parts[1].Trim(), out int score))
                        {
                            allLeaderboardEntries.Add(new LeaderboardEntry(name, score));
                            Match match = Regex.Match(name, @"^\d+$");
                            if (match.Success && int.TryParse(name, out int playerNumber))
                            {
                                if (playerNumber > maxNumberFound) maxNumberFound = playerNumber;
                            }
                        }
                        else Debug.LogWarning($"Gagal parse skor dari baris: {line}");
                    }
                    else Debug.LogWarning($"Format baris tidak valid: {line}");
                }
                lastPlayerNumberFromFile = maxNumberFound;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Gagal memuat leaderboard: {ex.Message}");
                lastPlayerNumberFromFile = 0;
            }
        }
        else
        {
            Debug.Log("File leaderboard tidak ditemukan. Player akan mulai dari 001.");
            lastPlayerNumberFromFile = 0;
        }
    }

    public void SaveLeaderboard()
    {
        try
        {
            List<string> lines = new List<string>();
            foreach (LeaderboardEntry entry in allLeaderboardEntries)
            {
                lines.Add($"{entry.playerName},{entry.score}");
            }
            File.WriteAllLines(filePath, lines);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Gagal menyimpan leaderboard: {ex.Message}");
        }
    }

    public void AddScore(int score)
    {
        string playerName = GetNextPlayerName();
        AddScore(playerName, score);
    }

    public void AddScore(string playerName, int score)
    {
        allLeaderboardEntries.Add(new LeaderboardEntry(playerName, score));
        SaveLeaderboard();
        Debug.Log($"Skor ditambahkan ke daftar lengkap: {playerName} - {score}");
    }

    public List<LeaderboardEntry> GetTopLeaderboardEntries()
    {
        return allLeaderboardEntries
                .OrderByDescending(entry => entry.score)
                .Take(MaxLeaderboardEntriesToDisplay)
                .ToList();
    }

    private void UpdateLeaderboardUITexts()
    {
        List<LeaderboardEntry> topEntries = GetTopLeaderboardEntries();
        int displayCount = Mathf.Min(topEntries.Count, nameUiTexts.Length);

        for (int i = 0; i < displayCount; i++)
        {
            if (rankUiTexts.Length > i && rankUiTexts[i] != null) rankUiTexts[i].text = (i + 1).ToString() + ".";
            if (nameUiTexts.Length > i && nameUiTexts[i] != null) nameUiTexts[i].text = topEntries[i].playerName;
            if (scoreUiTexts.Length > i && scoreUiTexts[i] != null) scoreUiTexts[i].text = topEntries[i].score.ToString();
        }

        for (int i = displayCount; i < nameUiTexts.Length; i++)
        {
            if (rankUiTexts.Length > i && rankUiTexts[i] != null) rankUiTexts[i].text = "";
            if (nameUiTexts.Length > i && nameUiTexts[i] != null) nameUiTexts[i].text = "";
            if (scoreUiTexts.Length > i && scoreUiTexts[i] != null) scoreUiTexts[i].text = "";
        }
    }

    // Modifikasi PerformFade untuk menerima parameter opsional guna memastikan blocksRaycasts diatur setelah fade-out paksa
    private IEnumerator PerformFade(float targetAlpha, bool forceBlockRaycastsOff = false)
    {
        if (leaderboardCanvasGroup == null)
        {
            Debug.LogError("Leaderboard CanvasGroup tidak ada untuk PerformFade.");
            yield break;
        }

        // Nonaktifkan interaksi selama fade jika targetnya transparan atau sedang menuju transparan
        if (targetAlpha < 0.1f || leaderboardCanvasGroup.alpha > targetAlpha) {
            leaderboardCanvasGroup.blocksRaycasts = false;
        }

        float startAlpha = leaderboardCanvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            if (leaderboardCanvasGroup == null) yield break; // Cek lagi jika objek dihancurkan selama fade
            time += Time.deltaTime;
            leaderboardCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        if (leaderboardCanvasGroup == null) yield break;
        leaderboardCanvasGroup.alpha = targetAlpha;

        if (targetAlpha > 0.9f) // Jika fade in selesai
        {
            leaderboardCanvasGroup.blocksRaycasts = true; // Aktifkan kembali interaksi
        }
        else if (forceBlockRaycastsOff || targetAlpha < 0.1f) // Jika fade out selesai atau dipaksa
        {
            leaderboardCanvasGroup.blocksRaycasts = false;
        }
        // Jika targetAlpha di antara (misal 0.5), biarkan blocksRaycasts seperti kondisi terakhirnya.
    }

    // Fungsi ResetPlayerNameCounterBasedOnLoadedData tetap sama
    public void ResetPlayerNameCounterBasedOnLoadedData() {
        int maxNumberFound = 0;
        foreach(var entry in allLeaderboardEntries) {
            Match match = Regex.Match(entry.playerName, @"^\d+$");
            if (match.Success && int.TryParse(entry.playerName, out int playerNumber)) {
                if (playerNumber > maxNumberFound) {
                    maxNumberFound = playerNumber;
                }
            }
        }
        lastPlayerNumberFromFile = maxNumberFound;
        Debug.Log($"Player name counter reset based on file. Last number found: {lastPlayerNumberFromFile}");
    }

    // Fungsi yang tidak lagi relevan dari versi sebelumnya
    // public void DisplayLeaderboardOnUI() // Digantikan oleh sistem interval
    // private IEnumerator FadeSequence() // Logikanya terintegrasi di LeaderboardIntervalCycle
}
