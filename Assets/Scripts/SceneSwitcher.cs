using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public class SceneSwitcherWithSFX : MonoBehaviour
{
    [Header("Scene Settings")]
    public float delayBeforeSwitch = 1.5f;
    public string nextSceneName = "Menu Lagu";

    [Header("Post Process Volume")]
    public PostProcessVolume volume;

    [Header("SFX Settings")]
    public AudioSource audioSource;
    public AudioClip enterSFX;
    public float sfxStartTime = 0f;
    public float sfxEndTime = 2f;

    private LensDistortion lensDistortion;
    private Bloom bloom;
    private AutoExposure autoExposure;

    private float zoomDuration = 1.0f;
    private float zoomTimer = 0f;
    private bool hasPressedKey = false;

    // Target and start values (dari sebelumnya)
    private float startIntensity, startCenterX, startScale;
    private float startBloomIntensity, startBloomThreshold;
    private Color startBloomColor;
    private float startMinExposure;

    private float targetIntensity = -100f;
    private float targetCenterX = 0.012f;
    private float targetScale = 5f;

    private float targetBloomIntensity = 40f;
    private float targetBloomThreshold = 0f;
    private Color targetBloomColor = new Color(132f / 255f, 255f / 255f, 255f / 255f);
    private float targetMinExposure = -5f;

    void Start()
    {
        volume.profile.TryGetSettings(out lensDistortion);
        volume.profile.TryGetSettings(out bloom);
        volume.profile.TryGetSettings(out autoExposure);

        if (lensDistortion != null)
        {
            startIntensity = lensDistortion.intensity.value;
            startCenterX = lensDistortion.centerX.value;
            startScale = lensDistortion.scale.value;
        }

        if (bloom != null)
        {
            startBloomIntensity = bloom.intensity.value;
            startBloomThreshold = bloom.threshold.value;
            startBloomColor = bloom.color.value;
        }

        if (autoExposure != null)
        {
            startMinExposure = autoExposure.minLuminance.value;
        }
    }

    void Update()
    {
        if (!hasPressedKey &&
            (Input.GetKeyDown(KeyCode.D) ||
             Input.GetKeyDown(KeyCode.F) ||
             Input.GetKeyDown(KeyCode.J) ||
             Input.GetKeyDown(KeyCode.K)))
        {
            hasPressedKey = true;
            zoomTimer = 0f;

            // Play SFX from custom time range
            if (enterSFX != null && audioSource != null)
            {
                audioSource.clip = enterSFX;
                audioSource.time = sfxStartTime;
                audioSource.Play();
                Invoke(nameof(StopSFX), sfxEndTime - sfxStartTime);
            }
        }

        if (hasPressedKey)
        {
            zoomTimer += Time.deltaTime;
            float t = Mathf.Clamp01(zoomTimer / zoomDuration);
            float smoothT = Mathf.SmoothStep(0, 1, t);

            if (lensDistortion != null)
            {
                lensDistortion.intensity.value = Mathf.Lerp(startIntensity, targetIntensity, smoothT);
                lensDistortion.centerX.value = Mathf.Lerp(startCenterX, targetCenterX, smoothT);
                lensDistortion.scale.value = Mathf.Lerp(startScale, targetScale, smoothT);
            }

            if (bloom != null)
            {
                bloom.intensity.value = Mathf.Lerp(startBloomIntensity, targetBloomIntensity, smoothT);
                bloom.threshold.value = Mathf.Lerp(startBloomThreshold, targetBloomThreshold, smoothT);
                bloom.color.value = Color.Lerp(startBloomColor, targetBloomColor, smoothT);
            }

            if (autoExposure != null)
            {
                autoExposure.minLuminance.value = Mathf.Lerp(startMinExposure, targetMinExposure, smoothT);
            }

            if (zoomTimer >= delayBeforeSwitch)
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }

    void StopSFX()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
