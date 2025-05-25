using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(7)]
public class DebuffLaneDisappear : DebuffBase
{
    public string targetTag = "LaneForHide";
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
        GameObject[] laneHides = GameObject.FindGameObjectsWithTag(targetTag);

        foreach (GameObject laneHide in laneHides)
        {
            laneHide.SetActive(false);
        }
    }
}
