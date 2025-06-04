using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuffPrefab", menuName = "BuffPrefab")]
public class PrefabMenuController : ScriptableObject
{
    public List<GameObject> buffPrefabs; // Daftar prefab yang tersedia
    public List<GameObject> removedBuffPrefabs;
}
