using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-15)]
public class DebuffReverseAudio : DebuffBase
{
    public string targetTag = "VideoPlayerForMeme";
    // Start is called before the first frame update
    void Awake()
    {
        ActivateDebuff();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public override void ActivateDebuff()
    {
        GameObject laneShifts = GameObject.FindGameObjectWithTag(targetTag);
        BeatmapAssigner beatmapAssigner = laneShifts.GetComponent<BeatmapAssigner>();
        beatmapAssigner.isReversedAudio = true;
    }
}
