using UnityEngine;
using System.Collections.Generic;
using System.IO; // Untuk operasi file
using System.Linq; // Untuk OrderByDescending, Take
using System.Text.RegularExpressions; // Untuk parsing nama pemain
using UnityEngine.UI; // Jika Anda ingin contoh display sederhana

// Struktur data untuk setiap entri di leaderboard
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
    public static LeaderboardManager Instance { get; private set; }

    private List<LeaderboardEntry> allLeaderboardEntries = new List<LeaderboardEntry>(); // Menyimpan SEMUA entri
    private const int MaxLeaderboardEntriesToDisplay = 10; // Hanya untuk tampilan
    private const string LeaderboardFileName = "leaderboard.txt"; // Nama file teks

    private string filePath;
    private int lastPlayerNumberFromFile = 0; // Untuk melacak nomor pemain terakhir dari file

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        filePath = Path.Combine(Application.persistentDataPath, LeaderboardFileName);
        Debug.Log("Leaderboard file path: " + filePath);

        LoadLeaderboard(); // Muat semua entri dan tentukan lastPlayerNumberFromFile
    }

    /// <summary>
    /// Menghasilkan nama pemain berikutnya secara berurutan (misal: "001", "002")
    /// berdasarkan nomor terakhir yang ditemukan di file leaderboard.
    /// </summary>
    /// <returns>Nama pemain yang digenerate.</returns>
    public string GetNextPlayerName()
    {
        lastPlayerNumberFromFile++; // Increment nomor terakhir yang diketahui
        return lastPlayerNumberFromFile.ToString("D3"); // Format menjadi 3 digit
    }

    /// <summary>
    /// Memuat SEMUA data leaderboard dari file teks dan menentukan nomor pemain terakhir.
    /// </summary>
    public void LoadLeaderboard()
    {
        allLeaderboardEntries.Clear();
        int maxNumberFound = 0; // Untuk melacak nomor pemain tertinggi dari file

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

                            // Coba ekstrak nomor dari nama pemain jika formatnya "NNN"
                            Match match = Regex.Match(name, @"^\d+$"); // Cocokkan jika nama hanya terdiri dari angka
                            if (match.Success && int.TryParse(name, out int playerNumber))
                            {
                                if (playerNumber > maxNumberFound)
                                {
                                    maxNumberFound = playerNumber;
                                }
                            }
                        }
                        else Debug.LogWarning($"Gagal parse skor dari baris: {line}");
                    }
                    else Debug.LogWarning($"Format baris tidak valid: {line}");
                }
                lastPlayerNumberFromFile = maxNumberFound; // Set nomor terakhir berdasarkan file
                // Tidak perlu mengurutkan di sini jika kita menyimpan semua data
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Gagal memuat leaderboard: {ex.Message}");
                lastPlayerNumberFromFile = 0; // Reset jika ada error saat load
            }
        }
        else
        {
            Debug.Log("File leaderboard tidak ditemukan. Akan dibuat saat skor pertama disimpan. Player akan mulai dari 001.");
            lastPlayerNumberFromFile = 0; // Jika file tidak ada, pemain pertama adalah 001 (counter akan jadi 1)
        }
    }

    /// <summary>
    /// Menyimpan SEMUA data leaderboard ke file teks.
    /// </summary>
    public void SaveLeaderboard()
    {
        // Opsi: Urutkan semua entri sebelum menyimpan jika Anda ingin file-nya terurut,
        // meskipun tidak wajib karena kita urutkan lagi saat menampilkan.
        // allLeaderboardEntries = allLeaderboardEntries.OrderByDescending(entry => entry.score).ToList();

        try
        {
            List<string> lines = new List<string>();
            foreach (LeaderboardEntry entry in allLeaderboardEntries)
            {
                lines.Add($"{entry.playerName},{entry.score}");
            }
            File.WriteAllLines(filePath, lines);
            // Debug.Log("Semua entri leaderboard berhasil disimpan.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Gagal menyimpan leaderboard: {ex.Message}");
        }
    }

    /// <summary>
    /// Menambahkan skor baru ke daftar SEMUA entri leaderboard.
    /// Nama pemain akan digenerate secara otomatis.
    /// </summary>
    /// <param name="score">Skor yang dicapai pemain.</param>
    public void AddScore(int score)
    {
        string playerName = GetNextPlayerName();
        AddScore(playerName, score);
    }

    /// <summary>
    /// Menambahkan skor baru ke daftar SEMUA entri leaderboard dengan nama pemain yang ditentukan.
    /// </summary>
    /// <param name="playerName">Nama pemain.</param>
    /// <param name="score">Skor yang dicapai pemain.</param>
    public void AddScore(string playerName, int score)
    {
        allLeaderboardEntries.Add(new LeaderboardEntry(playerName, score));
        SaveLeaderboard(); // Simpan semua entri setelah menambahkan yang baru
        Debug.Log($"Skor ditambahkan ke daftar lengkap: {playerName} - {score}");
    }

    /// <summary>
    /// Mengambil daftar Top N entri leaderboard untuk ditampilkan.
    /// </summary>
    /// <returns>Daftar Top N entri leaderboard yang sudah diurutkan.</returns>
    public List<LeaderboardEntry> GetTopLeaderboardEntries()
    {
        // Urutkan semua entri berdasarkan skor (tertinggi dulu) lalu ambil N teratas
        return allLeaderboardEntries
                .OrderByDescending(entry => entry.score)
                .Take(MaxLeaderboardEntriesToDisplay)
                .ToList();
    }

    // --- Contoh untuk menampilkan leaderboard ke UI Text ---
    public Text[] rankUiTexts;
    public Text[] nameUiTexts;
    public Text[] scoreUiTexts;

    /// <summary>
    /// Contoh fungsi untuk menampilkan Top N leaderboard ke UI Text.
    /// </summary>
    public void DisplayLeaderboardOnUI()
    {
        // Tidak perlu LoadLeaderboard() di sini jika data sudah ada di allLeaderboardEntries
        // dan sudah di-load saat Awake atau setelah Save.
        // Namun, jika ada kemungkinan file diubah eksternal saat runtime, LoadLeaderboard() bisa dipanggil.

        List<LeaderboardEntry> topEntries = GetTopLeaderboardEntries(); // Dapatkan top N

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

    /// <summary>
    /// Opsional: Fungsi untuk mereset counter nama pemain jika file dihapus manual.
    /// Ini bisa dipanggil dari editor script atau tombol debug jika diperlukan.
    /// </summary>
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
}
