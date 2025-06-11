using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(9)]
public class DebuffLowerShadow : DebuffBase
{
    public string targetTag = "Shadow";
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
        GameObject LowerShadow = GameObject.FindGameObjectWithTag(targetTag);
        CanvasShadow Shadow = LowerShadow.GetComponent<CanvasShadow>();
        Shadow.LowerShadow.SetActive(true);
    }
}
