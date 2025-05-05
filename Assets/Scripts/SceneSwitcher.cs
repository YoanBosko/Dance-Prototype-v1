using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public string targetScene = "Menu Lagu"; // Ganti dengan nama scene tujuan
    private SerialController serialController;
    private bool sceneSwitched = false; // Cegah perpindahan berulang

    void Start()
    {
        serialController = GameObject.Find("SerialController").GetComponent<SerialController>();
    }

    void Update()
    {
        if (sceneSwitched) return;

        string message = serialController.ReadSerialMessage();
        if (message == null) return;

        Debug.Log("RFID Data: " + message);

        if (message.Contains("UID:"))
        {
            sceneSwitched = true; // Supaya tidak ganti scene berkali-kali
            SceneManager.LoadScene(targetScene);
        }
    }
}