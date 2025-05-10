using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PreviewManager : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform content;
    public Image previewBackground;
    public TMP_Text previewTitleText;               // << Tambahkan ini
    public Sprite[] backgroundSprites;              // Sesuai urutan
    public string[] songTitles;                     // << Tambahkan judul lagu

    private int lastIndex = -1;

    void Update()
    {
        int index = GetCenteredIndex();
        if (index != lastIndex)
        {
            lastIndex = index;
            previewBackground.sprite = backgroundSprites[index];
            previewTitleText.text = songTitles[index];  // << Ganti teks di sini
        }
    }

    int GetCenteredIndex()
    {
        float closest = float.MaxValue;
        int index = 0;

        for (int i = 0; i < content.childCount; i++)
        {
            RectTransform child = content.GetChild(i) as RectTransform;
            float distance = Mathf.Abs(child.transform.position.x - Screen.width / 2);
            if (distance < closest)
            {
                closest = distance;
                index = i;
            }
        }

        return index;
    }
}
