using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;

public class DiagonalScrollSnap : MonoBehaviour
{
    public ScrollRect scrollRect;
    private bool isCanvasActive = false;
    public GameObject menuCanvas;
    public GameObject script1; // Assign dari Inspector
    public KeyCode input1;
    public KeyCode input2;
    public KeyCode input3;
    public KeyCode input4;
    public RectTransform content;
    public RectTransform viewport;
    public List<string> songNames;
    public List<RectTransform> items = new List<RectTransform>();
    // public List<DaftarLagu> items = new List<DaftarLagu>();

    public int visibleItemCount = 5;
    public float snapSpeed = 5f;
    public float highlightScale = 1.2f;
    public float spacing = 170f;

    private int currentIndex = 0;
    private Vector2 targetPosition;
    private float targetPos;

    [Header("Menu Preview")]
    public Image imagePreview;
    public AudioSource audioSourcePreview;
    public Text songTitle;
    public Text songCredit;

    private bool onetime = true;

    void Start()
    {
        // SnapToIndex(currentIndex);
        // LoadAllSong();

        // Panggil setiap item dalam list satu kali di awal scene
        for (int i = 0; i < items.Count; i++)
        {
            currentIndex = (currentIndex + 1) % items.Count;
            SnapToIndex(currentIndex);
            PreviewMenu();  // Misalnya untuk memuat preview atau menyiapkan UI
            audioSourcePreview.Play();
        }

        // Setelah selesai, bisa reset ke index awal
        currentIndex = 7;
        SnapToIndex(currentIndex);
        PreviewMenu();
    }

    void Update()
    {
        HandleInput();
        SnapContent();
        HighlightItem();
        
    }

    // void LoadAllSong()
    // {
    //     foreach (char name in items.gameObject.GetComponent<DataHolder>().beatmapData.audioClip.name)
    //     {
    //         AudioClip clip = Resources.Load<AudioClip>("Songs/" + name);
    //         if (clip != null)
    //         {
    //             items[currentIndex].gameObject.GetComponent<DataHolder>().beatmapData.audioClip = clip;
    //         }
    //         else
    //         {
    //             Debug.Log("kosong");
    //         }
    //     }
    // }
    void HandleInput()
    {
        // Blokir input1 dan input2 jika canvas aktif
        if (!menuCanvas.gameObject.activeSelf)
        {
            if (Input.GetKeyDown(input1))
            {
                currentIndex = (currentIndex + 1) % items.Count;
                SnapToIndex(currentIndex);
                PreviewMenu();
                audioSourcePreview.Play();
            }
            else if (Input.GetKeyDown(input2))
            {
                currentIndex = (currentIndex - 1 + items.Count) % items.Count;
                SnapToIndex(currentIndex);
                PreviewMenu();
                audioSourcePreview.Play();
            }
            else if (Input.GetKeyDown(input3))
            {
                SelectedSong();
            }
        }
        
        if (Input.GetKeyDown(input4))
        {
            menuCanvas.gameObject.SetActive(false);
            isCanvasActive = false;
        }
    }

    public void SelectedSong()
    {

        GameObject obj = items[currentIndex].gameObject;
        DataHolder dataHolder = obj.GetComponent<DataHolder>();

        if (dataHolder != null && dataHolder.beatmapData != null)
        {
            // 🔁 Kirim beatmap yang dipilih
            BeatmapTransfer.Instance.CopyData(dataHolder.beatmapData);

            // Tampilkan canvas preview
            menuCanvas.gameObject.SetActive(true);
            script1.gameObject.SetActive(false);
        }
    }

    void SnapToIndex(int index)
    {
        if (items.Count == 0) return;

        Vector2 itemLocalPos = items[index].anchoredPosition;
        Vector2 viewportCenter = viewport.rect.size / 2f;
        // targetPosition = -itemLocalPos + viewportCenter;
        targetPosition = -itemLocalPos;

        float itemPos = items[index].anchoredPosition.x;
        targetPos = -itemPos + 550f;

        LoopItemPositions();
    }
    void PreviewMenu()
    {
        GameObject obj = items[currentIndex].gameObject;
        DataHolder dataHolder = obj.GetComponent<DataHolder>();
        imagePreview.sprite = dataHolder.beatmapData.image;
        audioSourcePreview.clip = dataHolder.beatmapData.audioClipForMenu;
        songCredit.text = dataHolder.beatmapData.songCredit;
        songTitle.text = dataHolder.beatmapData.songTitle;  
    }

    void SnapContent()
    {
        content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, new Vector2(targetPos, content.anchoredPosition.y), Time.deltaTime * snapSpeed);
    }

    void HighlightItem()
    {
        for (int i = 0; i < items.Count; i++)
        {
            float scale = (i == currentIndex) ? highlightScale : 1f;
            items[i].localScale = Vector3.Lerp(items[i].localScale, Vector3.one * scale, Time.deltaTime * 10f);
        }
    }

    void LoopItemPositions()
    {
        if (items.Count == 0) return;

        // float spacing = items[1].anchoredPosition.x - items[0].anchoredPosition.x;
        int halfCount = items.Count / 2;

        for (int i = 0; i < items.Count; i++)
        {
            int offset = i - currentIndex;

            // Wrap offset agar item terdekat tetap berada dekat visual center
            if (offset >=  halfCount)
                offset -= items.Count;
            else if (offset < -halfCount)
                offset += items.Count;

            float newX = items[currentIndex].anchoredPosition.x + offset * spacing;
            Vector2 newPos = new Vector2(newX, items[i].anchoredPosition.y);
            items[i].anchoredPosition = newPos;
        }
    }

}