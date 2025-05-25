using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(1)]
public class DebuffZoom : DebuffBase
{
    [Tooltip("Tag yang digunakan untuk mencari Canvas target")]
    public string canvasTag = "CanvasForScaler";

    [Tooltip("Kecepatan looping animasi (semakin kecil, semakin cepat)")]
    public float duration = 2f;

    [Tooltip("Skala minimum")]
    public float minScale = 0.65f;

    [Tooltip("Skala maksimum")]
    public float maxScale = 1.1f;

    [Tooltip("Kurva transisi antara 0 dan 1")]
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    void Start()
    {
        ActivateDebuff();
    }

    public override void ActivateDebuff()
    {
        // Cari semua GameObject dengan tag "CanvasForScaler"
        GameObject[] canvases = GameObject.FindGameObjectsWithTag("CanvasForScaler");

        foreach (GameObject canvasObj in canvases)
        {
            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                // Set mode ke Constant Pixel Size
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

                // Jalankan animasi perubahan scaleFactor
                StartCoroutine(AnimateScaleFactor(scaler));
            }
            else
            {
                Debug.LogWarning($"GameObject '{canvasObj.name}' tidak memiliki komponen CanvasScaler.");
            }
        }
    }

    IEnumerator AnimateScaleFactor(CanvasScaler scaler)
    {
        float time = 0f;

        while (true)
        {
            // Normalisasi waktu (0 - 1)
            float normalizedTime = Mathf.PingPong(time / duration, 1f);

            // Ambil nilai dari kurva
            float evaluated = curve.Evaluate(normalizedTime);

            // Interpolasi scaleFactor berdasarkan nilai kurva
            scaler.scaleFactor = Mathf.Lerp(minScale, maxScale, evaluated);

            time += Time.deltaTime;
            yield return null;
        }
    }
}
