using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(9)]
public class DebuffUpperShadow : DebuffBase
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
        GameObject UpperShadow = GameObject.FindGameObjectWithTag(targetTag);
        CanvasShadow Shadow = UpperShadow.GetComponent<CanvasShadow>();
        Shadow.UpperShadow.SetActive(true);
    }
}
