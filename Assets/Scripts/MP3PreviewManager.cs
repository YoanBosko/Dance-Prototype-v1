using UnityEngine;
using UnityEngine.UI;

public class MP3PreviewManager : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform content;
    public AudioSource audioSource;
    public AudioClip[] songClips;

    private int lastIndex = -1;

    void Update()
    {
        int index = GetCenteredIndex();

        if (index != lastIndex && index >= 0 && index < songClips.Length)
        {
            lastIndex = index;

            // Cek apakah audio yang sedang diputar berbeda dengan audio baru
            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.clip = songClips[index];
            audioSource.Play();
        }
    }

    int GetCenteredIndex()
    {
        float closest = float.MaxValue;
        int index = -1;

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
