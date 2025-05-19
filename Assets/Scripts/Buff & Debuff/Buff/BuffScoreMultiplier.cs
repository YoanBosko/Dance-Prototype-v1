using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffScoreMultiplier : BuffBase
{ 
    // Start is called before the first frame update
    void Start()
    {
        ActivateBuff();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void ActivateBuff()
    {
        ScoreManager.scorePerfectMultiplier = ScoreManager.scorePerfectMultiplier * 2;
        ScoreManager.scoreMultiplier = ScoreManager.scoreMultiplier * 2;
    }
}
