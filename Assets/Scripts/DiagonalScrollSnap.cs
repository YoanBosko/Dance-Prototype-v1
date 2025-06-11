using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum Difficulty { Easy, Medium, Hard }

[System.Serializable]
public class SongItemData
{
    [Tooltip("Transform dari item UI di dalam ScrollRect.")]
    public RectTransform itemTransform;

    [Tooltip("Tingkat kesulitan untuk lagu ini.")]
    public Difficulty difficulty;
}

public class DiagonalScrollSnap : MonoBehaviour
{
    [Header("Core Components")]
    public ScrollRect scrollRect;
    public RectTransform content;
    public RectTransform viewport;
    public GameObject menuCanvas;

    [Header("Song Lists")]
    [Tooltip("Daftar LENGKAP semua lagu yang tersedia. Atur difficulty di sini.")]
    public List<SongItemData> allSongItems;
    private List<SongItemData> activeItems = new List<SongItemData>();

    [Header("Snapping & Visuals")]
    public float snapSpeed = 5f;
    public float highlightScale = 1.2f;
    [Tooltip("Jarak horizontal antar item. Harus konsisten.")]
    public float spacing = 170f;

    [Header("Input")]
    public KeyCode inputUp;
    public KeyCode inputDown;
    public KeyCode inputSelect;
    public KeyCode inputBack;

    [Header("Menu Preview")]
    public Image imageBG;
    public SpriteRenderer imageAlbum;
    public AudioSource audioSourcePreview;
    public Text songTitle;
    public Text songDificulity; // Pastikan nama variabel cocok dengan Inspector Anda
    public Text songCredit;

    private int currentIndex = 0;
    private float targetPos;

    void Start()
    {
        FilterActiveItems();
        UpdateItemVisibility();

        if (activeItems.Count > 0)
        {
            currentIndex = 0;
            SnapToIndex(currentIndex);
            PreviewMenu();

            // --- PERBAIKAN ---

            // 1. Putar audio saat mulai
            if (audioSourcePreview != null)
            {
                audioSourcePreview.Play();
            }

            // 2. Atur posisi dan skala awal secara instan tanpa Lerp
            // Ini membuat tampilan awal langsung benar tanpa animasi.
            content.anchoredPosition = new Vector2(targetPos, content.anchoredPosition.y);
            for (int i = 0; i < activeItems.Count; i++)
            {
                float scale = (i == currentIndex) ? highlightScale : 1f;
                activeItems[i].itemTransform.localScale = Vector3.one * scale;
            }
        }
        else
        {
            Debug.LogError("Tidak ada item aktif yang bisa ditampilkan setelah filter!", this);
        }
    }

    void Update()
    {
        if (activeItems.Count == 0) return;

        HandleInput();
        // Fungsi Lerp di Update akan bekerja untuk transisi SETELAH input pertama
        SnapContent();
        HighlightItem();
    }

    private void FilterActiveItems()
    {
        activeItems.Clear();
        int cycleTime = 1;

        if (GameManager.Instance != null)
        {
            cycleTime = GameManager.Instance.cycleTime;
        }
        else
        {
            Debug.LogWarning("GameManager.Instance tidak ditemukan. Menggunakan cycleTime default = 1.", this);
        }

        Debug.Log($"Filtering items for cycleTime: {cycleTime}");

        switch (cycleTime)
        {
            case 2:
                foreach (var song in allSongItems)
                {
                    if (song.difficulty == Difficulty.Medium || song.difficulty == Difficulty.Hard)
                    {
                        activeItems.Add(song);
                    }
                }
                break;
            case 3:
                foreach (var song in allSongItems)
                {
                    if (song.difficulty == Difficulty.Hard)
                    {
                        activeItems.Add(song);
                    }
                }
                break;
            default:
                activeItems.AddRange(allSongItems);
                break;
        }
    }

    private void UpdateItemVisibility()
    {
        HashSet<RectTransform> activeTransforms = new HashSet<RectTransform>();
        foreach (var activeItem in activeItems)
        {
            activeTransforms.Add(activeItem.itemTransform);
        }

        foreach (var masterItem in allSongItems)
        {
            if (masterItem.itemTransform != null)
            {
                bool shouldBeActive = activeTransforms.Contains(masterItem.itemTransform);
                masterItem.itemTransform.gameObject.SetActive(shouldBeActive);
            }
        }
    }

