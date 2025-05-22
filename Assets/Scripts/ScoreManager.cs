using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public AudioSource hitSFX;
    public AudioSource missSFX;
    public TMPro.TextMeshPro comboText;
    public TMPro.TextMeshPro scoreText;
    public TMPro.TextMeshPro resultText;
    public TMPro.TextMeshPro accuracyText;
    public Slider slider;
    public Image fillImage; // drag Image dari Fill Rect ke sini
    public UnityEvent onHealthZero;
    public ScoreData scoreData;

    public GameObject resultPerfectPrefab;
    public GameObject resultGoodPrefab;
    public GameObject resultBadPrefab;
    public GameObject resultMissPrefab;

    public Transform resultSpawnPoint; // posisi di mana result akan dimunculkan
    private GameObject currentResultInstance;
    private LEDController_InGame ledController;
    int lastComboTriggered = 0;
    static int comboScore;
    static string result;
    static int totalScore = 0;
    static int healthBar = 1000;

    static int totalBeats = 0;  // Total beat dalam permainan
    public static int successfulHits;  // Jumlah hit yang masuk kategori Perfect atau Good
    static float scoreMultiplier = 1.0f; // Default multiplier

    static int perfectHits = 0;
    static int goodHits = 0;
    static int badHits = 0;
    static int missHits = 0;

    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // <--- ini penting
        }
        else
        {
            Destroy(gameObject); // Kalau sudah ada instance, hancurkan yang baru
        }
    }
    void Start()
    {
        Instance = this;
        comboScore = 0;
        result = "";
        totalBeats = SongManager.Instance.GetTotalBeats(); // Ambil total beat dari SongManager
        successfulHits = 0;
        healthBar = 1000;

        scoreData.ResetScore();

        ledController = FindObjectOfType<LEDController_InGame>();
    }

    public static void UpdateMultiplier()
    {
        scoreMultiplier = 1.0f + (comboScore / 100.0f * 0.25f);
    }

#region HitScoreManager
    public static void Perfect()
    {
        Instance.ShowResult(Instance.resultPerfectPrefab);
        comboScore += 1;
        perfectHits++;
        totalBeats++;
        healthBar += 10;

        totalScore += Mathf.RoundToInt(100 * scoreMultiplier); // 100 poin untuk Perfect
        UpdateMultiplier();
        Instance.hitSFX.Play();
    }

    public static void Good()
    {
        Instance.ShowResult(Instance.resultGoodPrefab);    // di fungsi Good
        comboScore += 1;
        goodHits++;
        totalBeats++;
        healthBar += 5;

        totalScore += Mathf.RoundToInt(75 * scoreMultiplier); // 75 poin untuk Good
        UpdateMultiplier();
        Instance.hitSFX.Play();
    }

    public static void Bad()
    {
        Instance.ShowResult(Instance.resultBadPrefab);    // di fungsi Bad
        comboScore = 0;  // Reset combo saat kena Bad
        badHits++;
        totalBeats++;
        healthBar -= 15;

        totalScore += Mathf.RoundToInt(50 * scoreMultiplier); // 50 poin untuk Bad
        UpdateMultiplier();
        Instance.hitSFX.Play();
    }

    public static void Miss()
    {
        Instance.ShowResult(Instance.resultMissPrefab);    // di fungsi Miss
        comboScore = 0;  // Reset combo saat Miss
        missHits++;
        totalBeats++;
        healthBar -= 70;

        UpdateMultiplier();
        Instance.missSFX.Play();
    }
#endregion
#region HoldScoreManager

    public IEnumerator HoldCoroutine()
    {
        while (true)
        {
            Perfect();
            yield return new WaitForSeconds(0.2f);
        }
    }

    public IEnumerator ReleaseCoroutine()
    {
        while (true)
        {
            Miss();
            yield return new WaitForSeconds(0.2f);
        }
    }
#endregion
    public static float GetAccuracy()
    {
        // if (totalBeats == 0) return 100f; // Hindari pembagian dengan nola

        // float accuracy = ((perfectHits * 1.0f) + (goodHits * 0.75f) + (badHits * 0.5f) + (missHits * 0.0f)) / totalBeats * 100f;
        // return accuracy;

        if (totalBeats == 0)
        return 100f; // Supaya awalnya langsung tampil 100%
        
        // Update formula untuk include hold notes
        float score = (perfectHits * 1.0f) + 
                     (goodHits * 0.75f) + 
                     (badHits * 0.5f);
        
        float totalPossible = totalBeats;
        return (score / totalPossible) * 100f;
    }

    private void Update()
    {
        resultText.text = result;
        if (comboScore > 0)
        {
            comboText.gameObject.SetActive(true);
            comboText.text = comboScore.ToString();
        }
        else
        {
            comboText.gameObject.SetActive(false);
        }

        if (comboScore % 15 == 0 && comboScore > 0 && comboScore != lastComboTriggered)
        {
            PostProcessingController.Instance.TriggerEffect();

            // Kirim COMBO_10 ke ESP32
            if (ledController != null)
            {
                ledController.TriggerComboEffect(comboScore);
            }

            lastComboTriggered = comboScore;
        }

        accuracyText.text = "" + GetAccuracy().ToString("F2") + "%"; // Update UI Akurasi
        scoreText.text = totalScore.ToString(); // Menampilkan total skor

        healthBar = Mathf.Clamp(healthBar, 0, 1000);
        slider.value = healthBar;
        UpdateFillColor();
        PostProcessingController.Instance.UpdateLowHealthEffect(healthBar);
        if (slider.value == 0)
        {
            onHealthZero?.Invoke();
        }
    }

    public void ScoreDataUpdate()
    {
        scoreData.score = totalScore;
        scoreData.accuracy = GetAccuracy();
        scoreData.perfectHits = perfectHits;
        scoreData.goodHits = goodHits;
        scoreData.badHits = badHits;
        scoreData.missHits = missHits;
    }

    void ShowResult(GameObject resultPrefab)
    {
        // Hancurkan result sebelumnya jika masih ada
        if (currentResultInstance != null)
        {
            Destroy(currentResultInstance);
        }

        // Spawn yang baru
        GameObject obj = Instantiate(resultPrefab, resultSpawnPoint.position, Quaternion.identity);
        currentResultInstance = obj; // Simpan referensi yang aktif sekarang

        Animator anim = obj.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Spawn");
        }

        // Optional: hancurkan otomatis jika tidak ada replace dalam 0.75 detik
        Destroy(obj, 0.5f);
    }

    void UpdateFillColor()
    {
        if (healthBar >= 500)
        {
            // 500 - 1000: putih
            fillImage.color = Color.white;
        }
        else if (healthBar >= 250)
        {
            // 250 - 500: transisi putih ke kuning
            float t = (healthBar - 250f) / 250f; // hasil 0–1
            fillImage.color = Color.Lerp(Color.yellow, Color.white, t); // dari kuning ke putih
        }
        else
        {
            // 0 - 250: transisi kuning ke merah
            float t = healthBar / 250f; // hasil 0–1
            fillImage.color = Color.Lerp(Color.red, Color.yellow, t); // dari merah ke kuning
        }
    }


}
