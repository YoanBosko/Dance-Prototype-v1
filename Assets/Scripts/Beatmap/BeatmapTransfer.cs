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

        targetAsset.image = sourceAsset.image;
        targetAsset.audioClip = sourceAsset.audioClip;

        Debug.Log("MediaAsset data copied from source to target.");

        BeatmapAssigner.Instance.AssignBeatmapAsset();
    }
    void Start()
    {
        Instance = this;
    }
}
