using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(16)]
public class DebuffVulnerable : DebuffBase
{ 
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
        ScoreManager.harmMultiplier = ScoreManager.harmMultiplier * 2;
    }
}
