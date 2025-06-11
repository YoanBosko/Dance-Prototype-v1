using System.ComponentModel;
using UnityEngine;

public class BeatmapTransfer : MonoBehaviour
{
    // [Header("Source ScriptableObject")]
    // public BeatmapData sourceAsset;

    [Header("Target ScriptableObject")]
    public BeatmapData targetAsset;
    public static BeatmapTransfer Instance;

    [ContextMenu("Copy MediaAsset Data")]
    public void CopyData(BeatmapData sourceAsset)
    {
        if (sourceAsset == null || targetAsset == null)
        {
            Debug.LogWarning("Source or Target MediaAsset is missing.");
            return;
        }

        targetAsset.imageForAlbum = sourceAsset.imageForAlbum;
        targetAsset.imageForBG = sourceAsset.imageForBG;
        targetAsset.audioClipForGameplay = sourceAsset.audioClipForGameplay;
        targetAsset.videoClip = sourceAsset.videoClip;
        targetAsset.midiFileHit = sourceAsset.midiFileHit;
        targetAsset.midiFileHold = sourceAsset.midiFileHold;
        targetAsset.songDifficulty = sourceAsset.songDifficulty;
        targetAsset.songTitle = sourceAsset.songTitle;
        targetAsset.audioClipForReverse = sourceAsset.audioClipForReverse;

        Debug.Log("MediaAsset data copied from source to target.");

        BeatmapAssigner.Instance.AssignBeatmapAsset();
    }
    void Start()
    {
        Instance = this;
    }
}
