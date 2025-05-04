using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "New Beatmap Data", menuName = "Beatmap/Beatmap Data")]
public class BeatmapData : ScriptableObject
{
    [Header("Image Asset")]
    public Sprite image;

    [Header("Audio Asset")]
    public AudioClip audioClip;

    [Header("Video Asset")]
    public VideoClip videoClip;

    [Header("MIDI File (as Text or Binary)")]
    public TextAsset midiFile;
}
