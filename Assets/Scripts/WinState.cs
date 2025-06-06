using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
// using UnityEngine.Rendering.PostProcessing; // Tidak lagi dibutuhkan secara langsung jika PostProcessTransitionController menangani detailnya

public class WinState : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource; // Sebaiknya di-assign manual jika SongManager.Instance tidak selalu ada
    public UnityEvent onAudioEnd;
    public UnityEvent onAudioEndCycleTime;

    [Header("Timing Settings")]
    public bool useDelay;
    public float delayTime;

    [Header("Transition Controllers")]
    public FadeController fadeController; // Assign via Inspector
    public PostProcessTransitionController ppTransitionController; // Assign PostProcessTransitionController Anda di sini
    public float postProcessTransitionDuration = 1.0f; // Durasi untuk efek post-process khusus di state ini

    [Header("Winning State Logic")]
    [HideInInspector] public bool enableButtonForWinning = false;

    private const float endThreshold = 0.1f; // toleransi error sedikit
    private bool ppTransitionHasFinished = false; // Flag untuk menandakan selesainya transisi post-process
    private float audioEndLogicDelayTimer = 0f; // Timer untuk logika delay internal (perbaikan dari skrip asli)
    private bool audioEndDelayStarted = false; // Flag untuk logika delay internal

    void Start()
    {
        // Mencoba mendapatkan AudioSource dari SongManager jika tidak di-assign manual
        if (audioSource == null && SongManager.Instance != null)
        {
            audioSource = SongManager.Instance.audioSource;
        }
        else if (audioSource == null)
        {
            Debug.LogWarning("AudioSource tidak di-assign di WinState dan SongManager.Instance tidak ditemukan.", this);
        }

        // Reset skor (pastikan ScoreManager.Instance ada)
        if (ScoreManager.Instance != null && ScoreManager.Instance.scoreData != null)
        {
            ScoreManager.Instance.scoreData.ResetScore();
        }
        else
        {
            Debug.LogWarning("ScoreManager.Instance atau ScoreManager.Instance.scoreData tidak ditemukan untuk reset skor.", this);
        }
    }

    void Update()
    {
        if (audioSource == null || audioSource.clip == null || !audioSource.isPlaying) // Tambahkan !audioSource.isPlaying untuk mencegah error jika audio berhenti lebih awal
            return;

        // Cek apakah waktu audio hampir mencapai durasi akhir
        // dan pastikan script ini masih aktif (belum memulai proses pindah scene)
        if (enabled && audioSource.time >= audioSource.clip.length - endThreshold)
        {
            if (useDelay)
            {
                if (!audioEndDelayStarted) // Mulai timer jika belum
                {
                    audioEndLogicDelayTimer = 0f;
                    audioEndDelayStarted = true;
                }
                audioEndLogicDelayTimer += Time.deltaTime;

                if (audioEndLogicDelayTimer >= delayTime)
                {
                    onAudioEnd?.Invoke();
                    enabled = false; // Nonaktifkan update setelah invoke untuk mencegah pemanggilan berulang
                }
            }
            else if (GameManager.Instance != null && GameManager.Instance.cycleTime == 3)
            {
                onAudioEndCycleTime?.Invoke();
                enabled = false; // Nonaktifkan update
            }
            else
            {
                onAudioEnd?.Invoke();
                enabled = false; // Nonaktifkan update
            }
        }

        // Logika tombol untuk menang (jika diperlukan)
        if (enableButtonForWinning)
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                enableButtonForWinning = false;
                // Sebaiknya panggil ChangeScene agar transisi PP juga berlaku di sini jika diinginkan
                ChangeScene("BuffPicker");
                // SceneManager.LoadScene("BuffPicker"); // Cara lama
            }
        }
    }

    /// <summary>
    /// Fungsi publik untuk memulai proses perpindahan scene dengan transisi.
    /// Ini bisa dipanggil oleh UnityEvents (onAudioEnd, onAudioEndCycleTime).
    /// </summary>
    public void ChangeScene(string sceneName)
    {
        // Hanya mulai coroutine jika belum ada yang berjalan dan script masih aktif
        if (this.enabled) // Cek this.enabled untuk pastikan coroutine tidak dimulai jika script sudah dinonaktifkan
        {
            StartCoroutine(TransitionAndChangeSceneSequence(sceneName));
        }
    }

    private IEnumerator TransitionAndChangeSceneSequence(string sceneName)
    {
        // 1. Mulai Transisi Post-Process dan Tunggu Selesai
        if (ppTransitionController != null)
        {
            Debug.Log("Memulai transisi Post-Process...");
            ppTransitionHasFinished = false; // Reset flag
            ppTransitionController.StartTransition(postProcessTransitionDuration, () => {
                ppTransitionHasFinished = true;
                Debug.Log("Callback onComplete Post-Process dipanggil.");
            });

            // Tunggu hingga callback onComplete dari PostProcessTransitionController dijalankan
            // atau timeout untuk mencegah macet.
            float ppWaitTimer = 0f;
            // Tambah sedikit buffer ke durasi tunggu untuk memberi waktu callback dieksekusi
            float maxPpWaitTime = postProcessTransitionDuration ;

            while (!ppTransitionHasFinished && ppWaitTimer < maxPpWaitTime)
            {
                ppWaitTimer += Time.deltaTime;
                yield return null;
            }

            if (!ppTransitionHasFinished)
            {
                Debug.LogWarning("Transisi Post-Process tidak menyelesaikan callback onComplete dalam waktu yang diharapkan.");
            }
            else
            {
                Debug.Log("Transisi Post-Process dianggap selesai.");
            }
        }
        else
        {
            Debug.Log("ppTransitionController tidak di-assign, melewati transisi Post-Process.");
        }

        // 2. Mulai Transisi Fade (dari FadeController) dan Tunggu Selesai
        //if (fadeController != null)
        //{
        //    Debug.Log("Memulai FadeOut dari FadeController...");
        // Asumsikan fadeController.FadeOut() adalah coroutine
        //yield return fadeController.FadeOut();
        //Debug.Log("FadeOut dari FadeController selesai.");
        //}
        //else
        //{
        //Debug.Log("fadeController tidak di-assign, melewati FadeOut.");
        //}
        //
        // 3. Pindah Scene
        Debug.Log($"Memuat scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}
