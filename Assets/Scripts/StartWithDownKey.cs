using UnityEngine;
using UnityEngine.SceneManagement;

public class StartWithDownKey : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "Menu Lagu"; // Ganti dengan nama scene tujuan

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Debug.Log("Tombol bawah ditekan!"); // Cek di Console
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
