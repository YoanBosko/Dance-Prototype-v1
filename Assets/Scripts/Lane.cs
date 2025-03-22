using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lane : MonoBehaviour
{
    public Melanchall.DryWetMidi.MusicTheory.NoteName noteRestriction;
    public KeyCode input;
    public GameObject notePrefab;
    List<Note> notes = new List<Note>();
    public List<double> timeStamps = new List<double>();

    int spawnIndex = 0;
    int inputIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void SetTimeStamps(Melanchall.DryWetMidi.Interaction.Note[] array)
    {
        foreach (var note in array)
        {
            if (note.NoteName == noteRestriction)
            {
                var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, SongManager.midiFile.GetTempoMap());
                timeStamps.Add((double)metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds + (double)metricTimeSpan.Milliseconds / 1000f);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (spawnIndex < timeStamps.Count)
        {
            if (SongManager.GetAudioSourceTime() >= timeStamps[spawnIndex] - SongManager.Instance.noteTime)
            {
                var note = Instantiate(notePrefab, transform);
                notes.Add(note.GetComponent<Note>());
                note.GetComponent<Note>().assignedTime = (float)timeStamps[spawnIndex];
                spawnIndex++;
            }
        }

        if (inputIndex < timeStamps.Count)
        {
            double timeStamp = timeStamps[inputIndex];
            double marginOfErrorGood = SongManager.Instance.marginOfErrorGood;
            double marginOfErrorBad = SongManager.Instance.marginOfErrorBad;
            double audioTime = SongManager.GetAudioSourceTime() - (SongManager.Instance.inputDelayInMilliseconds / 1000.0);

            if (Input.GetKeyDown(input))
            {
                if (Math.Abs(audioTime - timeStamp) < marginOfErrorGood)
                {
                    ScoreManager.Perfect();
                    Destroy(notes[inputIndex].gameObject);
                    inputIndex++;
                }
                else if (marginOfErrorGood < Math.Abs(audioTime - timeStamp) && Math.Abs(audioTime - timeStamp) < marginOfErrorBad)
                {
                    ScoreManager.Good();
                    Destroy(notes[inputIndex].gameObject);
                    inputIndex++;
                }
                else
                {
                    ScoreManager.Bad();
                    Destroy(notes[inputIndex].gameObject);
                    inputIndex++;
                }
            }
            if (timeStamp + marginOfErrorBad <= audioTime)
            {
                ScoreManager.Miss();
                inputIndex++;
            }
        }       
    
    }
    private void Perfect()
    {
        ScoreManager.Perfect();
    }
    private void Good()
    {
        ScoreManager.Good();
    }
    private void Bad()
    {
        ScoreManager.Bad();
    }
    private void Miss()
    {
        ScoreManager.Miss();
    }
}
