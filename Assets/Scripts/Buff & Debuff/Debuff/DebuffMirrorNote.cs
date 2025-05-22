using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebuffMirrorNote : DebuffBase
{
    public bool horizontal;
    public bool vertical;
    public GameObject prefabHitNoteDL;
    public GameObject prefabHitNoteUL;
    public GameObject prefabHitNoteUR;
    public GameObject prefabHitNoteDR;
    // Start is called before the first frame update
    public override void ActivateDebuff()
    {
        Dictionary<GameObject, Melanchall.DryWetMidi.MusicTheory.NoteName> prefabToNote;
        if (horizontal && !vertical)
        {
            prefabToNote = new Dictionary<GameObject, Melanchall.DryWetMidi.MusicTheory.NoteName>
            {
                { prefabHitNoteDL, Melanchall.DryWetMidi.MusicTheory.NoteName.G },
                { prefabHitNoteUL, Melanchall.DryWetMidi.MusicTheory.NoteName.F },
                { prefabHitNoteUR, Melanchall.DryWetMidi.MusicTheory.NoteName.B },
                { prefabHitNoteDR, Melanchall.DryWetMidi.MusicTheory.NoteName.A }
            };
        }
        else if (!horizontal && vertical)
        {
            prefabToNote = new Dictionary<GameObject, Melanchall.DryWetMidi.MusicTheory.NoteName>
            {
                { prefabHitNoteDL, Melanchall.DryWetMidi.MusicTheory.NoteName.B },
                { prefabHitNoteUL, Melanchall.DryWetMidi.MusicTheory.NoteName.A },
                { prefabHitNoteUR, Melanchall.DryWetMidi.MusicTheory.NoteName.G },
                { prefabHitNoteDR, Melanchall.DryWetMidi.MusicTheory.NoteName.F }
            };
        }
        else if (horizontal && vertical)
        {
            prefabToNote = new Dictionary<GameObject, Melanchall.DryWetMidi.MusicTheory.NoteName>
            {
                { prefabHitNoteDL, Melanchall.DryWetMidi.MusicTheory.NoteName.A },
                { prefabHitNoteUL, Melanchall.DryWetMidi.MusicTheory.NoteName.B },
                { prefabHitNoteUR, Melanchall.DryWetMidi.MusicTheory.NoteName.F },
                { prefabHitNoteDR, Melanchall.DryWetMidi.MusicTheory.NoteName.G }
            };
        }
        else
        {
            prefabToNote = new Dictionary<GameObject, Melanchall.DryWetMidi.MusicTheory.NoteName>
            {
                { prefabHitNoteDL, Melanchall.DryWetMidi.MusicTheory.NoteName.F },
                { prefabHitNoteUL, Melanchall.DryWetMidi.MusicTheory.NoteName.G },
                { prefabHitNoteUR, Melanchall.DryWetMidi.MusicTheory.NoteName.A },
                { prefabHitNoteDR, Melanchall.DryWetMidi.MusicTheory.NoteName.B }
            };
        }

        Lane[] allLanes = FindObjectsOfType<Lane>();

        foreach (Lane lane in allLanes)
        {
            if (prefabToNote.TryGetValue(lane.hitNotePrefab, out Melanchall.DryWetMidi.MusicTheory.NoteName note))
            {
                lane.noteRestriction = note;
            }
        }
    }
    void Start()
    {
        ActivateDebuff();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
