using UnityEngine;
using TMPro;

public class SongListManager : MonoBehaviour
{
    public GameObject[] songObjects;       // 10 background yang sudah ada
    public string[] songTitles;            // 10 judul lagu (isi dari Inspector)

    void Start()
    {
        for (int i = 0; i < songObjects.Length; i++)
        {
            TMP_Text text = songObjects[i].GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = songTitles[i];
            }
        }
    }
}
