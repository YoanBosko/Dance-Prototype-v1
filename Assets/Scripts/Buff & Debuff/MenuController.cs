using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events; // Tidak terpakai secara eksplisit di sini, tapi baik untuk ada jika diperlukan
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq; // Diperlukan untuk .Sum() dan .Any()
using UnityEngine.SceneManagement;

// Langkah 1: Definisikan kelas untuk menyimpan prefab dan bobotnya.
// Anda bisa meletakkan ini di file terpisah (misal: WeightedBuffPrefab.cs)
// atau di bagian atas file PrefabMenuController.cs atau MenuController.cs (di luar kelas lain).
[System.Serializable] // Agar bisa dilihat dan di-edit di Inspector Unity
public class WeightedBuffPrefab
{
    public GameObject prefab; // Prefab buff
    public float weight = 1f; // Bobot. Semakin tinggi, semakin besar kemungkinan terpilih.
                              // Pastikan bobot selalu positif.
}

// Langkah 3: Modifikasi MenuController Anda
public class MenuController : MonoBehaviour
{
    public PrefabMenuController buttonPrefabs; // Referensi ke PrefabMenuController
    public Transform buttonParent; // Parent untuk menampung instansiasi button
    public Vector3[] buttonPositions; // Posisi untuk menampilkan button (pastikan ada setidaknya 3)

    public Button[] menuButtons; // Button aktif yang bisa dikontrol (akan diisi oleh PickRandomButtons)
    private int selectedIndex = 0;
    public float highlightScale = 1.2f;
    public float transitionSpeed = 10f;

    void Start()
    {
        // Pastikan referensi tidak null
        if (buttonPrefabs == null)
        {
            Debug.LogError("PrefabMenuController (buttonPrefabs) belum di-assign di Inspector!");
            return;
        }
        if (buttonParent == null)
        {
            Debug.LogError("Button Parent belum di-assign di Inspector!");
            return;
        }
        if (buttonPositions == null || buttonPositions.Length < 3)
        {
             Debug.LogError("Button Positions tidak cukup atau belum di-assign. Butuh setidaknya 3 posisi.");
            return;
        }


        // Reset buff jika cycleTime adalah 1
        if (GameManager.Instance != null && GameManager.Instance.cycleTime == 1)
        {
            // Mengembalikan buff yang telah dihapus kembali ke daftar yang tersedia
            foreach (WeightedBuffPrefab weightedBuff in buttonPrefabs.removedBuffs)
            {
                // Cek apakah prefab dari weightedBuff sudah ada di availableBuffs (berdasarkan GameObject prefabnya)
                // Ini untuk menghindari duplikasi jika ada logika lain yang mungkin menambahkan kembali.
                if (!buttonPrefabs.availableBuffs.Any(wb => wb.prefab == weightedBuff.prefab))
                {
                    buttonPrefabs.availableBuffs.Add(weightedBuff);
                }
            }
            buttonPrefabs.removedBuffs.Clear(); // Kosongkan daftar yang dihapus
        }

        PickRandomButtons(); // Pilih tombol secara acak (dengan bobot)

        // Hanya highlight jika ada tombol yang berhasil dibuat
        if (menuButtons != null && menuButtons.Length > 0 && menuButtons[0] != null)
        {
             HighlightButton();
        }
        else if (menuButtons != null && menuButtons.Length > 0)
        {
            Debug.LogWarning("menuButtons array ada, tapi elemen pertama null. Tidak bisa highlight.");
        }
        else
        {
            Debug.LogWarning("Tidak ada tombol untuk di-highlight setelah PickRandomButtons.");
        }
    }

