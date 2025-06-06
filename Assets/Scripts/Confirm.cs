using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEditor.PackageManager;

public class Confirm : MonoBehaviour
{
    public KeyCode inputToStart; // atau input4
    public KeyCode input1;
    public GameObject script1;
    public GameObject menuCanvas;
    public UnityEvent inputToStartEvent;

    void Update()
    {
        if (Input.GetKeyDown(input1))
        {
            menuCanvas.gameObject.SetActive(false);
            script1.gameObject.SetActive(true);
        }
        if (Input.GetKeyDown(inputToStart))
        {
            TryLoadBeatmapScene();
        }
    }

    void TryLoadBeatmapScene()
    {
        // Pastikan BeatmapTransfer sudah terisi
        if (BeatmapTransfer.Instance != null && BeatmapTransfer.Instance.targetAsset != null)
        {
            Debug.Log("Loading scene with beatmap: " + BeatmapTransfer.Instance.targetAsset.name);
            inputToStartEvent?.Invoke();
            //SceneManager.LoadScene("SampleScene");
        }
        else
        {
            Debug.LogWarning("Tidak ada BeatmapData yang dikirim. Pastikan input3 ditekan dulu.");
        }
    }
}
