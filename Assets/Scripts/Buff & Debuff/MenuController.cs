using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

// Enum untuk mendefinisikan tingkat kelangkaan buff, telah diperbarui
public enum Rarity
{
    Common,
    Epic,
    Legendary
}

// Kelas WeightedBuffPrefab sekarang menggunakan Rarity enum yang baru
[System.Serializable]
public class WeightedBuffPrefab
{
    public GameObject prefab;
    [Tooltip("Tingkat kelangkaan buff ini. Common lebih sering muncul daripada Legendary.")]
    public Rarity rarity;
}

// Skrip MenuController yang telah dimodifikasi
public class MenuController : MonoBehaviour
{
    [Header("Core References")]
    public PrefabMenuController buttonPrefabs;
    public Transform buttonParent;
    public Vector3[] buttonPositions;
    [Tooltip("Data dari lagu yang dimainkan sebelumnya untuk menentukan modifier.")]
    public BeatmapData beatmapData; // Variabel BeatmapData baru

    [Header("UI & Animation")]
    public float highlightScale = 1.2f;
    public float transitionSpeed = 10f;

    [Header("Scene Transition")]
    public WinState winStateToDebuff;
    public WinState winStateToMenuLagu;

    // Variabel 'X' yang akan dihitung secara dinamis, tidak lagi diatur dari Inspector.
    private float randomizerModifierX = 0f;

    private Button[] menuButtons;
    private int selectedIndex = 0;

    void Start()
    {
        // Validasi
        if (buttonPrefabs == null) { Debug.LogError("PrefabMenuController (buttonPrefabs) belum di-assign!"); return; }
        if (buttonParent == null) { Debug.LogError("Button Parent belum di-assign!"); return; }
        if (buttonPositions == null || buttonPositions.Length < 3) { Debug.LogError("Button Positions tidak cukup atau belum di-assign."); return; }

        // Reset buff jika cycleTime adalah 1
        if (GameManager.Instance != null && GameManager.Instance.cycleTime == 1)
        {
            foreach (WeightedBuffPrefab weightedBuff in buttonPrefabs.removedBuffs)
            {
                if (!buttonPrefabs.availableBuffs.Any(wb => wb.prefab == weightedBuff.prefab))
                {
                    buttonPrefabs.availableBuffs.Add(weightedBuff);
                }
            }
            buttonPrefabs.removedBuffs.Clear();
        }

        PickRandomButtons();

        if (menuButtons != null && menuButtons.Length > 0 && menuButtons[0] != null)
        {
            HighlightButton();
        }
    }

    void Update()
    {
        if (menuButtons == null || menuButtons.Length == 0) return;

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
            if (menuButtons[selectedIndex] == null) return;

            menuButtons[selectedIndex].onClick.Invoke();

            string selectedButtonInstanceName = menuButtons[selectedIndex].gameObject.name.Replace("(Clone)", "").Trim();
            WeightedBuffPrefab selectedWeightedBuff = buttonPrefabs.availableBuffs.Find(wb => wb.prefab != null && wb.prefab.name == selectedButtonInstanceName);

            if (selectedWeightedBuff != null)
            {
                buttonPrefabs.availableBuffs.Remove(selectedWeightedBuff);
                buttonPrefabs.removedBuffs.Add(selectedWeightedBuff);
            }

            // Ganti scene berdasarkan cycleTime
            if (GameManager.Instance != null && GameManager.Instance.cycleTime == 2)
            {
                winStateToDebuff?.ChangeScene("DebuffScene");
            }
            else
            {
                winStateToMenuLagu?.ChangeScene("Menu Lagu");
            }
        }

