using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class OnAudioEnd : MonoBehaviour
{
    private AudioSource audioSource;
    public UnityEvent onAudioEnd;

    private bool hasTriggered = false;
    private const float endThreshold = 0.1f; // toleransi error sedikit

    void Start()
    {
        audioSource = SongManager.Instance.audioSource;
    }
    void Update()
    {
        if (audioSource == null || audioSource.clip == null)
            return;

        // Cek apakah waktu hampir mencapai durasi akhir
        if (!hasTriggered && audioSource.time >= audioSource.clip.length - endThreshold)
        {
            hasTriggered = true;
            onAudioEnd?.Invoke();
        }
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
    }

    public void ChangeScene(string aValue)
    {
        SceneManager.LoadScene(aValue);
    }
}
