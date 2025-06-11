using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasShadow : MonoBehaviour
{
    public GameObject UpperShadow;
    public GameObject LowerShadow;
    // Start is called before the first frame update
    void Start()
    {
        UpperShadow.SetActive(false);
        LowerShadow.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
