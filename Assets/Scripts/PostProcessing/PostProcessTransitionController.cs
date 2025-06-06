using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;

public class PostProcessTransitionController : MonoBehaviour
{
    [Header("Post Process Settings")]
    public PostProcessVolume volume;

    [Header("Transition Timing")]
    public float defaultTransitionDuration = 1.0f; // Durasi untuk animasi efek utama (Lens, Bloom, dll.)

    [Header("Volume Weight Animation")]
    [Tooltip("Durasi untuk transisi weight PostProcessVolume dari 0 ke 1 dan 1 ke 0. Hanya aktif jika weight awal adalah 0.")]
    [SerializeField] private float volumeWeightFadeDuration = 0.2f; // Durasi untuk weight fade in/out

    // Target values untuk efek (bisa diatur via Inspector)
    [Header("Lens Distortion Target")]
    [SerializeField] private bool animateLensDistortion = true;
    [SerializeField] private float targetIntensity = -100f;
    [SerializeField] private float targetCenterX = 0.012f;
    [SerializeField] private float targetScale = 5f;

    [Header("Bloom Target")]
    [SerializeField] private bool animateBloom = true;
    [SerializeField] private float targetBloomIntensity = 40f;
    [SerializeField] private float targetBloomThreshold = 0f;
    [SerializeField] private Color targetBloomColor = new Color(132f / 255f, 255f / 255f, 255f / 255f);

    [Header("Auto Exposure Target")]
    [SerializeField] private bool animateAutoExposure = true;
    [SerializeField] private float targetMinExposure = -5f;

    // Komponen Post-Processing
    private LensDistortion lensDistortion;
    private Bloom bloom;
    private AutoExposure autoExposure;

    // Nilai awal efek
    private float startIntensity, startCenterX, startScale;
    private float startBloomIntensity, startBloomThreshold;
    private Color startBloomColor = Color.white;
    private float startMinExposure;

    private Coroutine activeTransitionCoroutine;

    void Awake()
    {
        if (volume == null)
        {
            Debug.LogError("PostProcessVolume belum di-assign di PostProcessTransitionController.", this);
            enabled = false;
            return;
        }
        volume.profile.TryGetSettings(out lensDistortion);
        volume.profile.TryGetSettings(out bloom);
        volume.profile.TryGetSettings(out autoExposure);
    }

    public void StartTransition(float duration = -1f, System.Action onComplete = null)
    {
        if (volume == null)
        {
            Debug.LogError("Tidak bisa memulai transisi, PostProcessVolume tidak di-assign.", this);
            onComplete?.Invoke();
            return;
        }

        float actualMainEffectsDuration = (duration < 0) ? defaultTransitionDuration : duration;

        if (activeTransitionCoroutine != null)
        {
            StopCoroutine(activeTransitionCoroutine);
        }
        activeTransitionCoroutine = StartCoroutine(TransitionCoroutine(actualMainEffectsDuration, onComplete));
    }

    private IEnumerator TransitionCoroutine(float mainEffectsDuration, System.Action onComplete)
    {
        bool initialWeightWasZero = false;
        if (volume != null && Mathf.Approximately(volume.weight, 0f))
        {
            initialWeightWasZero = true;
        }

        // --- 1. Opsional: Fade Volume Weight IN ---
        if (initialWeightWasZero && volume != null)
        {
            Debug.Log("Initial volume weight is 0. Fading weight IN.");
            float weightTimer = 0f;
            // float startWeightValue = volume.weight; // Seharusnya 0
            while (weightTimer < volumeWeightFadeDuration)
            {
                weightTimer += Time.deltaTime;
                // Lerp dari 0 ke 1 untuk volume.weight
                volume.weight = Mathf.Lerp(0f, 1f, Mathf.Clamp01(weightTimer / volumeWeightFadeDuration));
                yield return null;
            }
            volume.weight = 1f; // Pastikan mencapai 1
        }
        else if (volume != null)
        {
            Debug.Log($"Initial volume weight is {volume.weight}. Skipping weight fade-in.");
        }


        // --- 2. Animasi Efek Post-Processing Utama ---
        StoreStartValues(); // Simpan nilai awal untuk efek utama

        float effectsTimer = 0f;
        while (effectsTimer < mainEffectsDuration)
        {
            effectsTimer += Time.deltaTime;
            float t = Mathf.Clamp01(effectsTimer / mainEffectsDuration);
            float smoothT = Mathf.SmoothStep(0, 1, t);

            ApplyEffectValues(smoothT);
            yield return null;
        }
        ApplyEffectValues(1f); // Pastikan nilai target efek utama tercapai


        // --- 3. Opsional: Fade Volume Weight OUT ---
        if (initialWeightWasZero && volume != null)
        {
            Debug.Log("Fading volume weight OUT.");
            float weightTimer = 0f;
            // float startWeightValue = volume.weight; // Seharusnya 1
            while (weightTimer < volumeWeightFadeDuration)
            {
                weightTimer += Time.deltaTime;
                // Lerp dari 1 ke 0 untuk volume.weight
                volume.weight = Mathf.Lerp(1f, 0f, Mathf.Clamp01(weightTimer / volumeWeightFadeDuration));
                yield return null;
            }
            volume.weight = 0f; // Pastikan mencapai 0
        }

        onComplete?.Invoke();
        activeTransitionCoroutine = null;
    }