    void Update()
    {
        // Pastikan menuButtons tidak null dan memiliki elemen sebelum diakses
        if (menuButtons == null || menuButtons.Length == 0)
        {
            return; // Tidak ada tombol untuk dikontrol
        }

        // Navigasi
        if (Input.GetKeyDown(KeyCode.J))
        {
            selectedIndex = (selectedIndex + 1) % menuButtons.Length;
            HighlightButton();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            selectedIndex = (selectedIndex - 1 + menuButtons.Length) % menuButtons.Length;
            HighlightButton();
        }
        // Aksi pemilihan
        else if (Input.GetKeyDown(KeyCode.K))
        {
            if (menuButtons[selectedIndex] == null) {
                Debug.LogWarning($"Tombol pada selectedIndex {selectedIndex} adalah null. Tidak bisa melakukan aksi.");
                return;
            }

            menuButtons[selectedIndex].onClick.Invoke(); // Panggil event onClick pada tombol yang dipilih

            // Dapatkan nama prefab asli dari nama GameObject yang di-instantiate
            // Nama instance biasanya "[NamaPrefabAsli](Clone)"
            string selectedButtonInstanceName = menuButtons[selectedIndex].gameObject.name;
            string originalPrefabName = selectedButtonInstanceName.Replace("(Clone)", "").Trim();

            // Cari WeightedBuffPrefab yang sesuai di availableBuffs berdasarkan nama prefabnya
            WeightedBuffPrefab selectedWeightedBuff = buttonPrefabs.availableBuffs.Find(wb => wb.prefab != null && wb.prefab.name == originalPrefabName);

            if (selectedWeightedBuff != null)
            {
                // Pindahkan dari daftar tersedia ke daftar yang sudah dihapus/digunakan
                buttonPrefabs.availableBuffs.Remove(selectedWeightedBuff);
                buttonPrefabs.removedBuffs.Add(selectedWeightedBuff);
            }
            else
            {
                Debug.LogWarning($"WeightedBuffPrefab dengan nama prefab '{originalPrefabName}' tidak ditemukan di availableBuffs. Mungkin sudah dipilih atau ada kesalahan nama.");
            }

            // Ganti scene berdasarkan cycleTime
            if (GameManager.Instance != null && GameManager.Instance.cycleTime == 2)
            {
                SceneManager.LoadScene("DebuffScene");
            }
            else
            {
                SceneManager.LoadScene("Menu Lagu");
            }
        }

        UpdateButtonScales(); // Update skala tombol (efek visual)
    }

