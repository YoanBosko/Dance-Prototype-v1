using UnityEngine;
using UnityEngine.SceneManagement;
// using UnityEngine.Rendering.PostProcessing; // Tidak lagi dibutuhkan secara langsung di sini

public class SceneSwitcherWithSFX : MonoBehaviour
{
    [Header("Scene Settings")]
    public float delayBeforeSwitch = 1.5f; // Total waktu dari input sampai scene berpindah
    public string nextSceneName = "Menu Lagu";

    [Header("Post Process Transition")]
    public PostProcessTransitionController ppTransitionController; // Assign controller Anda di sini
    public float postProcessTransitionDuration = 1.0f; // Durasi spesifik untuk transisi PP ini

    [Header("SFX Settings")]
    public AudioSource audioSource;
    public AudioClip enterSFX;
    public float sfxStartTime = 0f;
    public float sfxEndTime = 2f; // Ini adalah durasi SFX, bukan waktu akhir absolut

    private bool hasPressedKey = false;
    private bool sceneSwitchInvoked = false;

    void Update()
    {
        if (!hasPressedKey &&
            (Input.GetKeyDown(KeyCode.D) ||
             Input.GetKeyDown(KeyCode.F) ||
             Input.GetKeyDown(KeyCode.J) ||
             Input.GetKeyDown(KeyCode.K)))
        {
            hasPressedKey = true;

            // Reset skor kumulatif (jika LeaderboardManager ada dan memiliki struktur ini)
            // Pastikan LeaderboardManager ada dan scoreDataCumulative.score bisa diakses
            // Jika LeaderboardManager adalah Singleton:
            // if (LeaderboardManager.Instance != null && LeaderboardManager.Instance.scoreDataCumulative != null)
            // {
            // LeaderboardManager.Instance.scoreDataCumulative.score = 0;
            // }
            // Jika tidak, Anda perlu cara lain untuk mengakses dan mereset skor.
            // Untuk sekarang, saya akan mengomentarinya karena LeaderboardManager Anda tidak lagi Singleton.
            // Anda perlu menyesuaikan ini dengan cara Anda mengakses data skor kumulatif.
            // Debug.Log("Score kumulatif direset (logika perlu disesuaikan).");


            // Mulai Post Processing Transition
            if (ppTransitionController != null)
            {
                // ppTransitionController.StartTransition(postProcessTransitionDuration, OnPostProcessComplete);
                // Jika Anda tidak butuh callback spesifik saat PP selesai, bisa null:
                ppTransitionController.StartTransition(postProcessTransitionDuration, null);
            }
            else
            {
                Debug.LogWarning("PostProcessTransitionController belum di-assign di SceneSwitcherWithSFX.", this);
            }

            // Mainkan SFX
            if (enterSFX != null && audioSource != null)
            {
                audioSource.clip = enterSFX;
                audioSource.time = sfxStartTime; // Mulai dari waktu spesifik
                audioSource.Play();
                // Hitung durasi SFX akan dimainkan
                float sfxPlayDuration = Mathf.Max(0, sfxEndTime - sfxStartTime);
                if (sfxPlayDuration > 0)
                {
                    Invoke(nameof(StopSFX), sfxPlayDuration);
                }
                else
                {
                    // Jika sfxEndTime <= sfxStartTime, mungkin SFX tidak perlu di-stop manual
                    // atau ini adalah one-shot yang akan berhenti sendiri.
                    // Jika ingin berhenti segera: StopSFX();
                }
            }

            // Jadwalkan perpindahan scene
            if (!sceneSwitchInvoked)
            {
                Invoke(nameof(SwitchScene), delayBeforeSwitch);
                sceneSwitchInvoked = true;
            }
        }
    }

    // Callback opsional jika Anda perlu melakukan sesuatu tepat setelah transisi PP selesai
    // void OnPostProcessComplete()
    // {
    // Debug.Log("Transisi Post Processing Selesai.");
    // }

    void SwitchScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    void StopSFX()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}