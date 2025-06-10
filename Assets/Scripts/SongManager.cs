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
    public float noteSpeed;
    public float noteSpawnDistance = 10f;
    public float noteSpawnY = 7f;
    public float noteTapY = -3f;

    [Header("MIDI Files")]
    public string hitNoteMidiFile;
    public string holdNoteMidiFile;
    public string bombNoteMidiFile;

    public float noteTime => noteSpawnDistance / noteSpeed;

    // ----------------------------------------------------------------
    public float noteDespawnY => noteTapY - (noteSpawnY - noteTapY);
    // public float noteDespawnY => noteTapY;

    // return noteTapY - (noteSpawnY - noteTapY);
    //-12 -> distance note untuk destroy
    // ----------------------------------------------------------------

    public static MidiFile hitNoteMidi;
    public static MidiFile holdNoteMidi;
    public static MidiFile bombNoteMidi;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        //Controlled by Buff
        marginOfErrorPerfect = 0.05f;
        marginOfErrorGood = 0.2f;
        marginOfErrorBad = 0.4f;
        noteSpeed = 7f;

        // pakai ini untuk test beatmap dalam bentuk aplikasi.exe
        // if (!string.IsNullOrEmpty(SongDataBridge.songPath))
        // {
        //     StartCoroutine(LoadAudioAndMidi());
        // }
        // else
        // {
        //     ReadFromFile();
        // }

        // pakai ini untuk pengembangan project dalam unity
        if (Application.streamingAssetsPath.StartsWith("http://") || Application.streamingAssetsPath.StartsWith("https://"))
        {
            // StartCoroutine(ReadFromWebsite());
            Debug.Log("dari web");
        }
        else
        {
            ReadFromFile();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log((double)Instance.audioSource.timeSamples / Instance.audioSource.clip.frequency);
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
        // pakai ini untuk test beatmap dalam bentuk aplikasi.exe -- cara kerja: mengambil file dari skrip songdatabride
        // hitNoteMidi = MidiFile.Read(hitNoteMidiFile); 
        // holdNoteMidi = MidiFile.Read(holdNoteMidiFile);
        // bombNoteMidi = MidiFile.Read(bombNoteMidiFile);

        // pakai ini untuk pengembangan project dalam unity
        hitNoteMidi = MidiFile.Read(Application.streamingAssetsPath + "/" + hitNoteMidiFile);
        holdNoteMidi = MidiFile.Read(Application.streamingAssetsPath + "/" + holdNoteMidiFile);
        bombNoteMidi = MidiFile.Read(Application.streamingAssetsPath + "/" + bombNoteMidiFile);

        GetDataFromMidi();
        Invoke(nameof(StartSong), songDelayInSeconds);
    }

    public void GetDataFromMidi()
    {
        var hitNotes = hitNoteMidi.GetNotes();
        var holdNotes = holdNoteMidi.GetNotes();
        var bombNotes = bombNoteMidi.GetNotes();

        // pakai ini untuk pengembangan project dalam unity, untuk test beatmap dalam bentuk aplikasi bisa dijadiin comment saja
        var arrayHit = new Melanchall.DryWetMidi.Interaction.Note[hitNotes.Count];
        hitNotes.CopyTo(arrayHit, 0);
        var arrayHold = new Melanchall.DryWetMidi.Interaction.Note[holdNotes.Count];
        holdNotes.CopyTo(arrayHold, 0);
        var arrayBomb = new Melanchall.DryWetMidi.Interaction.Note[bombNotes.Count];
        bombNotes.CopyTo(arrayBomb, 0);

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
        // return (double)Instance.audioSource.timeSamples / Instance.audioSource.clip.frequency;
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
