using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video; // Diperlukan untuk komponen Video
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

// Pastikan kelas WeightedBuffPrefab sudah ada dan dapat diakses oleh skrip ini.

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
        if (canPressKey && Input.GetKeyDown(KeyCode.K))
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

                // Panggil ChangeScene dari WinState untuk transisi yang benar
                if (winState != null)
                {
                    // Nama scene bisa Anda atur di sini, saya menggunakan "BuffPicker" sesuai skrip lama Anda
                    winState.ChangeScene("BuffPicker");
                }
                else
                {
                    Debug.LogError("WinState belum di-assign! Memuat scene secara langsung.", this);
                    SceneManager.LoadScene("BuffPicker"); // Fallback jika WinState tidak ada
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
        canPressKey = false; // Kunci input pemain
        videoPlayer.clip = rouletteVideo;
        videoPlayer.isLooping = false; // Pastikan video roulette tidak looping
        videoPlayer.Play();
        Debug.Log("Memutar video roulette...");
    }

    // Fungsi ini akan dipanggil secara otomatis oleh event 'loopPointReached' dari VideoPlayer
    void OnVideoFinished(VideoPlayer source)
    {
        // Cek apakah video yang baru saja selesai adalah video roulette
        if (source.clip == rouletteVideo)
        {
            Debug.Log("Video roulette selesai. Memutar video hasil...");

            DebuffLoader debuffLoader = selectedWeightedDebuff.prefab.GetComponent<DebuffLoader>();
            if (debuffLoader != null && debuffLoader.resultVideo != null)
            {
                // Ganti klip ke video hasil dari prefab debuff yang terpilih
                source.clip = debuffLoader.resultVideo;
                source.isLooping = true; // Buat video hasil looping agar tetap tampil
                source.Play();

                // Izinkan pemain menekan tombol 'K' SEKARANG
                canPressKey = true;
                Debug.Log("Input diizinkan. Pemain bisa menekan 'K'.");
            }
            else
            {
                Debug.LogError($"Prefab '{selectedWeightedDebuff.prefab.name}' tidak memiliki komponen DebuffLoader atau 'resultVideo'-nya kosong.", this);
                // Jika tidak ada video hasil, izinkan input agar tidak macet
                canPressKey = true;
            }
        }
        // Jika video yang selesai bukan video roulette (berarti video hasil yang sedang looping),
        // tidak perlu melakukan apa-apa, biarkan saja terus looping.
    }

    // Fungsi GetRandomWeightedIndex tetap sama
    private int GetRandomWeightedIndex(List<WeightedBuffPrefab> weightedItems)
    {
        if (weightedItems == null || weightedItems.Count == 0)
        {
            Debug.LogWarning("GetRandomWeightedIndex: Daftar item kosong.");
            return -1;
        }

        List<WeightedBuffPrefab> validItems = weightedItems.Where(item => item.prefab != null && item.weight > 0).ToList();
        if (validItems.Count == 0)
        {
            Debug.LogWarning("GetRandomWeightedIndex: Tidak ada item valid (prefab non-null dengan bobot > 0) untuk dipilih.");
            return -1;
        }

        float totalWeight = validItems.Sum(item => item.weight);

        if (totalWeight <= 0f)
        {
            Debug.LogWarning("GetRandomWeightedIndex: Total bobot 0 atau kurang. Memilih secara uniform dari item valid.");
            return weightedItems.IndexOf(validItems[Random.Range(0, validItems.Count)]);
        }

        float randomNumber = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        for (int i = 0; i < validItems.Count; i++)
        {
            cumulativeWeight += validItems[i].weight;
            if (randomNumber <= cumulativeWeight)
            {
                return weightedItems.IndexOf(validItems[i]);
            }
        }

        Debug.LogWarning("GetRandomWeightedIndex: Fallback, memilih item valid terakhir.");
        return weightedItems.IndexOf(validItems[validItems.Count - 1]);
    }
}
