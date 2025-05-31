using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(20)]
public class DebuffFlashbang : DebuffBase
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
        ScoreManager.isFlashbang = true;
    }
}