        UpdateButtonScales();
    }

    void HighlightButton()
    {
        if (menuButtons == null || menuButtons.Length <= selectedIndex || menuButtons[selectedIndex] == null) return;
        EventSystem.current.SetSelectedGameObject(menuButtons[selectedIndex].gameObject);

        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] != null)
            {
                BuffLoader buffLoader = menuButtons[i].GetComponent<BuffLoader>();
                if (buffLoader != null)
                {
                    if (i == selectedIndex) buffLoader.ActivateMoreDescription();
                    else buffLoader.DeactivateMoreDescription();
                }
            }
        }
    }

    void UpdateButtonScales()
    {
        if (menuButtons == null) return;
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] != null)
            {
                Vector3 targetScale = (i == selectedIndex) ? new Vector3(highlightScale, highlightScale, 1f) : Vector3.one;
                menuButtons[i].transform.localScale = Vector3.Lerp(menuButtons[i].transform.localScale, targetScale, Time.deltaTime * transitionSpeed);
            }
        }
    }

    /// <summary>
    /// Memilih indeks acak dari daftar berdasarkan Rarity dan logika kondisional baru.
    /// </summary>
    private int GetRandomWeightedIndex(List<WeightedBuffPrefab> weightedList)
    {
        if (weightedList == null || weightedList.Count == 0) return -1;

        // --- Langkah 1: Kalkulasi Modifier 'X' berdasarkan kondisi ---

        if (GameManager.Instance != null && beatmapData != null)
        {
            int cycleTime = GameManager.Instance.cycleTime;
            string difficulty = beatmapData.songDifficulty; // Asumsi 'songDifficulty' adalah string "Easy", "Medium", "Hard"

            if (cycleTime == 1)
            {
                
            randomizerModifierX = 0f; // Reset X setiap kali fungsi dipanggil
                // Kondisi 1
                if (difficulty == "Medium")
                {
                    randomizerModifierX += 5f;
                }
                // Kondisi 2
                else if (difficulty == "Hard")
                {
                    randomizerModifierX += 10f; // Reset + 10 = 10
                }
            }
            // Kondisi 3
            else if (cycleTime == 2)
            {
                if (difficulty == "Hard")
                {
                    // Di cycle 2, X tidak di-reset, tapi karena ini dipanggil per scene, kita set saja nilainya
                    // Jika X harus persisten antar scene, perlu sistem save/static.
                    // Untuk sekarang, kita anggap kondisinya adalah X = 10
                    randomizerModifierX += 10f;
                }
            }
        }

        // --- Langkah 2: Tentukan Bobot (Weight) Final untuk setiap Rarity ---
        // Bobot dasar
        float commonWeight = 50f;
        float epicWeight = 35f;
        float legendaryWeight = 15f;

        // Terapkan modifier X
        commonWeight -= randomizerModifierX;
        legendaryWeight += randomizerModifierX;

        // Pastikan bobot tidak menjadi negatif
        commonWeight = Mathf.Max(1f, commonWeight); // Minimal bobot adalah 1
        legendaryWeight = Mathf.Max(1f, legendaryWeight);

        // --- Langkah 3: Debug Log untuk Testing ---
        Debug.Log($"--- Randomizer Check ---\nCycleTime: {GameManager.Instance?.cycleTime}, Difficulty: {beatmapData?.songDifficulty}\nModifier X = {randomizerModifierX}\nFinal Weights -> Common: {commonWeight}, Epic: {epicWeight}, Legendary: {legendaryWeight}");

        // --- Langkah 4: Buat daftar dengan bobot yang sudah dihitung ---
        var calculatedWeights = weightedList
            .Select(item => {
                float weight = 1f;
                switch (item.rarity)
                {
                    case Rarity.Common: weight = commonWeight; break;
                    case Rarity.Epic: weight = epicWeight; break;
                    case Rarity.Legendary: weight = legendaryWeight; break;
                }
                return new { Item = item, Weight = weight };
            })
            .Where(x => x.Item.prefab != null && x.Weight > 0)
            .ToList();

        if (calculatedWeights.Count == 0)
        {
            Debug.LogWarning("Tidak ada item yang bisa dipilih setelah kalkulasi bobot.");
            return -1;
        }

        // --- Langkah 5: Pilih Item secara Acak ---
        float totalWeight = calculatedWeights.Sum(x => x.Weight);
        if (totalWeight <= 0) return -1;

        float randomNumber = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        foreach (var weightedItem in calculatedWeights)
        {
            cumulativeWeight += weightedItem.Weight;
            if (randomNumber <= cumulativeWeight)
            {
                return weightedList.IndexOf(weightedItem.Item);
            }
        }

        return weightedList.IndexOf(calculatedWeights.Last().Item); // Fallback
    }

    void PickRandomButtons()
    {
        int numberOfButtonsToPick = 3;
        if (buttonPrefabs.availableBuffs == null || buttonPositions.Length < numberOfButtonsToPick)
        {
            menuButtons = new Button[0];
            return;
        }

        if (buttonPrefabs.availableBuffs.Count < numberOfButtonsToPick)
        {
            Debug.LogWarning($"Tidak cukup buff unik yang bisa dipilih ({buttonPrefabs.availableBuffs.Count}) untuk mengisi {numberOfButtonsToPick} slot.");
            menuButtons = new Button[0];
            return;
        }

        menuButtons = new Button[numberOfButtonsToPick];
        List<int> chosenOriginalIndexes = new List<int>();

        for (int i = 0; i < numberOfButtonsToPick; i++)
        {
            int randomIndexInOriginalList;
            int attempts = 0;
            const int maxAttempts = 50;

            do
            {
                randomIndexInOriginalList = GetRandomWeightedIndex(buttonPrefabs.availableBuffs);
                if (randomIndexInOriginalList == -1)
                {
                    Debug.LogError("Gagal memilih buff, tidak ada item yang bisa dipilih.");
                    return;
                }
                attempts++;
            } while (chosenOriginalIndexes.Contains(randomIndexInOriginalList) && attempts < maxAttempts);

            if (attempts >= maxAttempts)
            {
                Debug.LogError("Gagal menemukan buff unik setelah beberapa kali percobaan.");
                continue;
            }

            chosenOriginalIndexes.Add(randomIndexInOriginalList);
            WeightedBuffPrefab selectedWeightedBuff = buttonPrefabs.availableBuffs[randomIndexInOriginalList];

            GameObject prefabToInstantiate = selectedWeightedBuff.prefab;
            GameObject buttonInstance = Instantiate(prefabToInstantiate, buttonParent);
            buttonInstance.transform.localPosition = buttonPositions[i];
            menuButtons[i] = buttonInstance.GetComponent<Button>();
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
