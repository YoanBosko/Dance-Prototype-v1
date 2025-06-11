using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[DefaultExecutionOrder(-5)]
public class DebuffBackgroundMeme : DebuffBase
{
    public string targetTag = "VideoPlayerForMeme";
    public VideoClip videoClipMeme;
    // Start is called before the first frame update
    void Start()
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
        videoClipMeme = beatmapAssigner.videoClipMeme;
        beatmapAssigner.targetVideoPlayer.clip = videoClipMeme;
        // beatmapAssigner.beatmapData.videoClip = videoClipMeme;
    }
}
