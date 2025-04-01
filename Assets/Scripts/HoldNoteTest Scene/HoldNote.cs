using UnityEngine;
using UnityEngine.UI;

public class HoldNote : MonoBehaviour {
    public RectTransform bar;           // Referensi ke visual bar (UI)
    public Color perfectColor = Color.green;
    public Color missColor = Color.red;
    public float fullBarWidth = 200f;     // Lebar penuh bar, bisa disesuaikan

    private float startTime;
    private float duration;
    private bool isHolding = false;
    private float holdTimer = 0f;
    private float scoreInterval = 0.2f;
    private float scoreTimer = 0f;
    
    // State machine untuk hold note
    private enum NoteState { Idle, Holding, Released, Completed }
    private NoteState currentState = NoteState.Idle;
    
    // Inisialisasi note dengan data dari NoteSpawner
    public void Initialize(HoldNoteData data) {
        startTime = data.startTime;
        duration = data.duration;
        currentState = NoteState.Idle;
        // Set visual bar ke lebar penuh
        bar.sizeDelta = new Vector2(fullBarWidth, bar.sizeDelta.y);
    }
    
    void Update() {
        // Asumsikan note sudah aktif saat ditampilkan. 
        // Untuk sinkronisasi dengan waktu lagu, bisa ditambahkan pengecekan waktu saat ini.
        
        // Deteksi input, di sini sebagai contoh menggunakan KeyCode.Space
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (currentState == NoteState.Idle) {
                currentState = NoteState.Holding;
                isHolding = true;
                holdTimer = 0f;
                scoreTimer = 0f;
            }
        }
        
        if (Input.GetKeyUp(KeyCode.Space)) {
            if (currentState == NoteState.Holding) {
                currentState = NoteState.Released;
                isHolding = false;
                // Reset timer skor untuk mulai evaluasi "miss"
                scoreTimer = 0f;
            }
        }
        
        // Update logika berdasarkan state
        if (currentState == NoteState.Holding) {
            holdTimer += Time.deltaTime;
            scoreTimer += Time.deltaTime;
            UpdateBar();

            // Evaluasi skor setiap 0,2 detik ketika masih menahan tombol
            if (scoreTimer >= scoreInterval) {
                Debug.Log("Perfect");
                bar.GetComponent<Image>().color = perfectColor;
                // Di sini bisa memanggil ScoreManager untuk menambah skor
                // ScoreManager.instance.AddScore(perfectPoints);
                scoreTimer = 0f;
            }
            
            // Selesaikan hold note jika waktu hold sudah memenuhi durasi
            if (holdTimer >= duration) {
                currentState = NoteState.Completed;
                Debug.Log("Hold note completed");
            }
        }
        else if (currentState == NoteState.Released) {
            // Evaluasi miss setiap 0,2 detik ketika tombol tidak ditekan
            scoreTimer += Time.deltaTime;
            if (scoreTimer >= scoreInterval) {
                Debug.Log("Miss");
                bar.GetComponent<Image>().color = missColor;
                // Misal: ScoreManager.instance.AddScore(missPenalty);
                scoreTimer = 0f;
            }
        }
    }
    
    // Update visual bar sesuai sisa waktu hold
    void UpdateBar() {
        float remainingTime = Mathf.Clamp(duration - holdTimer, 0, duration);
        float newWidth = (remainingTime / duration) * fullBarWidth;
        bar.sizeDelta = new Vector2(newWidth, bar.sizeDelta.y);
    }
}
