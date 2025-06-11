using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Video;

public class DebuffLoader : MonoBehaviour
{

    public Sprite image;
    public string cardTitleText;
    public string cardDescriptionText;
    
    //videoplayer
    [Tooltip("Video pengungkapan (reveal) yang akan diputar sekali setelah roulette.")]
    public VideoClip revealVideo;

    [Tooltip("Video hasil yang akan di-loop setelah video pengungkapan selesai.")]
    public VideoClip loopingResultVideo;
}
