using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public float delayBeforeSwitch = 0.5f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.LeftArrow) ||
            Input.GetKeyDown(KeyCode.RightArrow))
        {
            Invoke(nameof(SwitchScene), delayBeforeSwitch);
        }
    }

    void SwitchScene()
    {
        SceneManager.LoadScene("Menu Lagu");
    }
}
