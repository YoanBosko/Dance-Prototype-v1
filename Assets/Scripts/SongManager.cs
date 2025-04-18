using System.Collections;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using UnityEngine.Networking;
using System.IO;
using System.Linq;

public class SongManager : MonoBehaviour
{
    public static SongManager Instance;
    public AudioSource audioSource;
    public Lane[] lanes;

    [Header("Timing Settings")]
    public float songDelayInSeconds;
    public float BPM = 120f;
    public double marginOfErrorPerfect;
    public double marginOfErrorGood;
    public double marginOfErrorBad;
    public int inputDelayInMilliseconds;

    [Header("Note Movement")]
    public float noteSpeed = 5f;
    public float noteSpawnDistance = 8f;
    public float noteSpawnY = 6f;
    public float noteTapY = -3f;

    [Header("MIDI Files")]
    public string hitNoteMidiFile;
    public string holdNoteMidiFile;
    public string bombNoteMidiFile;

    public float noteTime => noteSpawnDistance / noteSpeed;
    public float noteDespawnY => noteTapY;

    public static MidiFile hitNoteMidi;
    public static MidiFile holdNoteMidi;
    public static MidiFile bombNoteMidi;

    void Start()
    {
        Instance = this;

        if (!string.IsNullOrEmpty(SongDataBridge.songPath))
        {
            StartCoroutine(LoadAudioAndMidi());
        }
        else
        {
            ReadFromFile();
        }
    }

    private IEnumerator LoadAudioAndMidi()
    {
        string audioPath = "file://" + SongDataBridge.songPath;
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audioPath, AudioType.UNKNOWN);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Audio load error: " + www.error);
            yield break;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
        audioSource.clip = clip;

        hitNoteMidiFile = SongDataBridge.tapMidiPath;
        holdNoteMidiFile = SongDataBridge.holdMidiPath;
        bombNoteMidiFile = SongDataBridge.tapMidiPath;

        ReadFromFile();
    }

    private void ReadFromFile()
    {
        hitNoteMidi = MidiFile.Read(hitNoteMidiFile);
        holdNoteMidi = MidiFile.Read(holdNoteMidiFile);
        bombNoteMidi = MidiFile.Read(bombNoteMidiFile);

        GetDataFromMidi();
        Invoke(nameof(StartSong), songDelayInSeconds);
    }

    public void GetDataFromMidi()
    {
        var hitNotes = hitNoteMidi.GetNotes();
        var holdNotes = holdNoteMidi.GetNotes();
        var bombNotes = bombNoteMidi.GetNotes();

        foreach (var lane in lanes)
        {
            lane.SetHitNoteTimeStamps(hitNotes.ToArray());
            lane.SetHoldNoteTimeStamps(holdNotes.ToArray());
            lane.SetBombNoteTimeStamps(bombNotes.ToArray());
        }
    }

    public void StartSong()
    {
        audioSource.Play();
    }

    public static double GetAudioSourceTime()
    {
        return (double)Instance.audioSource.timeSamples / Instance.audioSource.clip.frequency;
    }

    public int GetTotalBeats()
    {
        int totalBeats = 0;
        foreach (var lane in lanes)
        {
            totalBeats += lane.hitNoteTimeStamps.Count;
            totalBeats += lane.holdNoteTimeStamps.Count;
            totalBeats += lane.bombNoteTimeStamps.Count;
        }
        return totalBeats;
    }
}
