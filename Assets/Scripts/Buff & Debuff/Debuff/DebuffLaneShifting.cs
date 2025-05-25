using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(7)]
public class DebuffLaneShifting : DebuffBase
{
    public string targetTag = "LaneForHide";
    // Start is called before the first frame update

    [Header("Animasi")]
    public float amplitude = 100f;     // Jarak naik-turun
    public float speed = 1f;          // Kecepatan animasi
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
        GameObject[] laneShifts = GameObject.FindGameObjectsWithTag(targetTag);

        foreach (GameObject laneShift in laneShifts)
        {
            RectTransform rect = laneShift.GetComponent<RectTransform>();
            if (rect != null)
            {
                StartCoroutine(AnimatePositionFactor(rect));
            }
        }
    }
    IEnumerator AnimatePositionFactor(RectTransform rect)
    {
        Vector2 originalPos = rect.anchoredPosition;
        float time = 0f;

        while (true)
        {
            float offsetY = Mathf.Sin(time * speed) * amplitude;
            rect.anchoredPosition = originalPos + new Vector2(0f, offsetY);
            time += Time.deltaTime;

            yield return null;
        }
    }
}
