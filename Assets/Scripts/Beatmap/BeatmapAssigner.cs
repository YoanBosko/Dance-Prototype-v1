using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video; // Tambahkan ini

public class BeatmapAssigner : MonoBehaviour
{
    [Header("Media Asset to Assign")]
    public BeatmapData beatmapData;

    [Header("Target Components")]
    public SpriteRenderer targetSpriteRenderer;   // Untuk objek 2D biasa
    public Image targetUIImage;                   // Untuk UI Image
    public AudioSource targetAudioSource;         // Untuk audio
    public VideoPlayer targetVideoPlayer;         // Untuk video (baru)

    public static BeatmapAssigner Instance;

    void Start()
    {
        AssignBeatmapAsset();
        Instance = this;
    }

    public void AssignBeatmapAsset()
    {
        if (beatmapData == null)
        {
            Debug.LogWarning("BeatmapData belum diassign.");
            return;
        }

        // Assign Sprite ke SpriteRenderer (jika ada)
        if (targetSpriteRenderer != null)
        {
            targetSpriteRenderer.sprite = beatmapData.image;
        }

        // Assign Sprite ke UI Image (jika ada)
        if (targetUIImage != null)
        {
            targetUIImage.sprite = beatmapData.image;
        }

        // Assign AudioClip ke AudioSource (jika ada)
        if (targetAudioSource != null)
        {
            targetAudioSource.clip = beatmapData.audioClip;
        }

        // Assign VideoClip ke VideoPlayer (jika ada)
        if (targetVideoPlayer != null)
        {
            targetVideoPlayer.clip = beatmapData.videoClip;
        }

        Debug.Log("MediaAsset berhasil di-assign ke GameObject.");
    }
}
