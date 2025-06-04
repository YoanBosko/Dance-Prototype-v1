using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class WinState : MonoBehaviour
{
    public AudioSource audioSource;
    public UnityEvent onAudioEnd;
    public UnityEvent onAudioEndCycleTime;
    public bool useDelay;
    public float delayTime;
    public FadeController fadeController; // Assign via Inspector
    [HideInInspector] public bool enableButtonForWinning = false;

    private const float endThreshold = 0.1f; // toleransi error sedikit

    void Start()
    {
        if (SongManager.Instance != null)
        {
            audioSource = SongManager.Instance.audioSource;
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.scoreData.ResetScore();
        }
    }

    void Update()
    {
        if (audioSource == null || audioSource.clip == null)
            return;

        // Cek apakah waktu hampir mencapai durasi akhir
        if (audioSource.time >= audioSource.clip.length - endThreshold)
        {
            if (useDelay)
            {
                float timer = +Time.deltaTime;
                if (timer >= delayTime)
                {
                    onAudioEnd?.Invoke();
                }
            }
            else if (GameManager.Instance.cycleTime == 3)
            {
                onAudioEndCycleTime?.Invoke();
            }
            else
            {
                onAudioEnd?.Invoke();
            }
        }
        if (enableButtonForWinning)
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                enableButtonForWinning = false;
                SceneManager.LoadScene("BuffPicker");
            }
        }
    }

    public void ChangeScene(string sceneName)
    {
        StartCoroutine(FadeAndChangeScene(sceneName));
    }

    private IEnumerator FadeAndChangeScene(string sceneName)
    {
        if (fadeController != null)
        {
            yield return fadeController.FadeOut();
        }

        SceneManager.LoadScene(sceneName);
    }
}
