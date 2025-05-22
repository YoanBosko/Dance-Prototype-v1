using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(15)]
public class DebuffDoubleSpeed : DebuffBase
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
        SongManager.Instance.noteSpeed = SongManager.Instance.noteSpeed * 2.2f;
    }
}
