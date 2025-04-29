using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Lane : MonoBehaviour
{
    public KeyCode input;
    public GameObject hitNotePrefab;
    public GameObject holdNotePrefab;
    public GameObject bombNotePrefab;
    public Melanchall.DryWetMidi.MusicTheory.NoteName noteRestriction;

    public List<Note> notes = new List<Note>();
    public List<double> hitNoteTimeStamps = new List<double>();
    public List<double> holdNoteTimeStamps = new List<double>();
    public List<double> holdNoteTimeDuration = new List<double>();
    public List<double> bombNoteTimeStamps = new List<double>();

    private int hitSpawnIndex = 0;
    private int holdSpawnIndex = 0;
    private int bombSpawnIndex = 0;

    public bool isHolding = false;
    [HideInInspector]public bool oneTimeBool = false;
    [HideInInspector]public bool preInput = false; // Menandai apakah input dilakukan sebelum area
    public Coroutine currentCoroutine = null;

    void Update()
    {
        isHolding = Input.GetKey(input);

        SpawnHitNotes();
        SpawnHoldNotes();
        SpawnBombNotes();

        HandleHitNotes();
        HandleHoldNotes();
        HandleBombNotes();
    }

    #region Spawn Methods
    private void SpawnHitNotes()
    {
        if (hitSpawnIndex < hitNoteTimeStamps.Count &&
            SongManager.GetAudioSourceTime() >= hitNoteTimeStamps[hitSpawnIndex] - SongManager.Instance.noteTime)
        {
            var noteObj = Instantiate(hitNotePrefab, transform);
            var note = noteObj.GetComponent<Note>();
            note.assignedTime = (float)hitNoteTimeStamps[hitSpawnIndex];
            notes.Add(note);
            hitSpawnIndex++;
        }
    }

    private void SpawnHoldNotes()
    {
        if (holdSpawnIndex < holdNoteTimeStamps.Count &&
            SongManager.GetAudioSourceTime() >= holdNoteTimeStamps[holdSpawnIndex] - SongManager.Instance.noteTime)
        {
            var noteObj = Instantiate(holdNotePrefab, transform);
            var note = noteObj.GetComponent<Note>();
            note.assignedTime = (float)holdNoteTimeStamps[holdSpawnIndex];
            note.holdDuration = (float)holdNoteTimeDuration[holdSpawnIndex];
            notes.Add(note);
            holdSpawnIndex++;
        }
    }

    private void SpawnBombNotes()
    {
        if (bombSpawnIndex < bombNoteTimeStamps.Count &&
            SongManager.GetAudioSourceTime() >= bombNoteTimeStamps[bombSpawnIndex] - SongManager.Instance.noteTime)
        {
            var noteObj = Instantiate(bombNotePrefab, transform);
            var note = noteObj.GetComponent<Note>();
            note.assignedTime = (float)bombNoteTimeStamps[bombSpawnIndex];
            notes.Add(note);
            bombSpawnIndex++;
        }
    }
    #endregion

    #region Handle Input Methods
    private void HandleHitNotes()
    {
        double audioTime = SongManager.GetAudioSourceTime() - (SongManager.Instance.inputDelayInMilliseconds / 1000.0);

        // Auto-miss for Hit notes beyond Bad margin
        var missedHits = notes
            .Where(n => n.noteType == Note.NoteType.Hit &&
                        audioTime - n.assignedTime > SongManager.Instance.marginOfErrorBad)
            .ToList();
        foreach (var miss in missedHits)
        {
            ScoreManager.Miss();
            notes.Remove(miss);
            // Destroy(miss.gameObject);
        }

        // On key down, find closest valid Hit note
        if (Input.GetKeyDown(input))
        {
            Note closest = null;
            double bestDiff = double.MaxValue;

            foreach (var note in notes.Where(n => n.noteType == Note.NoteType.Hit))
            {
                double diff = Math.Abs(audioTime - note.assignedTime);
                if (diff <= SongManager.Instance.marginOfErrorBad && diff < bestDiff)
                {
                    closest = note;
                    bestDiff = diff;
                }
            }

            if (closest != null)
            {
                if (bestDiff < SongManager.Instance.marginOfErrorPerfect)
                    ScoreManager.Perfect();
                else if (bestDiff < SongManager.Instance.marginOfErrorGood)
                    ScoreManager.Good();
                else
                    ScoreManager.Bad();

                notes.Remove(closest);
                Destroy(closest.gameObject);
            }
        }
    }

    private void HandleHoldNotes()
    {
        double audioTime = SongManager.GetAudioSourceTime() - (SongManager.Instance.inputDelayInMilliseconds / 1000.0);
        // Auto-miss for Hold notes that have fully expired
        var expiredHolds = notes
            .Where(n => n.noteType == Note.NoteType.Hold &&
                        audioTime > n.assignedTime + n.holdDuration + SongManager.Instance.marginOfErrorBad)
            .ToList();
        foreach (var miss in expiredHolds)
        {
            // ScoreManager.Miss();
            notes.Remove(miss);
            StopAllCoroutines();
            // Destroy(miss.gameObject);
        }

        // Process the next upcoming Hold note
        var nextHold = notes
            .Where(n => n.noteType == Note.NoteType.Hold)
            .OrderBy(n => n.assignedTime)
            .FirstOrDefault();

        if (Input.GetKeyDown(input))
        {
            preInput = true;
        }
        else if (Input.GetKeyUp(input))
        {
            preInput = false;
        }

        if (nextHold == null) return;

        double start = nextHold.assignedTime;
        double end = start + nextHold.holdDuration;

        // Within interaction window
        if (audioTime >= start - SongManager.Instance.marginOfErrorPerfect &&
            audioTime <= end + SongManager.Instance.marginOfErrorBad)
        {
            if (preInput)
            {
                if (currentCoroutine != null) StopCoroutine(currentCoroutine);
                currentCoroutine = StartCoroutine(ScoreManager.Instance.HoldCoroutine());
                preInput = false;
            }
            // else if (Input.GetKeyDown(input))
            // {
            //     if (currentCoroutine != null) StopCoroutine(currentCoroutine);
            //     currentCoroutine = StartCoroutine(ScoreManager.Instance.HoldCoroutine());
            // }
            else if (Input.GetKeyUp(input))
            {
                if (currentCoroutine != null) StopCoroutine(currentCoroutine);
                currentCoroutine = StartCoroutine(ScoreManager.Instance.ReleaseCoroutine());
                oneTimeBool = true;
                // notes.Remove(nextHold);
                // Destroy(nextHold.gameObject);
            }
            else if (oneTimeBool == false && !Input.GetKey(input) && audioTime > start + SongManager.Instance.marginOfErrorBad)
            {
                if (currentCoroutine != null) StopCoroutine(currentCoroutine);
                currentCoroutine = StartCoroutine(ScoreManager.Instance.ReleaseCoroutine());
                Debug.Log("terpanggil hanya sekali " + oneTimeBool);
                oneTimeBool = true;
            }
        }
    }

    private void HandleBombNotes()
    {
        // (Optional) implement bomb note logic similarly
    }
    #endregion

    #region Set Note Timestamps
    public void SetHitNoteTimeStamps(Melanchall.DryWetMidi.Interaction.Note[] array)
    {
        foreach (var note in array)
        {
            if (note.NoteName == noteRestriction)
            {
                var metric = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, SongManager.hitNoteMidi.GetTempoMap());
                hitNoteTimeStamps.Add(metric.Minutes * 60f + metric.Seconds + metric.Milliseconds / 1000f);
            }
        }
    }

    public void SetHoldNoteTimeStamps(Melanchall.DryWetMidi.Interaction.Note[] array)
    {
        foreach (var note in array)
        {
            if (note.NoteName == noteRestriction)
            {
                var mStart = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, SongManager.holdNoteMidi.GetTempoMap());
                var mEnd = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time + note.Length, SongManager.holdNoteMidi.GetTempoMap());
                double start = mStart.Minutes * 60f + mStart.Seconds + mStart.Milliseconds / 1000f;
                double duration = (mEnd.Minutes * 60f + mEnd.Seconds + mEnd.Milliseconds / 1000f) - start;
                holdNoteTimeStamps.Add(start);
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
                var metric = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, SongManager.bombNoteMidi.GetTempoMap());
                bombNoteTimeStamps.Add(metric.Minutes * 60f + metric.Seconds + metric.Milliseconds / 1000f);
            }
        }
    }
    #endregion
}
