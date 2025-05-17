using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffLoader : MonoBehaviour
{
    public GameObject newBuffPrefab;

    public void AddBuffToList()
    {
        GameManager.Instance.AddBuff(newBuffPrefab);
    }
}