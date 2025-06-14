using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video; // Diperlukan untuk komponen Video
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

// Pastikan kelas WeightedBuffPrefab dan enum Rarity sudah ada dan dapat diakses
// public enum Rarity { Common, Epic, Legendary }
// [System.Serializable] public class WeightedBuffPrefab { ... }

public class DebuffMenuController : MonoBehaviour
{
    [Header("Data & Scene Flow")]
    public List<WeightedBuffPrefab> debuffPrefabs = new List<WeightedBuffPrefab>();
    public WinState winState; // Assign di Inspector untuk transisi scene

    [Header("Video Settings")]
    [Tooltip("Komponen VideoPlayer untuk memutar video.")]
    public VideoPlayer videoPlayer;
    [Tooltip("UI RawImage untuk menampilkan output video.")]
    public RawImage videoDisplay;
    [Tooltip("Video roulette yang akan diputar pertama kali.")]
    public VideoClip rouletteVideo; // Video "gacha"

    private WeightedBuffPrefab selectedWeightedDebuff;
    private bool canPressKey = false; // Flag untuk mengontrol input setelah video pertama

    void Start()
    {
        // Validasi komponen yang diperlukan
        if (debuffPrefabs == null || debuffPrefabs.Count == 0)
        {
            Debug.LogError("Daftar 'debuffPrefabs' kosong atau belum di-assign di Inspector!", this);
            return;
        }
        if (videoPlayer == null || videoDisplay == null || rouletteVideo == null)
        {
            Debug.LogError("Komponen Video (Player, Display, atau Roulette Clip) belum di-assign!", this);
            return;
        }

        // Mendaftarkan event listener untuk ketika video selesai
        videoPlayer.loopPointReached += OnVideoFinished;

        AssignRandomDebuffAndPlay();
    }

    void OnDestroy()
    {
        // Membersihkan event listener untuk mencegah memory leak
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }

    void Update()
    {
        // Hanya izinkan input 'K' jika flag canPressKey adalah true
        if (canPressKey && Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.J))
        {
            canPressKey = false; // Nonaktifkan input lagi untuk mencegah penekanan berulang

            if (selectedWeightedDebuff != null && selectedWeightedDebuff.prefab != null)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddDebuff(selectedWeightedDebuff.prefab);
                }
                else
                {
                    Debug.LogError("GameManager.Instance tidak ditemukan!");
                }
                
                if (winState != null)
                {
                    // Menggunakan nama scene dari skrip lama Anda
                    winState.ChangeScene("Menu Lagu");
                }
                else
                {
                    Debug.LogError("WinState belum di-assign! Memuat scene secara langsung.", this);
                    SceneManager.LoadScene("Menu Lagu"); // Fallback
                }
            }
            else
            {
                Debug.LogWarning("Tidak ada debuff terpilih untuk ditambahkan.");
            }
        }
    }

    void AssignRandomDebuffAndPlay()
    {
        // Pilih debuff secara acak menggunakan sistem bobot
        int randomIndex = GetRandomWeightedIndex(debuffPrefabs);
        if (randomIndex == -1)
        {
            Debug.LogError("Gagal memilih debuff secara acak (mungkin semua bobot 0 atau daftar tidak valid).", this);
            return;
        }
        selectedWeightedDebuff = debuffPrefabs[randomIndex];
        if (selectedWeightedDebuff.prefab == null)
        {
             Debug.LogError($"Prefab pada indeks {randomIndex} adalah null.", this);
             return;
        }

        // Mulai memutar video pertama (roulette)
        canPressKey = false;
        videoPlayer.clip = rouletteVideo;
        videoPlayer.isLooping = false;
        videoPlayer.Play();
        Debug.Log("Memutar video roulette...");
    }
    
    // Fungsi ini sekarang menangani alur 3 video
    void OnVideoFinished(VideoPlayer source)
    {
        DebuffLoader debuffLoader = selectedWeightedDebuff.prefab.GetComponent<DebuffLoader>();

        // Cek jika video 1 (roulette) selesai
        if (source.clip == rouletteVideo)
        {
            Debug.Log("Video roulette selesai. Memutar video reveal (video 2)...");

            if (debuffLoader != null && debuffLoader.revealVideo != null)
            {
                // Putar video 2 (reveal), jangan di-loop
                source.clip = debuffLoader.revealVideo;
                source.isLooping = false;
                source.Play();
            }
            else
            {
                Debug.LogError($"Prefab '{selectedWeightedDebuff.prefab.name}' tidak memiliki DebuffLoader atau 'revealVideo'-nya kosong. Mencoba langsung ke video loop.", this);
                // Langsung coba putar video loop jika video reveal tidak ada
                PlayLoopingVideo(source, debuffLoader);
            }
        }
        // Cek jika video 2 (reveal) selesai
        else if (debuffLoader != null && source.clip == debuffLoader.revealVideo)
        {
            Debug.Log("Video reveal selesai. Memutar video hasil looping (video 3)...");
            PlayLoopingVideo(source, debuffLoader);
        }
    }

    // Fungsi helper baru untuk memulai video looping
    void PlayLoopingVideo(VideoPlayer source, DebuffLoader debuffLoader)
    {
        if (debuffLoader != null && debuffLoader.loopingResultVideo != null)
        {
            // Putar video 3 (looping result), atur agar looping
            source.clip = debuffLoader.loopingResultVideo;
            source.isLooping = true;
            source.Play();

            // Izinkan input SEKARANG
            canPressKey = true;
            Debug.Log("Input diizinkan. Pemain bisa menekan 'K'.");
        }
        else
        {
            Debug.LogError($"Prefab '{selectedWeightedDebuff.prefab.name}' tidak memiliki DebuffLoader atau 'loopingResultVideo'-nya kosong.", this);
            // Izinkan input agar tidak macet
            canPressKey = true;
        }
    }

    /// <summary>
    /// Mengonversi nilai enum Rarity menjadi bobot numerik.
    /// </summary>
    private float GetWeightFromRarity(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:    return 50f;
            case Rarity.Epic:      return 35f;
            case Rarity.Legendary: return 15f;
            default:               return 1f;
        }
    }

    /// <summary>
    /// Memilih indeks acak dari daftar berdasarkan Rarity.
    /// </summary>
    private int GetRandomWeightedIndex(List<WeightedBuffPrefab> weightedList)
    {
        if (weightedList == null || weightedList.Count == 0) return -1;
        
        // Buat daftar sementara yang berisi item dan bobot kalkulasinya
        var calculatedWeights = weightedList
            .Select(item => new {
                Item = item,
                Weight = GetWeightFromRarity(item.rarity) // Menggunakan fungsi helper baru
            })
            .Where(x => x.Item.prefab != null && x.Weight > 0)
            .ToList();

        if (calculatedWeights.Count == 0)
        {
            Debug.LogWarning("Tidak ada item yang bisa dipilih setelah kalkulasi bobot.");
            return -1;
        }
        
        float totalWeight = calculatedWeights.Sum(x => x.Weight);
        if (totalWeight <= 0) return -1;

        float randomNumber = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        foreach (var weightedItem in calculatedWeights)
        {
            cumulativeWeight += weightedItem.Weight;
            if (randomNumber <= cumulativeWeight)
            {
                // Kembalikan indeks item dari daftar asli
                return weightedList.IndexOf(weightedItem.Item);
            }
        }

        return weightedList.IndexOf(calculatedWeights.Last().Item); // Fallback
    }
}
