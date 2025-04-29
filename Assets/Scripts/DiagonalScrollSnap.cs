using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiagonalScrollSnap : MonoBehaviour
{
    public RectTransform content;
    public List<RectTransform> items;
    public float snapSpeed = 10f;
    public float snapThreshold = 0.01f;
    public Color highlightColor = Color.green;
    public Color normalColor = Color.white;

    private int currentIndex = 1; // Karena ada dummy di awal
    private Vector2 targetPosition;
    private bool isSnapping = false;

    void Start()
    {
        if (items == null || items.Count == 0) return;

        // Duplicate first and last items for seamless looping
        RectTransform firstClone = Instantiate(items[0], content);
        RectTransform lastClone = Instantiate(items[items.Count - 1], content);

        firstClone.name += "_clone";
        lastClone.name += "_clone";

        firstClone.SetAsLastSibling();
        lastClone.SetAsFirstSibling();

        items.Insert(0, lastClone);
        items.Add(firstClone);

        currentIndex = 1; // item asli pertama
        SnapToItem(currentIndex, true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentIndex++;
            SnapToItem(currentIndex);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentIndex--;
            SnapToItem(currentIndex);
        }

        if (isSnapping)
        {
            content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, targetPosition, Time.deltaTime * snapSpeed);
            if (Vector2.Distance(content.anchoredPosition, targetPosition) < snapThreshold)
            {
                content.anchoredPosition = targetPosition;
                isSnapping = false;

                // Reset index jika menyentuh dummy
                if (currentIndex == 0)
                {
                    currentIndex = items.Count - 2;
                    SnapToItem(currentIndex, true);
                }
                else if (currentIndex == items.Count - 1)
                {
                    currentIndex = 1;
                    SnapToItem(currentIndex, true);
                }
            }
        }
    }

    void SnapToItem(int index, bool instant = false)
    {
        index = Mathf.Clamp(index, 0, items.Count - 1);

        RectTransform viewport = content.parent.GetComponent<RectTransform>();
        Vector2 itemCenter = items[index].anchoredPosition;
        Vector2 viewCenter = viewport.sizeDelta / 2;
        targetPosition = -itemCenter + viewCenter;
        isSnapping = !instant;

        if (instant)
            content.anchoredPosition = targetPosition;

        // Highlight only real items
        for (int i = 1; i < items.Count - 1; i++)
        {
            Image img = items[i].GetComponent<Image>();
            if (img != null)
                img.color = (i == index) ? highlightColor : normalColor;
        }
    }
}
