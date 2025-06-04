using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class WinningSceneManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;        // Drag VideoPlayer component here
    public VideoClip victory1;             // Clip: Victory_1 (0-4s)
    public VideoClip victory2;             // Clip: Victory_2 (5-25s)
    public GameObject textParent;          // Parent GameObject of all text elements

    private bool hasSwitchedToLoop = false;

    void Start()
    {
        textParent.SetActive(false); // Hide stats initially

        videoPlayer.clip = victory1;
        videoPlayer.isLooping = false;
        videoPlayer.Play();

        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        if (!hasSwitchedToLoop)
        {
            hasSwitchedToLoop = true;

            videoPlayer.clip = victory2;
            videoPlayer.isLooping = true;
            videoPlayer.Play();

            textParent.SetActive(true); // Show text/statistics
        }
    }
}
