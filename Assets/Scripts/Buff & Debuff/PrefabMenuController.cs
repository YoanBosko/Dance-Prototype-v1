using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuffPrefab", menuName = "BuffPrefab")]
public class PrefabMenuController : ScriptableObject
{
    public List<WeightedBuffPrefab> availableBuffs = new List<WeightedBuffPrefab>(); // Daftar prefab yang tersedia
    public List<WeightedBuffPrefab> removedBuffs = new List<WeightedBuffPrefab>();
}
