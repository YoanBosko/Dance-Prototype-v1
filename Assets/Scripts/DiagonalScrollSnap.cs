using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DiagonalScrollSnap : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform content;
    public RectTransform viewport;
    public List<RectTransform> items = new List<RectTransform>();
    // public List<DaftarLagu> items = new List<DaftarLagu>();

    public int visibleItemCount = 5;
    public float snapSpeed = 5f;
    public float highlightScale = 1.2f;
    public float spacing = 170f;

    private int currentIndex = 0;
    private Vector2 targetPosition;
    private float targetPos;

    void Start()
    {
        SnapToIndex(currentIndex);
    }

    void Update()
    {
        HandleInput();
        SnapContent();
        HighlightItem();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentIndex = (currentIndex + 1) % items.Count;
            SnapToIndex(currentIndex);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentIndex = (currentIndex - 1 + items.Count) % items.Count;
            SnapToIndex(currentIndex);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            GameObject obj = items[currentIndex].gameObject;
            DataHolder dataHolder = obj.GetComponent<DataHolder>();
            BeatmapTransfer.Instance.CopyData(dataHolder.beatmapData); 
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