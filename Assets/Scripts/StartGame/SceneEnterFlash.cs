using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class SceneEnterFlash : MonoBehaviour
{
    public PostProcessVolume volume;

    [Header("Flash Settings")]
    public float transitionDuration = 1f;

    private AutoExposure autoExposure;
    private Bloom bloom;

    private float timer = 0f;
    private bool isTransitioning = false;

    // FROM -> TO values
    private float startEV = -5f;
    private float endEV = 1f;

    private float startBloomIntensity = 0.8f;
    private float endBloomIntensity = 0f;

    void Start()
    {
        volume.enabled = false; // efek tidak aktif saat di editor

        if (volume.profile.TryGetSettings(out autoExposure) &&
            volume.profile.TryGetSettings(out bloom))
        {
            // Set nilai awal untuk efek
            autoExposure.minLuminance.value = startEV;
            autoExposure.maxLuminance.value = startEV;
            bloom.intensity.value = startBloomIntensity;

            // Aktifkan efek baru memulai transisi
            volume.enabled = true;
            isTransitioning = true;
        }
    }

    void Update()
    {
        if (!isTransitioning) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / transitionDuration);
        float smoothT = Mathf.SmoothStep(0, 1, t);

        if (autoExposure != null)
        {
            autoExposure.minLuminance.value = Mathf.Lerp(startEV, endEV, smoothT);
            autoExposure.maxLuminance.value = Mathf.Lerp(startEV, endEV, smoothT);
        }

        if (bloom != null)
        {
            bloom.intensity.value = Mathf.Lerp(startBloomIntensity, endBloomIntensity, smoothT);
        }

        if (t >= 1f)
        {
            isTransitioning = false;
            volume.enabled = false;
        }
    }
}