    private void StoreStartValues()
    {
        if (lensDistortion != null && animateLensDistortion)
        {
            startIntensity = lensDistortion.intensity.value;
            startCenterX = lensDistortion.centerX.value;
            startScale = lensDistortion.scale.value;
        }

        if (bloom != null && animateBloom)
        {
            startBloomIntensity = bloom.intensity.value;
            startBloomThreshold = bloom.threshold.value;
            if (bloom.color.overrideState)
                startBloomColor = bloom.color.value;
        }

        if (autoExposure != null && animateAutoExposure)
        {
            startMinExposure = autoExposure.minLuminance.value;
        }
    }

    private void ApplyEffectValues(float t)
    {
        if (lensDistortion != null && animateLensDistortion)
        {
            lensDistortion.intensity.value = Mathf.Lerp(startIntensity, targetIntensity, t);
            lensDistortion.centerX.value = Mathf.Lerp(startCenterX, targetCenterX, t);
            lensDistortion.scale.value = Mathf.Lerp(startScale, targetScale, t);
        }

        if (bloom != null && animateBloom)
        {
            bloom.intensity.value = Mathf.Lerp(startBloomIntensity, targetBloomIntensity, t);
            bloom.threshold.value = Mathf.Lerp(startBloomThreshold, targetBloomThreshold, t);
            bloom.color.value = Color.Lerp(startBloomColor, targetBloomColor, t);
        }

        if (autoExposure != null && animateAutoExposure)
        {
            autoExposure.minLuminance.value = Mathf.Lerp(startMinExposure, targetMinExposure, t);
        }
    }

    public void StopAndResetTransition()
    {
        if (activeTransitionCoroutine != null)
        {
            StopCoroutine(activeTransitionCoroutine);
            activeTransitionCoroutine = null;
            Debug.Log("Post-process transition stopped.");
            // Jika Anda ingin mereset weight ke 0 jika awalnya 0:
            // if (volume != null && Mathf.Approximately(volume.weight, 1f)) // Asumsi ini dipanggil saat weight mungkin 1
            // {
            // // Cek apakah perlu di-reset ke 0 (mungkin perlu flag tambahan jika kondisi awal kompleks)
            // // Untuk kesederhanaan, Anda bisa panggil coroutine fade out singkat di sini jika perlu
            // // StartCoroutine(PerformVolumeWeightFade(0f, volumeWeightFadeDuration / 2)); // Fade out cepat
            // }
        }
    }

    // Helper coroutine jika Anda perlu fade weight secara terpisah
    // private IEnumerator PerformVolumeWeightFade(float targetWeight, float fadeDuration)
    // {
    // if (volume == null) yield break;
    // float weightTimer = 0f;
    // float startWeight = volume.weight;
    // while (weightTimer < fadeDuration)
    // {
    // weightTimer += Time.deltaTime;
    // volume.weight = Mathf.Lerp(startWeight, targetWeight, Mathf.Clamp01(weightTimer / fadeDuration));
    // yield return null;
    // }
    // volume.weight = targetWeight;
    // }
}
