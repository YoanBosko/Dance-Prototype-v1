using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Video; // Diperlukan untuk komponen Video

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
    public BeatmapData beatmapData;

    [Header("UI & Animation")]
    public float highlightScale = 1.2f;
    public float transitionSpeed = 10f;

    [Header("Video Settings")]
    [Tooltip("Komponen VideoPlayer untuk memutar video roulette.")]
    public VideoPlayer videoPlayer;
    [Tooltip("Video roulette yang akan diputar sebelum buff muncul.")]
    public VideoClip rouletteVideo;
    [Tooltip("Parent GameObject untuk semua tombol buff. Akan disembunyikan selama video.")]
    public GameObject buffButtonsContainer;

    [Header("Scene Transition")]
    public WinState winStateToDebuff;
    public WinState winStateToMenuLagu;

    private float randomizerModifierX = 0f;
    private Button[] menuButtons;
    private int selectedIndex = 0;
    private bool canNavigate = false; // Flag untuk mengontrol input

    void Start()
    {
        // Validasi
        if (buttonPrefabs == null) { Debug.LogError("PrefabMenuController (buttonPrefabs) belum di-assign!"); return; }
        if (videoPlayer == null || rouletteVideo == null) { Debug.LogError("VideoPlayer atau Roulette Video belum di-assign!"); return; }
        if (buffButtonsContainer == null) { Debug.LogError("Buff Buttons Container belum di-assign!"); return; }

        // Sembunyikan container buff dan nonaktifkan navigasi
        buffButtonsContainer.SetActive(false);
        canNavigate = false;

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

        // Daftarkan event dan putar video roulette
        videoPlayer.loopPointReached += OnRouletteVideoEnd;
        videoPlayer.clip = rouletteVideo;
        videoPlayer.isLooping = false;
        videoPlayer.Play();
    }

    void OnDestroy()
    {
        // Membersihkan event listener untuk mencegah memory leak
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnRouletteVideoEnd;
        }
    }
    
    // Dipanggil saat video roulette selesai
    private void OnRouletteVideoEnd(VideoPlayer source)
    {
        // Hanya jalankan sekali untuk video roulette
        if (source.clip == rouletteVideo)
        {
            // Hapus listener agar tidak terpanggil lagi
            source.loopPointReached -= OnRouletteVideoEnd;

            // Tampilkan container buff
            if(buffButtonsContainer != null) buffButtonsContainer.SetActive(true);

            // Sekarang buat dan tampilkan tombol buff
            PickRandomButtons();

            if (menuButtons != null && menuButtons.Length > 0 && menuButtons[0] != null)
            {
                 HighlightButton();
            }

            // Aktifkan navigasi
            canNavigate = true;
        }
    }


    void Update()
    {
        // Hanya proses input jika navigasi diizinkan
        if (!canNavigate || menuButtons == null || menuButtons.Length == 0) return;

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

    private int GetRandomWeightedIndex(List<WeightedBuffPrefab> weightedList)
    {
        if (weightedList == null || weightedList.Count == 0) return -1;
        
        randomizerModifierX = 0f;

        if (GameManager.Instance != null && beatmapData != null)
        {
            int cycleTime = GameManager.Instance.cycleTime;
            string difficulty = beatmapData.songDifficulty;

            if (cycleTime == 1)
            {
                if (difficulty == "Medium")
                {
                    randomizerModifierX = 5f;
                }
                else if (difficulty == "Hard")
                {
                    randomizerModifierX = 10f;
                }
            }
            else if (cycleTime == 2)
            {
                if (difficulty == "Hard")
                {
                    randomizerModifierX = 10f;
                }
            }
        }

        float commonWeight = 50f;
        float epicWeight = 35f;
        float legendaryWeight = 15f;

        commonWeight -= randomizerModifierX;
        legendaryWeight += randomizerModifierX;

        commonWeight = Mathf.Max(1f, commonWeight);
        legendaryWeight = Mathf.Max(1f, legendaryWeight);

        Debug.Log($"--- Randomizer Check ---\nCycleTime: {GameManager.Instance?.cycleTime}, Difficulty: {beatmapData?.songDifficulty}\nModifier X = {randomizerModifierX}\nFinal Weights -> Common: {commonWeight}, Epic: {epicWeight}, Legendary: {legendaryWeight}");

        var calculatedWeights = weightedList
            .Select(item => {
                float weight = 1f;
                switch (item.rarity)
                {
                    case Rarity.Common:    weight = commonWeight; break;
                    case Rarity.Epic:      weight = epicWeight; break;
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

        return weightedList.IndexOf(calculatedWeights.Last().Item);
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
