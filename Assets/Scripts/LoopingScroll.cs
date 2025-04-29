using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class LoopingScroll : MonoBehaviour
{
    [Header("References")]
    public ScrollRect scrollRect;
    public RectTransform viewPortTransform;
    public RectTransform contentPanelTransform;
    public RectTransform[] itemPrefabs;

    [Header("Layout Settings")]
    public float itemSpacingX = 10f;

    [Header("Highlight Settings")]
    public Color normalColor = Color.white;
    public Color highlightedColor = Color.yellow;
    public float scaleMultiplier = 1.5f;

    private float itemWidth;
    private Vector2 targetPosition;
    private Vector2 velocity = Vector2.zero;
    private bool isDragging = false;

    private List<RectTransform> allItems = new List<RectTransform>();

    void Start()
    {
        if (itemPrefabs.Length == 0) return;

        itemWidth = itemPrefabs[0].rect.width + itemSpacingX;
        int itemsToFill = Mathf.CeilToInt(viewPortTransform.rect.width / itemWidth) + 2;

        // Fill to right
        for (int i = 0; i < itemsToFill; i++)
        {
            RectTransform item = Instantiate(itemPrefabs[i % itemPrefabs.Length], contentPanelTransform);
            item.name = "Item_Right_" + i;
            item.localScale = Vector3.one;
            allItems.Add(item);
        }

        // Fill to left
        for (int i = 0; i < itemsToFill; i++)
        {
            int index = itemPrefabs.Length - 1 - (i % itemPrefabs.Length);
            RectTransform item = Instantiate(itemPrefabs[index], contentPanelTransform);
            item.name = "Item_Left_" + i;
            item.SetAsFirstSibling();
            item.localScale = Vector3.one;
            allItems.Insert(0, item);
        }

        CenterContent();
    }

    void Update()
    {
        isDragging = Input.GetMouseButton(0);

        if (!isDragging)
        {
            contentPanelTransform.anchoredPosition = Vector2.SmoothDamp(
                contentPanelTransform.anchoredPosition,
                targetPosition,
                ref velocity,
                0.1f
            );

            if (Vector2.Distance(contentPanelTransform.anchoredPosition, targetPosition) < 0.1f)
            {
                SnapToNearestItem();
            }
        }

        HandleKeyboardInput();
        HighlightCenteredItem();
    }

    void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            targetPosition += new Vector2(-itemWidth, 0);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            targetPosition += new Vector2(itemWidth, 0);
        }
    }

    void CenterContent()
    {
        float centerX = (contentPanelTransform.rect.width - viewPortTransform.rect.width) / 2f;
        targetPosition = new Vector2(-centerX, contentPanelTransform.anchoredPosition.y);
        contentPanelTransform.anchoredPosition = targetPosition;
    }

    void SnapToNearestItem()
    {
        float minDistance = float.MaxValue;
        RectTransform nearestItem = null;
        Vector2 viewportCenter = viewPortTransform.position;

        foreach (var item in allItems)
        {
            float distance = Mathf.Abs(item.position.x - viewportCenter.x);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestItem = item;
            }
        }

        if (nearestItem != null)
        {
            Vector2 itemLocalPos = (Vector2)contentPanelTransform.InverseTransformPoint(nearestItem.position);
            Vector2 centerOffset = new Vector2(itemLocalPos.x, 0);
            targetPosition = contentPanelTransform.anchoredPosition - centerOffset;
        }
    }

    void HighlightCenteredItem()
    {
        Vector2 centerWorldPos = viewPortTransform.position;
        RectTransform closestItem = null;
        float closestDistance = float.MaxValue;

        foreach (var item in allItems)
        {
            float distance = Mathf.Abs(item.position.x - centerWorldPos.x);
            Image img = item.GetComponent<Image>();

            if (img != null)
                img.color = normalColor;

            item.localScale = Vector3.one;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestItem = item;
            }
        }

        if (closestItem != null)
        {
            Image img = closestItem.GetComponent<Image>();
            if (img != null)
                img.color = highlightedColor;

            closestItem.localScale = Vector3.one * scaleMultiplier;
        }
    }
}
