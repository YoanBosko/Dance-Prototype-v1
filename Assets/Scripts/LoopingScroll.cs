using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LoopingScroll : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    public ScrollRect scrollRect;
    public RectTransform viewPortTransform;
    public RectTransform contentPanelTransform;
    public RectTransform[] itemList;
    public float itemSpacing = 10f;
    public float scrollSpeed = 500f;
    public float smoothTime = 0.1f;
    public float snapSpeed = 10f;  // Kecepatan snap saat menggulir

    private float itemWidth;
    private float totalContentWidth;
    private Vector2 velocity = Vector2.zero;
    private Vector2 targetPosition;
    private float centerOffset;
    private bool isDragging = false;
    private int currentIndex = 0;  // Untuk melacak item yang berada di tengah

    bool isSnapped;

    void Start()
    {
        isSnapped = false;

        if (itemList.Length == 0) return;

        itemWidth = itemList[0].rect.width + itemSpacing;
        int itemsToAdd = Mathf.CeilToInt(viewPortTransform.rect.width / itemWidth);

        // Menambah item ke konten
        for (int i = 0; i < itemsToAdd; i++)
        {
            RectTransform rt = Instantiate(itemList[i % itemList.Length], contentPanelTransform);
            rt.SetAsLastSibling();
        }

        // Menambah item di sisi kiri
        for (int i = 0; i < itemsToAdd; i++)
        {
            int num = itemList.Length - i - 1;
            while (num < 0) num += itemList.Length;

            RectTransform rt = Instantiate(itemList[num], contentPanelTransform);
            rt.SetAsFirstSibling();
        }

        int totalItems = contentPanelTransform.childCount;
        totalContentWidth = itemWidth * totalItems;

        centerOffset = -itemWidth * (totalItems / 2);
        targetPosition = new Vector2(centerOffset, contentPanelTransform.anchoredPosition.y);
        contentPanelTransform.anchoredPosition = targetPosition;
    }

    void Update()
    {
        if (!isDragging)
        {
            // Melakukan transisi halus ke posisi target
            contentPanelTransform.anchoredPosition = Vector2.SmoothDamp(
                contentPanelTransform.anchoredPosition,
                targetPosition,
                ref velocity,
                smoothTime
            );
        }

        // Kecepatan scrolling dengan tombol panah (opsional)
        if (Input.GetKey(KeyCode.RightArrow))
        {
            targetPosition += new Vector2(-scrollSpeed * Time.deltaTime, 0);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            targetPosition += new Vector2(scrollSpeed * Time.deltaTime, 0);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        // Melakukan snapping ke item terdekat
        SnapToNearestItem();
    }

    void SnapToNearestItem()
    {
        // Menghitung posisi item terdekat berdasarkan posisi sekarang
        float nearestPosX = Mathf.Round(contentPanelTransform.anchoredPosition.x / itemWidth) * itemWidth;

        // Debugging
        Debug.Log("Nearest Position X: " + nearestPosX);

        // Menentukan item yang terletak di tengah
        currentIndex = Mathf.RoundToInt((contentPanelTransform.anchoredPosition.x - centerOffset) / itemWidth);
        currentIndex = Mathf.Clamp(currentIndex, 0, contentPanelTransform.childCount - 1);

        // Menyesuaikan posisi target untuk snapping
        targetPosition = new Vector2(nearestPosX, contentPanelTransform.anchoredPosition.y);
    }
}
