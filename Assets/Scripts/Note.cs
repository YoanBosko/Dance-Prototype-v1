using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    public enum NoteType { Hit, Hold, Bomb }
    public NoteType noteType = NoteType.Hit;

    double timeInstantiated;
    public float assignedTime;
    public float holdDuration; // Durasi hold note

    [Header("Hold Note Components")]
    public GameObject head;
    public GameObject body;
    public GameObject tail;
    // public float pixelsPerUnit = 100f;

    [Header("Hold Settings")]
    // public Material holdBodyMaterial;
    // private bool isHoldActive;

    public bool isHoldActive = false;
    public bool isBeingJudged = false;
    public bool isShrinking = false;
    public float shrinkSpeed;
    [HideInInspector] public float originalBodyLength;
    private Vector3 originalBodyPosition;
    public bool isMissed = false;
    
    void Start()
    {
        timeInstantiated = SongManager.GetAudioSourceTime();

        if (noteType == NoteType.Hold)
        {
            // SetupHoldVisual();
            
            // [TAMBAH] Simpan nilai awal body
            originalBodyLength = body.GetComponent<SpriteRenderer>().size.y;
            originalBodyPosition = body.transform.localPosition;

            float lengthInUnits = SongManager.Instance.noteSpawnDistance / 2 * holdDuration / SongManager.Instance.noteTime;
        
            // Atur posisi komponen
            head.SetActive(true);
            body.SetActive(true);
            tail.SetActive(true);

            // Posisi komponen
            body.transform.localPosition = new Vector3(0, lengthInUnits/2, 0);
            body.GetComponent<SpriteRenderer>().size = new Vector2(
                body.GetComponent<SpriteRenderer>().size.x, 
                lengthInUnits
            );

            tail.transform.localPosition = new Vector3(0, lengthInUnits, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //general
        if (!isBeingJudged && !isShrinking)
        {
            double timeSinceInstantiated = SongManager.GetAudioSourceTime() - timeInstantiated;
            float t = (float)(timeSinceInstantiated / (SongManager.Instance.noteTime * 2));
            
            if (t > 1 && noteType != NoteType.Hold)
            {
                Destroy(gameObject);
            }
            else if (t > 1 && noteType == NoteType.Hold && !isBeingJudged)
            {
                //jalankan shrinking
                Debug.Log("terpanggil");
                shrinkSpeed = SongManager.Instance.noteSpeed;
                StartCoroutine(HoldJudgmentCoroutine());
            }
            else if (noteType == NoteType.Hold && !isShrinking)
            {
                transform.localPosition = Vector3.Lerp(Vector3.up * SongManager.Instance.noteSpawnY, Vector3.up * SongManager.Instance.noteDespawnY, t); 
                GetComponent<SpriteRenderer>().enabled = true;
            }
            else if (noteType != NoteType.Hold)
            {
                transform.localPosition = Vector3.Lerp(Vector3.up * SongManager.Instance.noteSpawnY, Vector3.up * SongManager.Instance.noteDespawnY, t); 
                GetComponent<SpriteRenderer>().enabled = true;
            }

        // // Rule 8: Cek jika note melewati judgement line tanpa di-hold
        // if (transform.position.y <= SongManager.Instance.noteTapY && !isHoldActive && !isMissed)
        // {
        //     StartCoroutine(ProcessMiss());
        // }
        }
    }

    public IEnumerator HoldJudgmentCoroutine()
    {
            isBeingJudged = true;
            // isShrinking = true;
            head.SetActive(false); // Hilangkan head
            
            // Proses penyusutan
            while(body.GetComponent<SpriteRenderer>().size.y > 0 && isHoldActive || body.GetComponent<SpriteRenderer>().size.y > 0 && (float)(SongManager.GetAudioSourceTime() - timeInstantiated / (SongManager.Instance.noteTime * 2)) > 1)
            {
                // Update ukuran body
                float newSize = body.GetComponent<SpriteRenderer>().size.y - (shrinkSpeed * Time.deltaTime * 0.5f);
                body.GetComponent<SpriteRenderer>().size = new Vector2(
                    body.GetComponent<SpriteRenderer>().size.x, 
                    Mathf.Max(newSize, 0)
                );
                
                // Update posisi body
                body.transform.localPosition += Vector3.down * (shrinkSpeed * Time.deltaTime * 0.25f);
                tail.transform.localPosition += Vector3.down * (shrinkSpeed * Time.deltaTime * 0.5f);
                Debug.Log("total kecepatan: " + shrinkSpeed * Time.fixedDeltaTime);
                
                // Beri skor setiap 0.2 detik
                // if(Mathf.FloorToInt(Time.time * 5) % 5 == 0) // 5 = 1/0.2
                // {
                //     ScoreManager.HoldPerfect();
                // }
                
                yield return null;
            }
    }

    // public IEnumerator ProcessMiss()
    // {
    //     float lengthInUnits = holdDuration * SongManager.Instance.noteSpeed;
    //     isMissed = true;
    //     while(transform.position.y > SongManager.Instance.noteDespawnY - lengthInUnits)
    //     {
    //         // transform.Translate(Vector3.down * SongManager.Instance.noteSpeed * Time.deltaTime);
    //         yield return null;
    //     }
    //     ScoreManager.HoldMiss();
    //     Destroy(gameObject);
    // }
}
