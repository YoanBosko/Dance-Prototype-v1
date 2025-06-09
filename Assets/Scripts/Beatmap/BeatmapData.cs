using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "New Beatmap Data", menuName = "Beatmap/Beatmap Data")]
public class BeatmapData : ScriptableObject
{
    [Header("Image Asset")]
    public Sprite imageForAlbum;
    public Sprite imageForBG;

    [Header("Audio Asset")]
    public AudioClip audioClipForMenu;
    public AudioClip audioClipForGameplay;

    [Header("Video Asset")]
    public VideoClip videoClip;

    [Header("Text Asset")]
    public string songTitle;
    public string songDifficulty;
    public string songCredit;

    [Header("MIDI File (as Text or Binary)")]
    public string midiFileHit;
    public string midiFileHold;
    //public byte[] GetMidiBytes()
    //{
    //    return midiFile != null ? midiFile.bytes : null;
    //}
}
