using UnityEngine;
using UnityEngine.UI;

public class LoopingScroll : MonoBehaviour
{
    public ScrollRect ScrollRect;
    public RectTransform viewPortTransform;
    public RectTransform contentPanelTransform;
    public RectTransform[] itemList;
    public float itemSpacing = 10f;  // Adjust for item spacing

    void Start()
    {
        int ItemsToAdd = Mathf.CeilToInt(viewPortTransform.rect.width / (itemList[0].rect.width + itemSpacing));

        // Add items to fill the visible area
        for (int i = 0; i < ItemsToAdd; i++)
        {
            RectTransform RT = Instantiate(itemList[i % itemList.Length], contentPanelTransform);
            RT.SetAsLastSibling();
        }

        // Add additional items to fill space
        for (int i = 0; i < ItemsToAdd; i++)
        {
            int num = itemList.Length - i - 1;
            while (num < 0)
            {
                num += itemList.Length;
            }
            RectTransform RT = Instantiate(itemList[num], contentPanelTransform);
            RT.SetAsFirstSibling();
        }
    }
}
