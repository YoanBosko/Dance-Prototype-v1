using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffTimingTolerance : BuffBase
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
        SongManager.Instance.marginOfErrorPerfect = 0.15f;
        SongManager.Instance.marginOfErrorGood = 0.25f;
        SongManager.Instance.marginOfErrorBad = 0.45f;
    }
}
