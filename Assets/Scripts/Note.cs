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

    [Header("Hold Settings")]

    public bool isShrinking = false;
    public float shrinkSpeed;
    [HideInInspector] public float originalBodyLength;
    private Vector3 originalBodyPosition;


    //new variabel
    private float timePass = 0f;
    private bool isLateHold = false;
    private Vector2 lateHoldBodySize; 
    private Vector3 lateHoldBodyPosition;
    private Vector3 lateHoldTailPosition;
    private double pausedAt = 0.0;
    private double pauseDuration = 0.0;
    private double timeSinceInstantiated;
    private float t;
    private bool isPaused = false;
    private bool isDeleting = false;
    
    void Start()
    {
        timeInstantiated = SongManager.GetAudioSourceTime();

        if (noteType == NoteType.Hold)
        {
            // [TAMBAH] Simpan nilai awal body
            originalBodyLength = body.GetComponent<SpriteRenderer>().size.y;
            originalBodyPosition = body.transform.localPosition;

            float lengthInUnits = SongManager.Instance.noteSpawnDistance * holdDuration / SongManager.Instance.noteTime;
        
            // Atur posisi komponen
            head.SetActive(true);
            body.SetActive(true);
            tail.SetActive(true);

            // Posisi komponen
            body.transform.localPosition = new Vector3(0, lengthInUnits / 2, 0);
            body.GetComponent<SpriteRenderer>().size = new Vector2(
                body.GetComponent<SpriteRenderer>().size.x, 
                lengthInUnits
            );

            tail.transform.localPosition = new Vector3(0, lengthInUnits, 0);

            // Simpan state saat late hold dimulai
            lateHoldBodySize = body.GetComponent<SpriteRenderer>().size;
            lateHoldBodyPosition = body.transform.localPosition;
            lateHoldTailPosition = tail.transform.localPosition;

            // Debug.Log(lateHoldBodyPosition + " " + lateHoldBodySize);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isPaused)
        {
            timeSinceInstantiated = SongManager.GetAudioSourceTime() - timeInstantiated - pauseDuration;
            t = (float)(timeSinceInstantiated / SongManager.Instance.noteTime / 2);
        }
        if (noteType == NoteType.Hit)
        {
            HitNoteControl(t);
        }
        if (noteType == NoteType.Hold)
        {
            HoldNoteControl(t);
        }
    }

    public void HitNoteControl(float t)
    {
        if (t > 1) //destroy object ketika sudah mencapai t(lokasi despawn obj)
        {
            Lane lane = GetComponentInParent<Lane>();
            lane.InputIndex++;
            ScoreManager.Miss();
            Destroy(gameObject);
        }
        else if (noteType != NoteType.Hold) //mengatur pergerakan jatuhnya note selain hold dengan lerp
        {
            transform.localPosition = Vector3.Lerp(Vector3.up * SongManager.Instance.noteSpawnY, Vector3.up * SongManager.Instance.noteDespawnY, t); 
            GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    public void HoldNoteControl(float t)
    {
        Lane lane = GetComponentInParent<Lane>();
        if (t >= 0.5f && lane.isHolding && !isPaused && !isShrinking)
        {
            float t2 = ((float)(timeSinceInstantiated + pauseDuration) - SongManager.Instance.noteTime) / SongManager.Instance.noteTime ;
            float t3 = lateHoldBodySize.y / (SongManager.Instance.noteSpeed * SongManager.Instance.noteTime * 2);
            if (t2 < t3)
            {
            StopCoroutine("HoldDeleteCoroutine");
            TakeSnapshotForLateHold();
            StartCoroutine("HoldJudgmentCoroutine");
            Pause();
            // Debug.Log("terpanggil " + "1");
            isShrinking = true;
            }
            else
            {
                return;
            }
        }
        else if (t >= 0.5f && !lane.isHolding && !isPaused)
        {
            if (t >= 1)
            { 
                timePass = ((float)timeSinceInstantiated - SongManager.Instance.noteTime + (float)pauseDuration) / holdDuration;
                if(!isShrinking && !isDeleting)
                {
                    StartCoroutine("HoldDeleteCoroutine");
                    // Debug.Log("terpanggil " + "2");
                    isDeleting = true;    
                }
            }
            else
            {
                timePass = ((float)timeSinceInstantiated - SongManager.Instance.noteTime) / holdDuration;
                isShrinking = false; 
                // Debug.Log("terpanggil " + "3 " + timePass);
            }
        }
        else if (isPaused && !lane.isHolding)
        {
            StopCoroutine("HoldJudgmentCoroutine");
            Resume();
            isShrinking = false;
            
        }
        transform.localPosition = Vector3.Lerp(Vector3.up * SongManager.Instance.noteSpawnY, Vector3.up * SongManager.Instance.noteDespawnY, t); 
        GetComponent<SpriteRenderer>().enabled = true;
    }

    private void TakeSnapshotForLateHold()
    {

        float bodySizeShrinkAmount = lateHoldBodySize.y * timePass;
        float bodyPositionShrink = lateHoldBodyPosition.y * timePass;
        float tb = 10f / lateHoldBodySize.y;
        float peakPos = lateHoldBodySize.y / (lateHoldBodySize.y / SongManager.Instance.noteSpeed)  + lateHoldBodyPosition.y;
        const float endPos = 10f;
        float tailMovedAmount = lateHoldTailPosition.y - tail.transform.localPosition.y;
        if (timePass <= tb)
        {
            body.transform.localPosition = new Vector3(0, lateHoldBodyPosition.y + bodyPositionShrink - (tailMovedAmount / 2), 0);
            body.GetComponent<SpriteRenderer>().size = new Vector2(
                body.GetComponent<SpriteRenderer>().size.x, 
                lateHoldBodySize.y - bodySizeShrinkAmount - tailMovedAmount
            );
        }
        else
        {
            float factor = (timePass - tb) / (1f - tb);
            body.transform.localPosition = new Vector3(0, peakPos + (endPos - peakPos) * factor, 0);
            body.GetComponent<SpriteRenderer>().size = new Vector2(
                body.GetComponent<SpriteRenderer>().size.x, 
                lateHoldBodySize.y - bodySizeShrinkAmount
            );
        }

        Debug.Log("isinya adalah " + tailMovedAmount);
    }

    public IEnumerator HoldJudgmentCoroutine()
    {
        // isShrinking = true;
        Lane lane = GetComponentInParent<Lane>();
        shrinkSpeed = SongManager.Instance.noteSpeed;
        head.SetActive(false); // Hilangkan head
        
        // Proses penyusutan
        while(body.GetComponent<SpriteRenderer>().size.y > 0 && lane.isHolding)
        {
            // Update ukuran body
            float newSize = body.GetComponent<SpriteRenderer>().size.y - (shrinkSpeed * Time.deltaTime);
            body.GetComponent<SpriteRenderer>().size = new Vector2(
                body.GetComponent<SpriteRenderer>().size.x, 
                Mathf.Max(newSize, 0)
            );
            
            // Update posisi body
            body.transform.localPosition += Vector3.down * (shrinkSpeed * Time.deltaTime * 0.5f);
            tail.transform.localPosition += Vector3.down * (shrinkSpeed * Time.deltaTime);
            // Debug.Log("value is holding: " + lane.isHolding);
            yield return null;
        }
        if (body.GetComponent<SpriteRenderer>().size.y == 0)
        {
            lane.InputIndex++;
            Destroy(gameObject);
        }
    }

    public IEnumerator HoldDeleteCoroutine()
    {
        Lane lane = GetComponentInParent<Lane>();
        shrinkSpeed = SongManager.Instance.noteSpeed;
        while(tail.transform.localPosition.y > 0 && t > 1)
        {
            body.transform.localPosition += Vector3.down * (shrinkSpeed * Time.deltaTime);
            tail.transform.localPosition += Vector3.down * (shrinkSpeed * Time.deltaTime);
            // Debug.Log("terpanggil " + "5");
            yield return null;
        }
        lane.InputIndex++;
        Destroy(gameObject);
    }

    public void Pause()
    {
        if(!isPaused)
        {
            pausedAt = SongManager.GetAudioSourceTime();
            if (t > 1)
            {
                StopCoroutine("HoldDeleteCoroutine");
            }
            isPaused = true;
        }
    }

    public void Resume()
    {
        // Tambahkan durasi pause ke total waktu yang dilewatkan
        if (isPaused)
        {
            pauseDuration += SongManager.GetAudioSourceTime() - pausedAt;
            // tHalfValue = tPassedTime;
            if (t > 1)
            {
                StartCoroutine("HoldDeleteCoroutine");
                
            }
            isPaused = false;
        }
    }
}
