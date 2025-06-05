using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.Events; // Tidak terpakai secara eksplisit
// using UnityEngine.EventSystems; // Tidak terpakai secara eksplisit
using System.Collections.Generic;
using System.Linq; // Diperlukan untuk .Where() dan .Sum()
using UnityEngine.SceneManagement;

// Anda dapat menggunakan kembali kelas WeightedBuffPrefab yang sudah ada
// jika Anda telah menempatkannya di file terpisah atau dapat diakses secara global.
// Jika belum, Anda bisa mendefinisikannya lagi di sini atau (lebih baik)
// pastikan ia ada di satu tempat dan direferensikan oleh kedua skrip.
// Untuk contoh ini, saya anggap WeightedBuffPrefab sudah ada dan dapat diakses.
// [System.Serializable]
// public class WeightedBuffPrefab // Jika perlu didefinisikan ulang
// {
// public GameObject prefab;
// public float weight = 1f;
// }

public class DebuffMenuController : MonoBehaviour
{
    // Ganti GameObject[] dengan List<WeightedBuffPrefab>
    // public GameObject[] buttonPrefabs; // Daftar prefab yang tersedia (LAMA)
    public List<WeightedBuffPrefab> debuffPrefabs = new List<WeightedBuffPrefab>(); // Daftar prefab debuff dengan bobot

    public Image imageDisplay; // Ubah nama variabel agar lebih jelas (sebelumnya 'image')
    public Text titleText;
    public Text descriptionText;

    private WeightedBuffPrefab selectedWeightedDebuff; // Menyimpan prefab yang dipilih beserta bobotnya

