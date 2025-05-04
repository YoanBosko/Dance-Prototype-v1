using UnityEngine;

[System.Serializable]
public class HoldNoteData {
    public float startTime;       // Waktu mulai note dalam lagu
    public float duration;        // Durasi hold note
    public Vector3 spawnPosition; // Posisi spawn di dunia game
}


public class NoteSpawner : MonoBehaviour {
    public GameObject holdNotePrefab;      // Prefab hold note
    public AudioClip song;                   // Lagu yang digunakan
    public float songStartDelay = 1f;        // Delay sebelum lagu dimulai
    public bool spawnOnTheFly = true;        // Pilihan spawn on‑the‑fly vs spawn all di awal
    public HoldNoteData[] holdNoteDataList;  // Data untuk masing‑masing hold note

    private float songTimer = 0f;
    
    void Start() {
        // Jika memilih untuk spawn semua note di awal, instantiate seluruh note
        if (!spawnOnTheFly) {
            foreach (var noteData in holdNoteDataList) {
                SpawnHoldNote(noteData);
            }
        }
        // Mulai lagu dengan delay (implementasi pemutaran audio disesuaikan)
        Invoke("PlaySong", songStartDelay);
    }
    
    void PlaySong() {
        // Contoh: dapat menggunakan AudioSource untuk memainkan lagu
        GetComponent<AudioSource>().clip = song;
        GetComponent<AudioSource>().Play();
    }
    
    void Update() {
        songTimer += Time.deltaTime;
        // Jika spawn on‑the‑fly, spawn note sesuai waktu lagu
        if (spawnOnTheFly) {
            foreach (var noteData in holdNoteDataList) {
                // Misal: spawn note ketika waktu lagu sudah mendekati startTime
                if (songTimer >= noteData.startTime && noteData.startTime > 0) {
                    SpawnHoldNote(noteData);
                    // Tandai agar tidak spawn lagi, misal dengan set startTime ke -1
                    noteData.startTime = -1;
                }
            }
        }
    }
    
    public void SpawnHoldNote(HoldNoteData noteData) {
         GameObject noteObj = Instantiate(holdNotePrefab, noteData.spawnPosition, Quaternion.identity);
         HoldNote holdNote = noteObj.GetComponent<HoldNote>();
         holdNote.Initialize(noteData);
    }
}
