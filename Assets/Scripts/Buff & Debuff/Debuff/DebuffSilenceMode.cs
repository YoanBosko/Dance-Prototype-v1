using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(2)]
public class DebuffSilenceMode : DebuffBase
{
    [Tooltip("Tag untuk GameObject yang mengandung AudioSource yang ingin dimute")]
    public string targetTag = "AudioForMute";
    // Start is called before the first frame update
    void Start()
    {
        ActivateDebuff();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void ActivateDebuff()
    {
        // Cari semua GameObject dengan tag tertentu
        GameObject[] audioObjects = GameObject.FindGameObjectsWithTag(targetTag);

        foreach (GameObject obj in audioObjects)
        {
            AudioSource audio = obj.GetComponent<AudioSource>();
            if (audio != null)
            {
                audio.mute = true;
                Debug.Log($"AudioSource pada '{obj.name}' dimute.");
            }
            else
            {
                Debug.LogWarning($"GameObject '{obj.name}' tidak memiliki komponen AudioSource.");
            }
        }
    }
}
