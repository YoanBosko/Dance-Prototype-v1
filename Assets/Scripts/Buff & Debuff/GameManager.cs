using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder (-50)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("List prefab buff (dapat diisi via Inspector atau runtime)")]
    public List<GameObject> buffPrefabs = new List<GameObject>();
    public GameObject debuffPrefabs;

    private List<BuffBase> activeBuffs = new List<BuffBase>();
    private DebuffBase activeDebuffs;

    public int cycleTime = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SampleScene")
        {
            CreateBuffComponents();
            CreateDebuffComponents();
        }
        else
        {
            RemoveBuffComponents();
            RemoveDebuffComponents();
        }

        if (scene.name == "Menu Lagu")
        {
            cycleTime++;
            Debug.Log("Cycle Time: " + cycleTime);
        }

        if (scene.name == "StartGame")
        {
            buffPrefabs.Clear();
            debuffPrefabs = null;
            cycleTime = 0;
            LeaderboardManager.Instance.AddScore(LeaderboardManager.Instance.scoreDataCummulative.score);
        }
    }

    private void CreateBuffComponents()
    {
        foreach (var prefab in buffPrefabs)
        {
            var type = prefab.GetComponent<BuffBase>()?.GetType();
            if (type != null && gameObject.GetComponent(type) == null)
            {
                var buff = (BuffBase)gameObject.AddComponent(type);
                activeBuffs.Add(buff);
                buff.ActivateBuff();
            }
        }
    }

    private void RemoveBuffComponents()
    {
        foreach (var buff in activeBuffs)
        {
            if (buff != null)
                Destroy(buff);
        }

        activeBuffs.Clear();
    }

    private void CreateDebuffComponents()
    {
        var type = debuffPrefabs.GetComponent<DebuffBase>()?.GetType();
        if (type != null && gameObject.GetComponent(type) == null)
        {
            var debuff = (DebuffBase)gameObject.AddComponent(type);
            activeDebuffs = debuff;
            // buff.ActivateDebuff();
        }
    }

    private void RemoveDebuffComponents()
    {
        Destroy(activeDebuffs);
    }

    // private void OnDestroy()
    // {
    //     SceneManager.sceneLoaded -= OnSceneLoaded;
    // }

    // Fungsi untuk menambahkan dari skrip lain
    public void AddBuff(GameObject buffPrefab)
    {
        if (!buffPrefabs.Contains(buffPrefab))
        {
            buffPrefabs.Add(buffPrefab);
        }
    }
    public void AddDebuff(GameObject debuffPrefab)
    {
        debuffPrefabs = debuffPrefab;
    }
}
