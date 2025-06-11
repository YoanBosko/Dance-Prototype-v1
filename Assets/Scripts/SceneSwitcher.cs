using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcherWithSFX : MonoBehaviour
{
    [Header("Scene Settings")]
    public float delayBeforeSwitch = 1.5f;
    public string nextSceneName = "Menu Lagu";

    [Header("Dependencies")]
    [Tooltip("Assign komponen UINameAnimator untuk animasi nama pemain.")]
    public UINameAnimator nameAnimator; // Referensi baru
    [Tooltip("Assign komponen PostProcessTransitionController.")]
    public PostProcessTransitionController ppTransitionController;

    [Header("SFX Settings")]
    public AudioSource audioSource;
    public AudioClip enterSFX;
    public float sfxStartTime = 0f;
    public float sfxEndTime = 2f;

    private bool hasPressedKey = false;
    private bool sceneSwitchInvoked = false;

    void Update()
    {
        if (!hasPressedKey &&
            (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.F) ||
             Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.K)))
        {
            hasPressedKey = true; // Cegah input berulang

            if (nameAnimator != null)
            {
                // Contoh mendapatkan nama pemain. Ganti dengan logika Anda.
                // Misalnya, jika LeaderboardManager adalah Singleton:
                string playerName = "Player 001"; 
                if (LeaderboardManager.Instance != null) {
                    playerName = "Player " + LeaderboardManager.Instance.GetNextPlayerName(); // Asumsi fungsi ini ada
                }

                // Mulai animasi nama, dan teruskan fungsi StartSceneTransitionSequence sebagai callback
                nameAnimator.PlayNameAnimation(playerName, StartSceneTransitionSequence);
            }
            else
            {
                // Jika tidak ada animator, langsung jalankan sequence transisi
                StartSceneTransitionSequence();
            }
        }
    }
    
    /// <summary>
    /// Fungsi ini berisi semua logika transisi dan akan dipanggil SETELAH animasi nama selesai.
    /// </summary>
    private void StartSceneTransitionSequence()
    {
        // Reset skor kumulatif (logika Anda yang sudah ada, sesuaikan jika perlu)
        // Debug.Log("Score kumulatif direset.");

        // Mulai Post Processing Transition
        if (ppTransitionController != null)
        {
            ppTransitionController.StartTransition(delayBeforeSwitch, null);
        }
        else
        {
            Debug.LogWarning("PostProcessTransitionController belum di-assign di SceneSwitcherWithSFX.", this);
        }

        // Mainkan SFX
        PlaySFX();

        // Jadwalkan perpindahan scene
        if (!sceneSwitchInvoked)
        {
            Invoke(nameof(SwitchScene), delayBeforeSwitch);
            sceneSwitchInvoked = true;
        }
    }
    
    private void PlaySFX()
    {
        if (enterSFX != null && audioSource != null)
        {
            audioSource.clip = enterSFX;
            audioSource.time = sfxStartTime;
            audioSource.Play();
            
            float sfxPlayDuration = Mathf.Max(0, sfxEndTime - sfxStartTime);
            if (sfxPlayDuration > 0)
            {
                Invoke(nameof(StopSFX), sfxPlayDuration);
            }
        }
    }

    private void SwitchScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    private void StopSFX()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
