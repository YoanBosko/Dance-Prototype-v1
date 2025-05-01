using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
        }

        // Fill to left
        for (int i = 0; i < itemsToFill; i++)
        {
            int index = itemPrefabs.Length - 1 - (i % itemPrefabs.Length);
            RectTransform item = Instantiate(itemPrefabs[index], contentPanelTransform);
            item.name = "Item_Left_" + i;
            item.SetAsFirstSibling();
            item.localScale = Vector3.one;
        }

        CenterContent();
    }

    void Update()
    {
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
        // Calculate the true center of the content based on the viewport's position
        Vector2 contentSize = contentPanelTransform.rect.size;
        Vector2 viewportSize = viewPortTransform.rect.size;

        // The center is half of the content width minus half of the viewport width
        float centerX = (contentSize.x - viewportSize.x) / 2;

        // Set the target position based on this calculated center
        targetPosition = new Vector2(-centerX, contentPanelTransform.anchoredPosition.y);
        contentPanelTransform.anchoredPosition = targetPosition;
    }

    void HighlightCenteredItem()
    {
        float minDistance = float.MaxValue;
        RectTransform closestItem = null;
        Vector3 viewportCenter = viewPortTransform.position;
        int childCount = contentPanelTransform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            RectTransform item = contentPanelTransform.GetChild(i) as RectTransform;
            float distance = Mathf.Abs(item.position.x - viewportCenter.x);

            // Find the item closest to the viewport center
            if (distance < minDistance)
            {
                minDistance = distance;
                closestItem = item;
            }
        }

        // Highlight the centered item and scale it
        for (int i = 0; i < childCount; i++)
        {
            RectTransform item = contentPanelTransform.GetChild(i) as RectTransform;
            Image img = item.GetComponentInChildren<Image>(); // <-- Use child Image
            Transform child = item.childCount > 0 ? item.GetChild(0) : null;

            bool isSelected = (item == closestItem);

            if (img != null)
            {
                img.color = isSelected ? highlightedColor : normalColor;
            }

            if (child != null)
            {
                // Apply scaling directly to the child element
                child.localScale = isSelected ? Vector3.one * scaleMultiplier : Vector3.one;
            }
        }
    }

    void SnapToNearestItem()
    {
        // Snap the content position to the nearest item
        float currentX = contentPanelTransform.anchoredPosition.x;
        float snappedX = Mathf.Round(currentX / itemWidth) * itemWidth;
        targetPosition = new Vector2(snappedX, contentPanelTransform.anchoredPosition.y);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        SnapToNearestItem();
    }
}