using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.Events;

public class WinningSceneManager : MonoBehaviour
{
    [Header("Video Settings")]
    public VideoPlayer videoPlayer;         // Drag VideoPlayer component here
    public VideoClip victory1;              // Clip: Victory_1 (0-4s)
    public VideoClip victory2;              // Clip: Victory_2 (5-25s)

    [Header("UI & Scene Flow")]
    public GameObject textParent;           // Parent GameObject of all text elements
    public WinMenu winMenu;                 // Assign WinMenu script component here
    public UnityEvent onButtonPress;
    public UnityEvent onButtonPress3rdCycle;

    private bool hasSwitchedToLoop = false;
    private bool isInputEnabled = false;

    void Start()
    {
        // Validasi awal
        if (videoPlayer == null) { Debug.LogError("VideoPlayer belum di-assign!", this); return; }
        if (winMenu == null) { Debug.LogError("WinMenu belum di-assign!", this); return; }

        if (textParent != null)
        {
            textParent.SetActive(false); // Sembunyikan statistik di awal
        }

        videoPlayer.clip = victory1;
        videoPlayer.isLooping = false;
        videoPlayer.Play();
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void Update()
    {
        // Hanya cek input jika isInputEnabled adalah true
        if (isInputEnabled && (Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.D)))
        {
            // Cek apakah WinMenu sedang menganimasikan angka
            if (winMenu.IsAnimating)
            {
                // Jika ya, penekanan tombol pertama akan melewati animasi
                winMenu.SkipAllAnimations();
            }
            else
            {
                // Jika animasi sudah selesai, penekanan tombol akan memicu event
                Debug.Log("Animasi selesai. Memproses event perpindahan scene...");
                isInputEnabled = false; // Nonaktifkan input setelah diproses

                if (onButtonPress != null && winMenu.achievedGoodGrade == true && (GameManager.Instance == null || GameManager.Instance.cycleTime != 3) )
                {
                    onButtonPress?.Invoke();
                }
                else if (onButtonPress3rdCycle != null && GameManager.Instance != null && GameManager.Instance.cycleTime == 3 || winMenu.achievedGoodGrade == false)
                {
                    onButtonPress3rdCycle?.Invoke();
                }
                else
                {
                    Debug.LogWarning("Event untuk ditekan tidak diatur atau kondisi GameManager tidak terpenuhi!", this);
                }
            }
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        if (!hasSwitchedToLoop && vp.clip == victory1)
        {
            hasSwitchedToLoop = true;

            videoPlayer.clip = victory2;
            videoPlayer.isLooping = true;
            videoPlayer.Play();

            if (textParent != null)
            {
                textParent.SetActive(true); // Tampilkan statistik teks (ini akan memicu OnEnable di WinMenu)
            }

            isInputEnabled = true; // Aktifkan input
        }
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }
    }
}