    void HandleInput()
    {
        if (menuCanvas != null && menuCanvas.activeSelf) return;

        if (Input.GetKeyDown(inputDown))
        {
            currentIndex = (currentIndex + 1) % activeItems.Count;
            SnapToIndex(currentIndex);
            PreviewMenu();
            if (audioSourcePreview != null) audioSourcePreview.Play();
        }
        else if (Input.GetKeyDown(inputUp))
        {
            currentIndex = (currentIndex - 1 + activeItems.Count) % activeItems.Count;
            SnapToIndex(currentIndex);
            PreviewMenu();
            if (audioSourcePreview != null) audioSourcePreview.Play();
        }
        else if (Input.GetKeyDown(inputSelect))
        {
            SelectedSong();
        }
        else if (Input.GetKeyDown(inputBack))
        {
            if (menuCanvas != null) menuCanvas.SetActive(false);
        }
    }

    public void SelectedSong()
    {
        if (activeItems.Count == 0) return;
        GameObject obj = activeItems[currentIndex].itemTransform.gameObject;
        DataHolder dataHolder = obj.GetComponent<DataHolder>();

        if (dataHolder != null && dataHolder.beatmapData != null)
        {
            BeatmapTransfer.Instance.CopyData(dataHolder.beatmapData);
            if (menuCanvas != null) menuCanvas.SetActive(true);
        }
    }

    void SnapToIndex(int index)
    {
        if (activeItems.Count == 0) return;

        LoopItemPositions();
        
        float itemPos = activeItems[index].itemTransform.anchoredPosition.x;
        // Penyesuaian agar item benar-benar di tengah viewport
        // Anda mungkin perlu mengubah nilai 0 jika pivot viewport tidak di tengah.
        float viewportCenterX = viewport.rect.width * (0.5f - viewport.pivot.x);
        targetPos = -itemPos + viewportCenterX;
    }

    void PreviewMenu()
    {
        if (activeItems.Count == 0) return;
        GameObject obj = activeItems[currentIndex].itemTransform.gameObject;
        DataHolder dataHolder = obj.GetComponent<DataHolder>();

        if (dataHolder != null && dataHolder.beatmapData != null)
        {
            if (imageAlbum != null) imageAlbum.sprite = dataHolder.beatmapData.imageForAlbum;
            if (imageBG != null) imageBG.sprite = dataHolder.beatmapData.imageForBG;
            if (audioSourcePreview != null) audioSourcePreview.clip = dataHolder.beatmapData.audioClipForMenu;
            if (songCredit != null) songCredit.text = dataHolder.beatmapData.songCredit;
            if (songTitle != null) songTitle.text = dataHolder.beatmapData.songTitle;
            if (songDificulity != null) songDificulity.text = dataHolder.beatmapData.songDifficulty;
        }
    }

    void SnapContent()
    {
        if (content != null)
        {
            content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, new Vector2(targetPos, content.anchoredPosition.y), Time.deltaTime * snapSpeed);
        }
    }

    void HighlightItem()
    {
        for (int i = 0; i < activeItems.Count; i++)
        {
            float scale = (i == currentIndex) ? highlightScale : 1f;
            activeItems[i].itemTransform.localScale = Vector3.Lerp(activeItems[i].itemTransform.localScale, Vector3.one * scale, Time.deltaTime * 10f);
        }
    }

    private void LoopItemPositions()
    {
        if (activeItems.Count <= 1) return;

        float centerItemX = activeItems[currentIndex].itemTransform.anchoredPosition.x;
        int halfCount = activeItems.Count / 2;

        for (int i = 0; i < activeItems.Count; i++)
        {
            int offset = i - currentIndex;

            if (offset > halfCount)
            {
                offset -= activeItems.Count;
            }
            else if (offset < -halfCount)
            {
                offset += activeItems.Count;
            }

            float newX = centerItemX + (offset * spacing);
            Vector2 newPos = new Vector2(newX, activeItems[i].itemTransform.anchoredPosition.y);
            
            activeItems[i].itemTransform.anchoredPosition = newPos;
        }
    }
}
