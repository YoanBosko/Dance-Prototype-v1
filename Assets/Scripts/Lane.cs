using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using System.Linq;
using Melanchall.DryWetMidi.MusicTheory;

public class Lane : MonoBehaviour
{
    public KeyCode input;
    public GameObject hitNotePrefab;
    public GameObject holdNotePrefab;
    public GameObject bombNotePrefab;
    public Melanchall.DryWetMidi.MusicTheory.NoteName noteRestriction;
    
    [SerializeField]private List<Note> notes = new List<Note>();
    public List<double> hitNoteTimeStamps = new List<double>();
    public List<double> holdNoteTimeStamps = new List<double>();
    public List<double> holdNoteTimeDuration = new List<double>();
    public List<double> bombNoteTimeStamps = new List<double>();
    
    private int hitSpawnIndex = 0;
    [HideInInspector]public int InputIndex = 0;
    private int holdSpawnIndex = 0;
    // [HideInInspector]public int holdInputIndex = 0;
    private int bombSpawnIndex = 0;
    private int bombInputIndex = 0;
    
    public bool isHolding = false;
    private Coroutine currentCoroutine = null;
    private bool flip = false;
    private Dictionary<Note, Coroutine> activeHolds = new Dictionary<Note, Coroutine>();

    void Update()
    {
        isHolding = Input.GetKey(input); // Rule 3: Deteksi input hold
        
        SpawnHitNotes();
        SpawnHoldNotes();
        SpawnBombNotes();
        

        if (InputIndex < notes.Count)
        {
            var currentNote = notes[InputIndex];
            switch (currentNote.noteType)
            {
                case Note.NoteType.Hit:
                    HandleHitNotes();
                    break;
                case Note.NoteType.Hold:
                    HandleHoldNotes();
                    break;
                case Note.NoteType.Bomb:
                    HandleBombNotes();
                    break;
            }
        }
    }

    #region Spawn Methods
    private void SpawnHitNotes()
    {
        if (hitSpawnIndex < hitNoteTimeStamps.Count)
        {
            if (SongManager.GetAudioSourceTime() >= hitNoteTimeStamps[hitSpawnIndex] - SongManager.Instance.noteTime)
            {
                var note = Instantiate(hitNotePrefab, transform);
                notes.Add(note.GetComponent<Note>());
                note.GetComponent<Note>().assignedTime = (float)hitNoteTimeStamps[hitSpawnIndex];
                hitSpawnIndex++;
            }
        }
    }

    private void SpawnHoldNotes()
    {
        if (holdSpawnIndex < holdNoteTimeStamps.Count)
        {
            if (SongManager.GetAudioSourceTime() >= holdNoteTimeStamps[holdSpawnIndex] - SongManager.Instance.noteTime)
            {
                var note = Instantiate(holdNotePrefab, transform);
                var noteComponent = note.GetComponent<Note>();
                noteComponent.assignedTime = (float)holdNoteTimeStamps[holdSpawnIndex];
                noteComponent.holdDuration = (float)holdNoteTimeDuration[holdSpawnIndex];
                notes.Add(noteComponent);
                holdSpawnIndex++;
            }
        }
    }

    private void SpawnBombNotes()
    {
        if (bombSpawnIndex < bombNoteTimeStamps.Count)
        {
            if (SongManager.GetAudioSourceTime() >= bombNoteTimeStamps[bombSpawnIndex] - SongManager.Instance.noteTime)
            {
                var note = Instantiate(bombNotePrefab, transform);
                notes.Add(note.GetComponent<Note>());
                note.GetComponent<Note>().assignedTime = (float)bombNoteTimeStamps[bombSpawnIndex];
                bombSpawnIndex++;
            }
        }
    }
    #endregion