    void Start()
    {
        // Pastikan referensi tidak null
        if (debuffPrefabs == null || debuffPrefabs.Count == 0)
        {
            Debug.LogError("Daftar 'debuffPrefabs' kosong atau belum di-assign di Inspector!");
            return;
        }
        if (imageDisplay == null)
        {
            Debug.LogError("'Image Display' belum di-assign di Inspector!");
            return;
        }
        if (titleText == null)
        {
            Debug.LogError("'Title Text' belum di-assign di Inspector!");
            return;
        }
        if (descriptionText == null)
        {
            Debug.LogError("'Description Text' belum di-assign di Inspector!");
            return;
        }

        AssignRandomDebuff();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (selectedWeightedDebuff != null && selectedWeightedDebuff.prefab != null)
            {
                // Pastikan GameManager.Instance ada dan memiliki fungsi AddDebuff
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddDebuff(selectedWeightedDebuff.prefab);
                }
                else
                {
                    Debug.LogError("GameManager.Instance tidak ditemukan!");
                }
                SceneManager.LoadScene("Menu Lagu");
            }
            else
            {
                Debug.LogWarning("Tidak ada debuff terpilih untuk ditambahkan.");
            }
        }
    }

    void AssignRandomDebuff()
    {
        if (debuffPrefabs.Count == 0)
        {
            Debug.LogWarning("Tidak ada debuff di daftar 'debuffPrefabs'.");
            // Kosongkan UI jika tidak ada debuff
            if (imageDisplay != null) imageDisplay.sprite = null;
            if (titleText != null) titleText.text = "N/A";
            if (descriptionText != null) descriptionText.text = "Tidak ada debuff tersedia.";
            selectedWeightedDebuff = null;
            return;
        }

        // Dapatkan indeks acak berdasarkan bobot
        int randomIndex = GetRandomWeightedIndex(debuffPrefabs);

        if (randomIndex == -1)
        {
            Debug.LogError("Tidak dapat memilih debuff secara acak (mungkin semua bobot 0 atau daftar tidak valid).");
            selectedWeightedDebuff = null;
            // Anda mungkin ingin menampilkan pesan error di UI di sini
            if (imageDisplay != null) imageDisplay.sprite = null; // Atau gambar default error
            if (titleText != null) titleText.text = "Error";
            if (descriptionText != null) descriptionText.text = "Gagal memilih debuff.";
            return;
        }

        selectedWeightedDebuff = debuffPrefabs[randomIndex];

        // Pastikan prefab yang terpilih tidak null
        if (selectedWeightedDebuff.prefab == null)
        {
            Debug.LogError($"Prefab pada indeks {randomIndex} (setelah pemilihan berbobot) adalah null.");
            // Handle error, mungkin coba pilih lagi atau tampilkan error
            AssignRandomDebuff(); // Coba lagi (hati-hati bisa jadi infinite loop jika semua prefab bermasalah)
            return;
        }

        DebuffLoader debuffLoader = selectedWeightedDebuff.prefab.GetComponent<DebuffLoader>();

        if (debuffLoader != null)
        {
            if (imageDisplay != null) imageDisplay.sprite = debuffLoader.image;
            if (titleText != null) titleText.text = debuffLoader.cardTitleText;
            if (descriptionText != null) descriptionText.text = debuffLoader.cardDescriptionText;
        }
        else
        {
            Debug.LogError($"Prefab '{selectedWeightedDebuff.prefab.name}' tidak memiliki komponen DebuffLoader!");
            // Kosongkan UI atau tampilkan pesan error spesifik
            if (imageDisplay != null) imageDisplay.sprite = null;
            if (titleText != null) titleText.text = "Load Error";
            if (descriptionText != null) descriptionText.text = "Gagal memuat detail debuff.";
        }
    }

    /// <summary>
    /// Memilih indeks acak dari daftar WeightedBuffPrefab (atau item berbobot serupa) berdasarkan bobotnya.
    /// Fungsi ini identik dengan yang ada di MenuController, pastikan Anda memiliki satu versi yang konsisten.
    /// </summary>
    /// <param name="weightedItems">Daftar item berbobot.</param>
    /// <returns>Indeks item yang dipilih, atau -1 jika daftar kosong atau semua bobot nol/tidak valid.</returns>
    private int GetRandomWeightedIndex(List<WeightedBuffPrefab> weightedItems)
    {
        if (weightedItems == null || weightedItems.Count == 0)
        {
            Debug.LogWarning("GetRandomWeightedIndex: Daftar item kosong.");
            return -1;
        }

        // Filter item yang prefabnya null atau bobotnya tidak valid ( <= 0)
        List<WeightedBuffPrefab> validItems = weightedItems.Where(item => item.prefab != null && item.weight > 0).ToList();

        if (validItems.Count == 0)
        {
            Debug.LogWarning("GetRandomWeightedIndex: Tidak ada item valid (prefab non-null dengan bobot > 0) untuk dipilih.");
            return -1;
        }

        float totalWeight = 0f;
        foreach (var item in validItems)
        {
            totalWeight += item.weight;
        }

        if (totalWeight <= 0f) // Seharusnya tidak terjadi jika ada item di validItems dengan weight > 0
        {
            Debug.LogWarning("GetRandomWeightedIndex: Total bobot adalah 0 atau kurang. Ini tidak seharusnya terjadi jika ada item valid. Memilih secara uniform dari item valid.");
             // Jika totalWeight 0 karena semua bobot valid sangat kecil, pilih secara acak dari item valid.
            return weightedItems.IndexOf(validItems[Random.Range(0, validItems.Count)]);
        }

        float randomNumber = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        for (int i = 0; i < validItems.Count; i++)
        {
            cumulativeWeight += validItems[i].weight;
            if (randomNumber <= cumulativeWeight)
            {
                // Kembalikan indeks dari daftar ASLI (weightedItems)
                return weightedItems.IndexOf(validItems[i]);
            }
        }

        // Fallback, seharusnya jarang tercapai jika logika benar dan bobot positif.
        // Bisa terjadi jika ada masalah presisi float atau jika randomNumber tepat sama dengan totalWeight.
        Debug.LogWarning("GetRandomWeightedIndex: Fallback, memilih item valid terakhir.");
        if (validItems.Count > 0) // Pastikan ada item valid
            return weightedItems.IndexOf(validItems[validItems.Count - 1]);
        
        return -1; // Jika setelah fallback pun tidak ada
    }

    // Fungsi SetupWeights() dan GetWeightedRandomIndex(List<float> weights) yang lama sudah tidak diperlukan lagi
    // karena bobot sekarang dikelola melalui List<WeightedBuffPrefab> dan fungsi GetRandomWeightedIndex yang baru.
}
