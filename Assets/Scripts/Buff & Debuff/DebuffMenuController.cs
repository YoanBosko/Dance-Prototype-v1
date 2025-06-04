using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class DebuffMenuController : MonoBehaviour
{
    public GameObject[] buttonPrefabs; // Daftar prefab yang tersedia
    public Image image;
    public Text titleText;
    public Text descriptionText;

    //memberi bobot pada sistem randomizer
    private List<float> weights = new List<float>();
    private GameObject selectedPrefab;

    // Start is called before the first frame update
    void Start()
    {
        AssignRandomImage();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            GameManager.Instance.AddDebuff(selectedPrefab);
            // SceneManager.LoadScene("Menu Lagu");
            SceneManager.LoadScene("BuffPicker");
        }
    }

    void AssignRandomImage()
    {
        // Ambil prefab secara acak
        selectedPrefab = buttonPrefabs[Random.Range(0, buttonPrefabs.Length)];

        DebuffLoader debuffLoader = selectedPrefab.GetComponent<DebuffLoader>();
        image.sprite = debuffLoader.image;
        titleText.text = debuffLoader.cardTitleText;
        descriptionText.text = debuffLoader.cardDescriptionText;

        return;
    }

    void SetupWeights()
    {
        weights.Clear();
        for (int i = 0; i < buttonPrefabs.Length; i++)
        {
            if (i >= 0 && i <= 4)
                weights.Add(10f); // 5 item = 50% total, jadi 10% masing-masing
            else if (i >= 5 && i <= 8)
                weights.Add(8.75f); // 4 item = 35%, jadi 8.75% masing-masing
            else if (i >= 9 && i <= 10)
                weights.Add(7.5f); // 2 item = 15%, jadi 7.5% masing-masing
            else
                weights.Add(0f); // di luar range? tidak dipakai
        }
    }
    int GetWeightedRandomIndex(List<float> weights)
    {
        float totalWeight = 0f;
        foreach (float weight in weights)
        {
            totalWeight += weight;
        }

        float randomValue = Random.Range(0, totalWeight);
        float cumulativeWeight = 0f;

        for (int i = 0; i < weights.Count; i++)
        {
            cumulativeWeight += weights[i];
            if (randomValue <= cumulativeWeight)
            {
                return i;
            }
        }

        return weights.Count - 1; // fallback
    }
}
