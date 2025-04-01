using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.IO;
using UnityEngine.Networking;
using System;
using System.Linq;

public class SongManager : MonoBehaviour
{
    public static SongManager Instance;
    public AudioSource audioSource;
    public Lane[] lanes;

    [Header("Timing Settings")]
    public float songDelayInSeconds;
    public float BPM = 120f;
    public double marginOfErrorPerfect; // in seconds
    public double marginOfErrorGood; // in seconds
    public double marginOfErrorBad; // in seconds
    public int inputDelayInMilliseconds;
    
    [Header("Note Movement")]
    public float noteSpeed = 5f;         // Kecepatan note dalam unit/detik
    public float noteSpawnDistance = 8f; // Jarak tempuh note dari spawn ke tap point
    public float noteSpawnY = 6f;        // Posisi Y awal spawn note
    public float noteTapY = -3f;         // Posisi Y saat note harus ditekan

    [Header("MIDI Files")] // public string fileLocation;
    public string hitNoteMidiFile;
    public string holdNoteMidiFile;
    public string bombNoteMidiFile;

    public float noteTime {
        get {
            return noteSpawnDistance / noteSpeed;
            //1,6 -> speed note jatuh
        }
    }
    public float noteDespawnY
    {
        get
        {
            return noteTapY;
            // return noteTapY - (noteSpawnY - noteTapY);
            //-12 -> distance note untuk destroy
        }
    }

    // public static MidiFile midiFile;
    public static MidiFile hitNoteMidi;
    public static MidiFile holdNoteMidi;
    public static MidiFile bombNoteMidi;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
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

    // private IEnumerator ReadFromWebsite()
    // {
    //     using (UnityWebRequest www = UnityWebRequest.Get(Application.streamingAssetsPath + "/" + fileLocation))
    //     {
    //         yield return www.SendWebRequest();

    //         if (www.isNetworkError || www.isHttpError)
    //         {
    //             Debug.LogError(www.error);
    //         }
    //         else
    //         {
    //             byte[] results = www.downloadHandler.data;
    //             using (var stream = new MemoryStream(results))
    //             {
    //                 midiFile = MidiFile.Read(stream);
    //                 GetDataFromMidi();
    //             }
    //         }
    //     }
    // }

    private void ReadFromFile()
    {
        // midiFile = MidiFile.Read(Application.streamingAssetsPath + "/" + fileLocation);
        // GetDataFromMidi();

        hitNoteMidi = MidiFile.Read(Application.streamingAssetsPath + "/" + hitNoteMidiFile);
        holdNoteMidi = MidiFile.Read(Application.streamingAssetsPath + "/" + holdNoteMidiFile);
        bombNoteMidi = MidiFile.Read(Application.streamingAssetsPath + "/" + bombNoteMidiFile);
        
        GetDataFromMidi();
        Invoke(nameof(StartSong), songDelayInSeconds);
    }
    public void GetDataFromMidi()
    {
        // var notes = midiFile.GetNotes();
        // var array = new Melanchall.DryWetMidi.Interaction.Note[notes.Count];
        // notes.CopyTo(array, 0);

        // foreach (var lane in lanes) lane.SetTimeStamps(array);

        // Invoke(nameof(StartSong), songDelayInSeconds);

        var hitNotes = hitNoteMidi.GetNotes();
        var holdNotes = holdNoteMidi.GetNotes();
        var bombNotes = bombNoteMidi.GetNotes();

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
        return (double)Instance.audioSource.timeSamples / Instance.audioSource.clip.frequency;
    }

    void Update()
    {
        
    }

    public int GetTotalBeats()
    {
        int totalBeats = 0;
        foreach (var lane in lanes)
        {
            // totalBeats += lane.timeStamps.Count;
            totalBeats += lane.hitNoteTimeStamps.Count;
            totalBeats += lane.holdNoteTimeStamps.Count;
            totalBeats += lane.bombNoteTimeStamps.Count;
        }
        return totalBeats;
    }
}
