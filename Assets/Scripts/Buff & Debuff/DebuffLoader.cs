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
    [Tooltip("Video yang akan diputar setelah roulette selesai untuk debuff ini.")]
    public VideoClip resultVideo;
}