    #region Handle Input Methods
    private void HandleHitNotes()
    {
        double timeStamp = notes[InputIndex].assignedTime;
        double audioTime = SongManager.GetAudioSourceTime() - (SongManager.Instance.inputDelayInMilliseconds / 1000.0);

        if (Input.GetKeyDown(input))
        {
            if (Math.Abs(audioTime - timeStamp) < SongManager.Instance.marginOfErrorPerfect)
            {
                ScoreManager.Perfect();
                Destroy(notes.Find(x => Math.Abs(x.assignedTime - timeStamp) < 0.001f).gameObject);
                InputIndex++;
            }
            else if (Math.Abs(audioTime - timeStamp) < SongManager.Instance.marginOfErrorGood)
            {
                ScoreManager.Good();
                Destroy(notes.Find(x => Math.Abs(x.assignedTime - timeStamp) < 0.001f).gameObject);
                InputIndex++;
            }
            else if (Math.Abs(audioTime - timeStamp) < SongManager.Instance.marginOfErrorBad)
            {
                ScoreManager.Bad();
                // Destroy(notes[hitInputIndex].gameObject);
                Destroy(notes.Find(x => Math.Abs(x.assignedTime - timeStamp) < 0.001f).gameObject);
                InputIndex++;
            }
        }
    }

    private void HandleHoldNotes()
    {
        double timeStamp = notes[InputIndex].assignedTime;
        double audioTime = SongManager.GetAudioSourceTime() - (SongManager.Instance.inputDelayInMilliseconds / 1000.0);
        // Debug.Log(timeStamp);
        if ((float)(timeStamp - audioTime) < SongManager.Instance.marginOfErrorPerfect && (float)(audioTime - timeStamp) < notes[InputIndex].holdDuration)
        {
            if (isHolding)
            {
                if (!flip)
                {
                    // Mulai Coroutine A lewat instance
                    if (currentCoroutine != null) StopCoroutine(currentCoroutine);
                    currentCoroutine = StartCoroutine(ScoreManager.Instance.HoldCoroutine());
                    flip = true;
                }
                //kasih score manager input skor
            }
            else if (!isHolding)
            {
                bool oneTimeBool = false;
                if (!oneTimeBool && (float)(timeStamp - audioTime) <= 0f)
                {
                    if (currentCoroutine != null) StopCoroutine(currentCoroutine);
                    currentCoroutine = StartCoroutine(ScoreManager.Instance.ReleaseCoroutine());
                    oneTimeBool = true;
                }
                else if (flip)
                {
                    // Mulai Coroutine B lewat instance
                    if (currentCoroutine != null) StopCoroutine(currentCoroutine);
                    currentCoroutine = StartCoroutine(ScoreManager.Instance.ReleaseCoroutine());
                    flip = false;
                }
                //kasih score manager input miss
            }
        }
        else
        {
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        }   
    }

    private void HandleBombNotes()
    {
        // ... (logic untuk bomb notes)
    }
    #endregion

    #region Set Note Timestamps
    public void SetHitNoteTimeStamps(Melanchall.DryWetMidi.Interaction.Note[] array)
    {
        foreach (var note in array)
        {
            if (note.NoteName == noteRestriction)
            {
                var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, SongManager.hitNoteMidi.GetTempoMap());
                hitNoteTimeStamps.Add((double)metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds + (double)metricTimeSpan.Milliseconds / 1000f);
            }
        }
    }

    public void SetHoldNoteTimeStamps(Melanchall.DryWetMidi.Interaction.Note[] array)
    {
        foreach (var note in array)
        {
            if (note.NoteName == noteRestriction)
            {
                var metricStart = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, SongManager.holdNoteMidi.GetTempoMap());
                var metricEnd = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time + note.Length, SongManager.holdNoteMidi.GetTempoMap());
                
                double startTime = metricStart.Minutes * 60f + metricStart.Seconds + (double)metricStart.Milliseconds / 1000f;
                double duration = (metricEnd.Minutes * 60f + metricEnd.Seconds + (double)metricEnd.Milliseconds / 1000f) - startTime;
                
                holdNoteTimeStamps.Add(startTime);
                holdNoteTimeDuration.Add(duration);
            }
        }
    }

    public void SetBombNoteTimeStamps(Melanchall.DryWetMidi.Interaction.Note[] array)
    {
        foreach (var note in array)
        {
            if (note.NoteName == noteRestriction)
            {
                var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, SongManager.bombNoteMidi.GetTempoMap());
                bombNoteTimeStamps.Add((double)metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds + (double)metricTimeSpan.Milliseconds / 1000f);
            }
        }
    }
    #endregion
}