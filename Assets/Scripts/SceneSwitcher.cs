using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    void Update()
    {
        // Cek input dari tombol panah
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SwitchScene("Menu Lagu"); // Ganti dengan nama scene yang sesuai
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SwitchScene("Menu Lagu"); // Ganti dengan nama scene yang sesuai
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SwitchScene("Menu Lagu"); // Ganti dengan nama scene yang sesuai
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SwitchScene("Menu Lagu"); // Ganti dengan nama scene yang sesuai
        }
    }

    // Fungsi untuk memindahkan scene
    void SwitchScene(string sceneName)
    {
        // Memuat scene dengan nama yang diberikan
        SceneManager.LoadScene(sceneName);
    }
}
