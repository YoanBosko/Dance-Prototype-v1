using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(15)]
public class BuffSlowNote : BuffBase
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
        SongManager.Instance.noteSpeed = 3f;
    }
}
