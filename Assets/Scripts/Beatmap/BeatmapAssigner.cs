using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video; // Tambahkan ini

[DefaultExecutionOrder (-10)]
public class BeatmapAssigner : MonoBehaviour
{
    [Header("Source Media Asset")]
    public BeatmapData beatmapData;

    [Header("Target Components For Gameplay")]
    public SpriteRenderer targetSpriteRenderer;   // Untuk objek 2D biasa
    public Image targetUIImage;                   // Untuk UI Image
    public AudioSource targetAudioSource;         // Untuk audio
    public VideoPlayer targetVideoPlayer;         // Untuk video (baru)
    public SongManager songManager;

    [Header("Target Components For Winning")]
    public Text songTitle;
    public Text songDifficulty;

    public static BeatmapAssigner Instance;

    void Start()
    {
        AssignBeatmapAsset();
        Instance = this;
    }

    public void AssignBeatmapAsset()
    {
        #region Gameplay
        if (beatmapData == null)
        {
            Debug.LogWarning("BeatmapData belum diassign.");
            return;
        }

        // Assign Sprite ke SpriteRenderer (jika ada)
        if (targetSpriteRenderer != null)
        {
            targetSpriteRenderer.sprite = beatmapData.imageForAlbum;
            targetSpriteRenderer.sprite = beatmapData.imageForBG;
        }

        // Assign Sprite ke UI Image (jika ada)
        if (targetUIImage != null)
        {
            targetUIImage.sprite = beatmapData.imageForAlbum;
            targetUIImage.sprite = beatmapData.imageForBG;
        }

        // Assign AudioClip ke AudioSource (jika ada)
        if (targetAudioSource != null)
        {
            targetAudioSource.clip = beatmapData.audioClipForGameplay;
            targetAudioSource.Play();
        }

        // Assign VideoClip ke VideoPlayer (jika ada)
        if (targetVideoPlayer != null)
        {
            targetVideoPlayer.clip = beatmapData.videoClip;
        }

        if (songManager != null)
            {
                songManager.hitNoteMidiFile = beatmapData.midiFileHit;
                songManager.holdNoteMidiFile = beatmapData.midiFileHold;
            }
            else
            {
                Debug.LogWarning("SongManager belum di-assign! Harap assign SongManager melalui Inspector di Unity.");
            }
        #endregion
        #region Winning Scene
        if (songTitle != null)
        {
            songTitle.text = beatmapData.songTitle;
        }
        if (songDifficulty != null)
        {
            songDifficulty.text = beatmapData.songDifficulty;
        }

        #endregion
            Debug.Log("MediaAsset berhasil di-assign ke GameObject.");
    }
}
