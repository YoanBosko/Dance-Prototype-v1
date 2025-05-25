using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(6)]
public class DebuffHPHide : DebuffBase
{
    public string targetTag = "HPForHide";
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
        GameObject[] HP = GameObject.FindGameObjectsWithTag(targetTag);

        foreach (GameObject hp in HP) 
        { 
            Image hpImage = hp.GetComponent<Image>();
            if (hpImage != null)
            {
                hpImage.enabled = false;
            }
        }
    }
}
