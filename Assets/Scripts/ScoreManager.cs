using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[DefaultExecutionOrder(-10)]
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
    public Slider slider2;
    public Image fillImage; // drag Image dari Fill Rect ke sini
    public Image fillImage2;
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

    [Header("SoundFX Arrow")]
    public AudioSource switchButtonDK_SFX;
    public AudioSource switchButtonFJ_SFX;

    [Header("Controlled by Buff")]
    public static float scoreMultiplier; // Default multiplier
    public static float scorePerfectMultiplier; // Default multiplier
    public static float healMultiplier; // Default multiplier
    public static float harmMultiplier; // Default multiplier
    public static bool isUndead;
    public static bool isRegen1;
    public static bool isRegen2;
    public static bool oneMoreChance;
    public static bool noBadBreakCombo;

    [Header("Controlled by Debuff")]
    public static bool isInstaDeath;
    public static bool isResultHide;
    public static bool isFlashbang;

    static int perfectHits = 0;
    static int goodHits = 0;
    static int badHits = 0;
    static int missHits = 0;

    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // <--- ini penting
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

        //Controlled by Buff
        scoreMultiplier = 1;
        scorePerfectMultiplier = 1;
        healMultiplier = 1;
        harmMultiplier = 1;
        isUndead = false;
        isRegen1 = false;
        isRegen2 = false;
        oneMoreChance = false;
        noBadBreakCombo = false;

        //Controlled by Debuff
        isInstaDeath = false;
        isResultHide = false;
        isFlashbang = false;
        

        if (!isResultHide)
        {
            resultPerfectPrefab.SetActive(true);
            resultGoodPrefab.SetActive(true);
            resultBadPrefab.SetActive(true);
            resultMissPrefab.SetActive(true);
        }
        else
        {
            resultPerfectPrefab.SetActive(false);
            resultGoodPrefab.SetActive(false);
            resultBadPrefab.SetActive(false);
            resultMissPrefab.SetActive(false);
        }
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
        healthBar += (int)(10f * healMultiplier);

        totalScore += Mathf.RoundToInt(100 * scorePerfectMultiplier); // 100 poin untuk Perfect
        UpdateMultiplier();
        Instance.hitSFX.Play();
    }

    public static void Good()
    {
        Instance.ShowResult(Instance.resultGoodPrefab);    // di fungsi Good
        comboScore += 1;
        goodHits++;
        totalBeats++;
        healthBar += (int)(5f * healMultiplier);

        totalScore += Mathf.RoundToInt(75 * scoreMultiplier); // 75 poin untuk Good
        UpdateMultiplier();
        Instance.hitSFX.Play();
    }

    public static void Bad()
    {
        Instance.ShowResult(Instance.resultBadPrefab);    // di fungsi Bad
        if (!noBadBreakCombo)
        {
            comboScore = 0;  // Reset combo saat kena Bad
        }
        else
        {
            comboScore += 1;
        }
        badHits++;
        totalBeats++;
        healthBar -= (int)(15 * harmMultiplier);

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
        if (!isInstaDeath) healthBar -= (int)(70 * harmMultiplier);
        else healthBar = 0;

        if (isFlashbang)
        {
            GameObject canvasFlashbang = GameObject.FindGameObjectWithTag("Flashbang");
            canvasFlashbang.GetComponent<FadeController>().StartFadeIn();
        }

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
        // Play sound when pressing D or K
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.K))
        {
            if (switchButtonDK_SFX != null && !switchButtonDK_SFX.isPlaying)
                switchButtonDK_SFX.Stop(); // Hentikan kalau sedang bermain
                switchButtonDK_SFX.Play();
        }

        // Play sound when pressing F or J
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.J))
        {
            if (switchButtonFJ_SFX != null && !switchButtonFJ_SFX.isPlaying)
                switchButtonFJ_SFX.Stop();
                switchButtonFJ_SFX.Play();
        }

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
        else if (comboScore > 0 && comboScore % 100 == 0 && isRegen2)
        {
            healthBar += (int)(slider.maxValue / 2);
            healthBar += (int)(slider2.maxValue / 2);
        }
        else if (comboScore > 0 && comboScore % 200 == 0 && isRegen1)
        {
            healthBar += (int)(slider.maxValue / 4);
            healthBar += (int)(slider2.maxValue / 4);
        }
        
        if (healthBar < 80 && oneMoreChance)
        {
            healthBar = (int)slider.maxValue;
            healthBar = (int)slider2.maxValue;
            oneMoreChance = false;
        }

        accuracyText.text = "" + GetAccuracy().ToString("F2") + "%"; // Update UI Akurasi
        scoreText.text = totalScore.ToString(); // Menampilkan total skor

        healthBar = Mathf.Clamp(healthBar, 0, 1000);
        slider.value = healthBar;
        slider2.value = healthBar;
        UpdateFillColor();
        PostProcessingController.Instance.UpdateLowHealthEffect(healthBar);
        if (slider.value == 0 && !isUndead)
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
            fillImage2.color = Color.white;
        }
        else if (healthBar >= 250)
        {
            // 250 - 500: transisi putih ke kuning
            float t = (healthBar - 250f) / 250f; // hasil 0–1
            fillImage.color = Color.Lerp(Color.yellow, Color.white, t); // dari kuning ke putih
            fillImage2.color = Color.Lerp(Color.yellow, Color.white, t);
        }
        else
        {
            // 0 - 250: transisi kuning ke merah
            float t = healthBar / 250f; // hasil 0–1
            fillImage.color = Color.Lerp(Color.red, Color.yellow, t); // dari merah ke kuning
            fillImage2.color = Color.Lerp(Color.red, Color.yellow, t);
        }
    }


}
