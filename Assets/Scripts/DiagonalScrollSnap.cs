using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiagonalScrollSnap : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform content;
    public RectTransform viewport;
    public List<RectTransform> items = new List<RectTransform>();
    public float spacing = 100f; // Jarak antar item
    public float snapSpeed = 5f;
    public float highlightScale = 1.2f;

    private int currentIndex = 0;
    private Vector2 targetPosition;

    void Start()
    {
        ArrangeItemsDiagonally();
        SnapToIndex(currentIndex);
    }

    void Update()
    {
        HandleArrowInput();
        SnapContent();
        HighlightItem();
        CheckAndRepositionItems();
    }

    void HandleArrowInput()
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
    }

    void SnapToIndex(int index)
    {
        if (items.Count == 0) return;

        Vector2 itemLocalPos = items[index].anchoredPosition;
        Vector2 viewportCenter = viewport.rect.size / 2f;
        targetPosition = -itemLocalPos + viewportCenter;
    }

    void SnapContent()
    {
        content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, targetPosition, Time.deltaTime * snapSpeed);
    }

    void HighlightItem()
    {
        for (int i = 0; i < items.Count; i++)
        {
            float scale = (i == currentIndex) ? highlightScale : 1f;
            items[i].localScale = Vector3.Lerp(items[i].localScale, Vector3.one * scale, Time.deltaTime * 10f);
        }
    }

    void ArrangeItemsDiagonally()
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].anchoredPosition = new Vector2(i * spacing, -i * spacing); // diagonal kanan bawah
        }

        // Atur ukuran content agar cukup besar
        float width = (items.Count + 1) * spacing;
        content.sizeDelta = new Vector2(width, width);
    }

    void CheckAndRepositionItems()
    {
        float leftBound = content.anchoredPosition.x - viewport.rect.width / 2f - spacing;
        float rightBound = content.anchoredPosition.x + viewport.rect.width / 2f + spacing;

        foreach (RectTransform item in items)
        {
            Vector2 pos = item.anchoredPosition;
            float itemX = pos.x + content.anchoredPosition.x;

            // Jika keluar di kiri, pindah ke kanan
            if (itemX < leftBound)
            {
                pos.x += items.Count * spacing;
                item.anchoredPosition = pos;
            }
            // Jika keluar di kanan, pindah ke kiri
            else if (itemX > rightBound)
            {
                pos.x -= items.Count * spacing;
                item.anchoredPosition = pos;
            }
        }
    }
}
