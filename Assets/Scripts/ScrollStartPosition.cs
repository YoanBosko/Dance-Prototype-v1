using UnityEngine;
using UnityEngine.UI;

public class ScrollToCenter : MonoBehaviour
{
    public ScrollRect scrollRect;

    void Start()
    {
        // Pastikan scrollRect sudah diset
        if (scrollRect != null)
        {
            // Atur posisi horizontal scroll ke tengah (0.5f = 50%)
            scrollRect.horizontalNormalizedPosition = 0.5f;
        }
    }
}
