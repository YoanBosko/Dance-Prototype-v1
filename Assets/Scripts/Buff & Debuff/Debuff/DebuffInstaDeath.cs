using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class DebuffInstaDeath : DebuffBase
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
        ScoreManager.isInstaDeath = true;
    }
}
