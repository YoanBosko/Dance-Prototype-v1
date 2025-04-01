using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using System.Linq;

public class Lane : MonoBehaviour
{
    public KeyCode input;
    public GameObject hitNotePrefab;
    public GameObject holdNotePrefab;
    public GameObject bombNotePrefab;
    public Melanchall.DryWetMidi.MusicTheory.NoteName noteRestriction;
    
    private List<Note> notes = new List<Note>();
    public List<double> hitNoteTimeStamps = new List<double>();
    public List<double> holdNoteTimeStamps = new List<double>();
    public List<double> holdNoteTimeDuration = new List<double>();
    public List<double> bombNoteTimeStamps = new List<double>();
    
    private int hitSpawnIndex = 0;
    private int hitInputIndex = 0;
    private int holdSpawnIndex = 0;
    private int bombSpawnIndex = 0;
    private int bombInputIndex = 0;
    
    public bool isHolding = false;
    private Dictionary<Note, Coroutine> activeHolds = new Dictionary<Note, Coroutine>();

    void Update()
    {
        isHolding = Input.GetKey(input); // Rule 3: Deteksi input hold
        
        SpawnHitNotes();
        SpawnHoldNotes();
        SpawnBombNotes();
        
        HandleHitNotes();
        HandleHoldNotes(); // Semua logika hold dipindahkan ke sini
        HandleBombNotes();
    }

    #region Spawn Methods
    void SpawnHitNotes()
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

    void SpawnHoldNotes()
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

    void SpawnBombNotes()
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
    void HandleHitNotes()
    {
        if (hitInputIndex < hitNoteTimeStamps.Count)
        {
            double timeStamp = hitNoteTimeStamps[hitInputIndex];
            double audioTime = SongManager.GetAudioSourceTime() - (SongManager.Instance.inputDelayInMilliseconds / 1000.0);

            if (Input.GetKeyDown(input) && Note.NoteType.Hit == Note.NoteType.Hold)
            {
                if (Math.Abs(audioTime - timeStamp) < SongManager.Instance.marginOfErrorPerfect)
                {
                    ScoreManager.Perfect();
                    Destroy(notes.Find(x => Math.Abs(x.assignedTime - timeStamp) < 0.001f).gameObject);
                    hitInputIndex++;
                }
                else if (SongManager.Instance.marginOfErrorPerfect < Math.Abs(audioTime - timeStamp) && Math.Abs(audioTime - timeStamp) < SongManager.Instance.marginOfErrorGood)
                {
                    ScoreManager.Good();
                    Destroy(notes[hitInputIndex].gameObject);
                    hitInputIndex++;
                }
                else if (SongManager.Instance.marginOfErrorGood < Math.Abs(audioTime - timeStamp) && Math.Abs(audioTime - timeStamp) < SongManager.Instance.marginOfErrorBad)
                {
                    ScoreManager.Bad();
                    Destroy(notes[hitInputIndex].gameObject);
                    hitInputIndex++;
                }
            }
        }
    }

    void HandleHoldNotes()
    {
        foreach (var note in notes.Where(n => n.noteType == Note.NoteType.Hold).ToList())
        {
            // early hold
            if (isHolding && !note.isShrinking && !note.isMissed)
            {
                //tunggu sampe kena judgement line lalu menyusut
                if (note.transform.position.y <= SongManager.Instance.noteTapY)
                {
                    note.shrinkSpeed = SongManager.Instance.noteSpeed;

                    // float timeSinceMiss = (note.transform.position.y - SongManager.Instance.noteTapY) / SongManager.Instance.noteSpeed;
                    // note.body.GetComponent<SpriteRenderer>().size = new Vector2(
                    //     note.body.GetComponent<SpriteRenderer>().size.x,
                    //     Mathf.Max(note.originalBodyLength - (timeSinceMiss * note.shrinkSpeed), 0)
                    // );

                    activeHolds.Add(note, StartCoroutine(note.HoldJudgmentCoroutine()));
                    note.isShrinking = true;
                }
            }
            // early release
            else if (!isHolding && note.isShrinking && !note.isMissed)
            {
                //
                note.isShrinking = false;
                note.isMissed = true;
                note.isBeingJudged = false;
                StopCoroutine(activeHolds[note]);
            }
            // late hold
            else if (isHolding && !note.isShrinking && note.isMissed)
            {
                //
                note.transform.position = new Vector3(note.transform.position.x, SongManager.Instance.noteTapY, note.transform.position.z);
                note.isShrinking = true;
                note.isMissed = false;
                note.isBeingJudged = false;
                note.shrinkSpeed = SongManager.Instance.noteSpeed;

                float timeSinceMiss = (note.transform.position.y - SongManager.Instance.noteTapY) / SongManager.Instance.noteSpeed;
                note.body.GetComponent<SpriteRenderer>().size = new Vector2(
                    note.body.GetComponent<SpriteRenderer>().size.x,
                    Mathf.Max(note.originalBodyLength - (timeSinceMiss * note.shrinkSpeed), 0)
                );

                activeHolds.Add(note, StartCoroutine(note.HoldJudgmentCoroutine()));
            }
            // melewati judgement line
            else if (!isHolding && !note.isShrinking && !note.isMissed)
            {
                //
                if (note.transform.position.y <= SongManager.Instance.noteTapY - SongManager.Instance.marginOfErrorPerfect)
                {
                    note.isMissed = true;
                }
            }


            // // Rule 9: Early release
            // if (!isHolding && note.isHoldActive && !note.isMissed)
            // {
            //     note.isHoldActive = false;
            //     note.isShrinking = false;
            //     Debug.Log("call");
            //     if (activeHolds.ContainsKey(note))
            //     {
            //         StopCoroutine(activeHolds[note]);
            //         activeHolds.Remove(note);
            //         StartCoroutine(note.ProcessMiss());
            //         Debug.Log("call");
            //     }
            // }

            // // Rule 5 & 11: Mulai penilaian saat menyentuh judgement line dan input sedang di-hold
            // else if (note.transform.position.y <= SongManager.Instance.noteTapY && !note.isBeingJudged)
            // {
            //     if (isHolding) // Tidak perlu GetKeyDown (Rule 11)
            //     {
            //         note.isHoldActive = true;
            //         note.shrinkSpeed = SongManager.Instance.noteSpeed;
            //         activeHolds.Add(note, StartCoroutine(note.HoldJudgmentCoroutine()));
            //     }
            // }

            // // Rule 10: Late activation saat miss
            // else if (note.isMissed && isHolding && !note.isHoldActive)
            // {
            //     LateHoldActivation(note);
            // }
        }
    }

    // void LateHoldActivation(Note note)
    // {
    //     note.isMissed = false;
    //     note.isHoldActive = true;
        
    //     // Hitung catch-up shrinkage (Rule 10)
    //     float timeSinceMiss = (note.transform.position.y - SongManager.Instance.noteTapY) / SongManager.Instance.noteSpeed;
    //     note.body.GetComponent<SpriteRenderer>().size = new Vector2(
    //         note.body.GetComponent<SpriteRenderer>().size.x,
    //         Mathf.Max(note.originalBodyLength - (timeSinceMiss * note.shrinkSpeed), 0)
    //     );
        
    //     // activeHolds.Add(note, StartCoroutine(note.HoldJudgmentCoroutine()));
    // }

    void HandleBombNotes()
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