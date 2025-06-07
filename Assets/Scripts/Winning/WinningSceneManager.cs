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
    //public string nextSceneName = "MainMenu"; // Scene yang akan dimuat saat tombol 'K' ditekan
    public UnityEvent onButtonPress;
    public UnityEvent onButtonPress3rdCycle;

    private bool hasSwitchedToLoop = false;
    private bool isInputEnabled = false;      // Flag baru untuk mengontrol input

    void Start()
    {
        // Validasi awal
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer belum di-assign!", this);
            return;
        }

        if (textParent != null)
        {
            textParent.SetActive(false); // Sembunyikan statistik di awal
        }

        // Memulai video pertama
        videoPlayer.clip = victory1;
        videoPlayer.isLooping = false;
        videoPlayer.Play();

        // Mendaftarkan event listener untuk saat video selesai
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    // Dipanggil setiap frame
    void Update()
    {
        // Hanya cek input jika isInputEnabled adalah true
        if (isInputEnabled && Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Tombol ditekan. Memuat scene berikutnya...");

            // Nonaktifkan input lagi untuk mencegah pemanggilan berulang
            isInputEnabled = false;

            // Pindah ke scene berikutnya
            if (onButtonPress != null && GameManager.Instance.cycleTime != 3)
            {
                //SceneManager.LoadScene(nextSceneName);
                onButtonPress?.Invoke();
            }
            else if (onButtonPress3rdCycle != null && GameManager.Instance.cycleTime == 3)
            {
                onButtonPress3rdCycle?.Invoke();
            }
            else
            {
                Debug.LogWarning("Next Scene belum diatur!", this);
            }
        }
    }

    // Dipanggil saat video mencapai akhir
    void OnVideoEnd(VideoPlayer vp)
    {
        // Cek jika ini adalah akhir dari video pertama (victory1)
        if (!hasSwitchedToLoop && vp.clip == victory1)
        {
            hasSwitchedToLoop = true;

            // Pindah ke video kedua dan buat looping
            videoPlayer.clip = victory2;
            videoPlayer.isLooping = true;
            videoPlayer.Play();

            if (textParent != null)
            {
                textParent.SetActive(true); // Tampilkan statistik teks
            }

            // Aktifkan input tombol 'K' SEKARANG
            isInputEnabled = true;
        }
    }

    // Praktik yang baik untuk membersihkan event listener saat objek dihancurkan
    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }
    }
}
