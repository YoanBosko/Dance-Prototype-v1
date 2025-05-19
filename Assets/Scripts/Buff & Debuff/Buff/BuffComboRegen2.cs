using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffComboRegen2 : BuffBase
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
        ScoreManager.isRegen2 = true;
    }
}
