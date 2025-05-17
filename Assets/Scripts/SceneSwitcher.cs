using System.Collections;
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
        // SceneManager.LoadScene("Menu Lagu");
        StartCoroutine(LoadSceneAsync("Menu Lagu"));
    }
    IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        asyncLoad.allowSceneActivation = true;
    }
}