    void HighlightButton()
    {
        // Pastikan menuButtons dan selectedIndex valid
        if (menuButtons == null || menuButtons.Length == 0 || selectedIndex < 0 || selectedIndex >= menuButtons.Length || menuButtons[selectedIndex] == null)
        {
            // Debug.LogWarning("HighlightButton: Kondisi tidak valid untuk highlight tombol.");
            return;
        }

        EventSystem.current.SetSelectedGameObject(menuButtons[selectedIndex].gameObject);

        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] != null) // Pastikan tombol tidak null
            {
                BuffLoader buffLoader = menuButtons[i].GetComponent<BuffLoader>();
                if (buffLoader != null) // Pastikan BuffLoader ada
                {
                    if (i == selectedIndex)
                    {
                        buffLoader.ActivateMoreDescription();
                    }
                    else
                    {
                        buffLoader.DeactivateMoreDescription();
                    }
                }
                else
                {
                    // Debug.LogWarning($"Tombol '{menuButtons[i].name}' tidak memiliki komponen BuffLoader.");
                }
            }
        }
    }

    void UpdateButtonScales()
    {
        if (menuButtons == null) return;

        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] != null) // Pastikan tombol tidak null
            {
                Vector3 targetScale = (i == selectedIndex) ? new Vector3(highlightScale, highlightScale, 1f) : Vector3.one;
                menuButtons[i].transform.localScale = Vector3.Lerp(
                    menuButtons[i].transform.localScale,
                    targetScale,
                    Time.deltaTime * transitionSpeed
                );
            }
        }
    }

    /// <summary>
    /// Memilih indeks acak dari daftar WeightedBuffPrefab berdasarkan bobotnya.
    /// </summary>
    /// <param name="weightedList">Daftar item berbobot.</param>
    /// <returns>Indeks item yang dipilih, atau -1 jika daftar kosong atau semua bobot nol.</returns>
    private int GetRandomWeightedIndex(List<WeightedBuffPrefab> weightedList)
    {
        if (weightedList == null || weightedList.Count == 0)
        {
            Debug.LogWarning("GetRandomWeightedIndex: Daftar kosong.");
            return -1; // Tidak ada yang bisa dipilih
        }

        // Filter item yang prefabnya null atau bobotnya tidak valid ( <= 0)
        List<WeightedBuffPrefab> validItems = weightedList.Where(item => item.prefab != null && item.weight > 0).ToList();

        if (validItems.Count == 0)
        {
            Debug.LogWarning("GetRandomWeightedIndex: Tidak ada item valid (prefab non-null dengan bobot > 0) untuk dipilih.");
            // Jika tidak ada item valid, mungkin kita pilih secara acak dari yang ada jika hanya bobotnya 0 semua (tapi prefab ada)
            // Untuk sekarang, kita return -1
            return -1;
        }


        float totalWeight = 0f;
        foreach (var item in validItems)
        {
            totalWeight += item.weight;
        }

        // Jika totalWeight masih 0 (misalnya semua bobot valid adalah sangat kecil hingga dianggap 0 float),
        // maka pilih secara uniform dari item valid.
        if (totalWeight <= 0f)
        {
            Debug.LogWarning("GetRandomWeightedIndex: Total bobot adalah 0 atau kurang. Memilih secara uniform dari item valid.");
            if (validItems.Count > 0)
                return weightedList.IndexOf(validItems[Random.Range(0, validItems.Count)]); // Dapatkan indeks asli dari daftar awal
            return -1; // Seharusnya tidak sampai sini jika validItems.Count > 0
        }

        float randomNumber = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        for (int i = 0; i < validItems.Count; i++)
        {
            cumulativeWeight += validItems[i].weight;
            if (randomNumber <= cumulativeWeight)
            {
                // Kita menemukan item yang dipilih dari validItems,
                // sekarang kita perlu menemukan indeks aslinya di weightedList (daftar input awal)
                return weightedList.IndexOf(validItems[i]);
            }
        }

        // Fallback, seharusnya tidak mudah tercapai jika logika benar dan bobot positif.
        // Mungkin terjadi jika ada masalah presisi float.
        Debug.LogWarning("GetRandomWeightedIndex: Fallback, memilih item valid terakhir.");
         if (validItems.Count > 0)
            return weightedList.IndexOf(validItems[validItems.Count - 1]);
        
        return -1; // Jika tidak ada item valid sama sekali
    }


    void PickRandomButtons()
    {
        // Pastikan daftar availableBuffs di buttonPrefabs tidak null
        if (buttonPrefabs.availableBuffs == null)
        {
            Debug.LogError("buttonPrefabs.availableBuffs adalah null!");
            menuButtons = new Button[0]; // Inisialisasi menuButtons agar tidak null
            return;
        }

        // Kita butuh 3 tombol, jadi pastikan ada cukup posisi dan setidaknya 3 buff unik yang tersedia DENGAN PREFAB VALID
        int numberOfButtonsToPick = 3;
        if (buttonPositions.Length < numberOfButtonsToPick)
        {
            Debug.LogWarning($"Tidak cukup posisi tombol ({buttonPositions.Length}). Butuh {numberOfButtonsToPick}.");
            menuButtons = new Button[0];
            return;
        }

        // Hitung jumlah buff yang benar-benar bisa dipilih (prefab tidak null dan bobot > 0)
        List<WeightedBuffPrefab> selectableBuffs = buttonPrefabs.availableBuffs
                                                    .Where(wb => wb.prefab != null && wb.weight > 0)
                                                    .ToList();

        if (selectableBuffs.Count < numberOfButtonsToPick)
        {
            Debug.LogWarning($"Tidak cukup buff unik yang bisa dipilih ({selectableBuffs.Count}) untuk mengisi {numberOfButtonsToPick} slot. Cek prefab dan bobotnya.");
            // Opsional: coba isi sebanyak yang ada
            // numberOfButtonsToPick = selectableBuffs.Count;
            // Jika ingin gagal total jika kurang dari 3:
            menuButtons = new Button[new Button[numberOfButtonsToPick].Length]; // Inisialisasi dengan slot null jika gagal
            return;
        }

        // Inisialisasi array menuButtons
        menuButtons = new Button[numberOfButtonsToPick];
        List<int> chosenOriginalIndexes = new List<int>(); // Menyimpan indeks dari buttonPrefabs.availableBuffs yang sudah dipilih

        // Buat salinan daftar yang tersedia untuk dimodifikasi (jika perlu, tapi dengan chosenOriginalIndexes mungkin tidak)
        // List<WeightedBuffPrefab> tempListForPicking = new List<WeightedBuffPrefab>(buttonPrefabs.availableBuffs);

        for (int i = 0; i < numberOfButtonsToPick; i++)
        {
            // Jika jumlah buff yang tersisa (setelah memperhitungkan yang sudah dipilih) kurang dari yang kita butuhkan
             if (buttonPrefabs.availableBuffs.Count - chosenOriginalIndexes.Count < 1) {
                 Debug.LogWarning($"Tidak cukup item unik tersisa untuk dipilih untuk slot ke-{i}.");
                 menuButtons[i] = null; // Biarkan slot ini kosong
                 continue; // Lanjut ke slot berikutnya, mungkin bisa diisi jika ada buff lain (tidak relevan jika kita selalu butuh 3 unik)
             }

            int randomIndexInOriginalList;
            WeightedBuffPrefab selectedWeightedBuff;
            int attempts = 0; // Untuk mencegah infinite loop jika ada masalah logika
            const int maxAttempts = 50; // Batas percobaan

            do
            {
                // Panggil fungsi GetRandomWeightedIndex pada daftar yang *ASLI* dan *LENGKAP* (availableBuffs)
                // karena GetRandomWeightedIndex sendiri sudah menangani filtering item yang valid.
                randomIndexInOriginalList = GetRandomWeightedIndex(buttonPrefabs.availableBuffs);

                if (randomIndexInOriginalList == -1) { // Jika tidak ada item yang bisa dipilih (misal semua bobot 0 atau daftar kosong)
                    Debug.LogError("PickRandomButtons: GetRandomWeightedIndex mengembalikan -1. Tidak ada buff yang bisa dipilih.");
                    menuButtons[i] = null; // Tidak ada tombol untuk slot ini
                    // Sebaiknya break atau return dari fungsi PickRandomButtons jika ini terjadi dan merupakan error kritis
                    goto NextSlot; // Lompat ke iterasi berikutnya dari loop for
                }
                attempts++;
                if (attempts > maxAttempts) {
                    Debug.LogError("PickRandomButtons: Melebihi batas percobaan untuk menemukan buff unik. Cek logika bobot atau jumlah buff.");
                    menuButtons[i] = null;
                    goto NextSlot;
                }
            } while (chosenOriginalIndexes.Contains(randomIndexInOriginalList)); // Pastikan item ini belum pernah dipilih untuk slot lain

            chosenOriginalIndexes.Add(randomIndexInOriginalList);
            selectedWeightedBuff = buttonPrefabs.availableBuffs[randomIndexInOriginalList];

            // Ambil GameObject prefab dari WeightedBuffPrefab yang terpilih
            GameObject prefabToInstantiate = selectedWeightedBuff.prefab;
            GameObject buttonInstance = Instantiate(prefabToInstantiate, buttonParent);
            buttonInstance.transform.localPosition = buttonPositions[i]; // Atur posisi sesuai array
            menuButtons[i] = buttonInstance.GetComponent<Button>();

            if (menuButtons[i] == null)
            {
                Debug.LogError($"Prefab '{prefabToInstantiate.name}' yang di-instantiate tidak memiliki komponen Button!");
            }

            NextSlot:; // Label untuk goto jika terjadi error pada pemilihan
        }
    }
}

// Jangan lupa untuk meng-assign GameManager Anda atau pastikan Instance-nya dapat diakses.
// Contoh sederhana GameManager (jika belum ada):
// public class GameManager : MonoBehaviour
// {
// public static GameManager Instance { get; private set; }
// public int cycleTime = 0; // Atau nilai default lain
//
// void Awake()
// {
// if (Instance == null)
// {
// Instance = this;
// DontDestroyOnLoad(gameObject);
// }
// else
// {
// Destroy(gameObject);
// }
// }
// }
