using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwipeMenu : MonoBehaviour
{
    public Scrollbar scrollbar;
    public GameObject contentPanel;
    public float scaleFactor = 1.2f;  // Scale factor for the selected item
    private float scrollPos = 0;
    private float[] pos;
    private float distance;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize position array size and set up scrolling distance
        InitializePositions();
        // Setup scrollbar's initial value
        scrollbar.value = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // Recalculate positions if the number of children changes
        if (pos.Length != transform.childCount)
        {
            InitializePositions();
        }

        // Get the current scrollbar value (scroll position)
        scrollPos = scrollbar.value;

        // Implement snapping and scaling logic
        for (int i = 0; i < pos.Length; i++)
        {
            // If current scroll position is near the position of a child, smoothly scroll to it
            if (scrollPos < pos[i] + (distance / 2) && scrollPos > pos[i] - (distance / 2))
            {
                scrollbar.value = Mathf.Lerp(scrollbar.value, pos[i], 0.1f);
                SnapAndScaleItem(i);  // Snap and scale the centered item
            }
        }

        // Looping behavior for scrolling content
        LoopItems();

        // Right and Left Arrow key input for navigation
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ScrollToNextItem();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ScrollToPreviousItem();
        }
    }

    // Function to scale the centered item and reset others
    void SnapAndScaleItem(int index)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            if (i == index)  // The centered item
            {
                child.localScale = Vector3.Lerp(child.localScale, new Vector3(scaleFactor, scaleFactor, 1f), 0.1f);
            }
            else  // Non-centered items
            {
                child.localScale = Vector3.Lerp(child.localScale, new Vector3(1f, 1f, 1f), 0.1f);
            }
        }
    }

    // Loop items so they wrap around when the scroll reaches the end or beginning
    void LoopItems()
    {
        // Check if the child count is valid before accessing children
        if (transform.childCount == 0) return;

        // Check the current position of the first and last items
        float firstItemPos = transform.GetChild(0).localPosition.x;
        float lastItemPos = transform.GetChild(transform.childCount - 1).localPosition.x;

        // If the first item is off-screen to the left, move it to the end
        if (firstItemPos < -500f)
        {
            Transform firstItem = transform.GetChild(0);
            firstItem.SetAsLastSibling(); // Move it to the end
        }

        // If the last item is off-screen to the right, move it to the beginning
        if (lastItemPos > 500f)
        {
            Transform lastItem = transform.GetChild(transform.childCount - 1);
            lastItem.SetAsFirstSibling(); // Move it to the front
        }
    }

    // Function to scroll to the next item
    void ScrollToNextItem()
    {
        float nextPos = Mathf.Min(scrollbar.value + distance, 1f);  // Ensure it doesn't exceed 1
        scrollbar.value = nextPos;
    }

    // Function to scroll to the previous item
    void ScrollToPreviousItem()
    {
        float prevPos = Mathf.Max(scrollbar.value - distance, 0f);  // Ensure it doesn't go below 0
        scrollbar.value = prevPos;
    }

    // Function to initialize position array
    void InitializePositions()
    {
        pos = new float[transform.childCount];
        distance = 1f / (pos.Length - 1f);

        // Calculate the positions of all children based on their relative distances
        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = distance * i;
        }
    }
}
