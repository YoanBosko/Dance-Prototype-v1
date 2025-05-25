using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(4)]
public class DebuffResultHide : DebuffBase
{
    public string targetTag = "ResultForHide";
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
        GameObject[] resultLoct = GameObject.FindGameObjectsWithTag(targetTag);

        foreach (GameObject obj in resultLoct)
        {
            RectTransform rect = obj.GetComponent<RectTransform>();
            if (rect != null)
            {
                Vector2 anchoredPos = rect.anchoredPosition;
                anchoredPos.y += 500f;
                rect.anchoredPosition = anchoredPos;
            }
            else
            {
                Debug.LogWarning($"GameObject '{obj.name}' tidak memiliki RectTransform.");
            }
        }
    }
}
