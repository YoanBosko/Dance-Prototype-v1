using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // public List<GameObject> buttonPrefabs; // Daftar prefab yang tersedia
    public PrefabMenuController buttonPrefabs;
    public Transform buttonParent; // Parent untuk menampung instansiasi button
    public Vector3[] buttonPositions; // Posisi untuk menampilkan button

    public Button[] menuButtons; // Button aktif yang bisa dikontrol
    private int selectedIndex = 0;
    public float highlightScale = 1.2f;
    public float transitionSpeed = 10f;

    void Start()
    {
        PickRandomButtons();
        HighlightButton();
        if (GameManager.Instance.cycleTime == 1)
        {
            foreach (GameObject obj in buttonPrefabs.removedBuffPrefabs)
            {
                if (!buttonPrefabs.buffPrefabs.Contains(obj))
                {
                    buttonPrefabs.buffPrefabs.Add(obj);
                }
            }
            buttonPrefabs.removedBuffPrefabs.Clear(); // Kosongkan agar tidak double
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            selectedIndex = (selectedIndex + 1) % menuButtons.Length;
            HighlightButton();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            selectedIndex = (selectedIndex - 1 + menuButtons.Length) % menuButtons.Length;
            HighlightButton();
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            menuButtons[selectedIndex].onClick.Invoke();
            string menuButtonsName = menuButtons[selectedIndex].name.Replace("(Clone)", "").Trim();
            GameObject selectedGameObject = buttonPrefabs.buffPrefabs.Find(obj => obj.name == menuButtonsName);
            buttonPrefabs.buffPrefabs.Remove(selectedGameObject);
            buttonPrefabs.removedBuffPrefabs.Add(selectedGameObject);
            // if (GameManager.Instance.cycleTime == 2)
            // {
            //     SceneManager.LoadScene("DebuffScene");
            // }
            // else
            // {                
            //     SceneManager.LoadScene("Menu Lagu");
            // }
            SceneManager.LoadScene("DebuffScene");
        }

        UpdateButtonScales();
    }

    void HighlightButton()
    {
        EventSystem.current.SetSelectedGameObject(menuButtons[selectedIndex].gameObject);
        for (int i = 0; i < menuButtons.Length; i++)
        {
            BuffLoader buffLoader = menuButtons[i].gameObject.GetComponent<BuffLoader>();
            if (i == selectedIndex) buffLoader.ActivateMoreDescription();
            else buffLoader.DeactivateMoreDescription();
        }
    }

    void UpdateButtonScales()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            Vector3 targetScale = (i == selectedIndex) ? new Vector3(highlightScale, highlightScale, 1f) : Vector3.one;
            menuButtons[i].transform.localScale = Vector3.Lerp(
                menuButtons[i].transform.localScale,
                targetScale,
                Time.deltaTime * transitionSpeed
            );
        }
    }

    void PickRandomButtons()
    {
        // Pastikan kita punya posisi yang cukup
        if (buttonPositions.Length < 3 || buttonPrefabs.buffPrefabs.Count < 3)
        {
            Debug.LogWarning("Tidak cukup posisi atau prefab untuk memilih 3 button.");
            return;
        }

        List<int> chosenIndexes = new List<int>();
        menuButtons = new Button[3];

        for (int i = 0; i < 3; i++)
        {
            int randIndex;
            do
            {
                randIndex = Random.Range(0, buttonPrefabs.buffPrefabs.Count);
            } while (chosenIndexes.Contains(randIndex));

            chosenIndexes.Add(randIndex);

            GameObject buttonInstance = Instantiate(buttonPrefabs.buffPrefabs[randIndex], buttonParent);
            buttonInstance.transform.localPosition = buttonPositions[i];
            menuButtons[i] = buttonInstance.GetComponent<Button>();
        }
    }
}




