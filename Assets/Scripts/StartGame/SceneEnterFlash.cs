using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections; // Diperlukan untuk Coroutine

public class SceneEnterFlash : MonoBehaviour
{
    [Header("Post Process Settings")]
    public PostProcessVolume volume;

    [Header("Transition Settings")]
    [Tooltip("Durasi animasi flash saat scene dimuat.")]
    [SerializeField] private float transitionDuration = 1f;

    // Nilai Awal dan Akhir sekarang bisa diatur dari Inspector agar lebih modular
    [Header("Auto Exposure (FROM -> TO)")]
    [SerializeField] private float startEV = -5f;
    [SerializeField] private float endEV = 1f;

    [Header("Bloom (FROM -> TO)")]
    [SerializeField] private float startBloomIntensity = 0.8f;
    [SerializeField] private float endBloomIntensity = 0f;

    // Komponen Post-Processing (privat)
    private AutoExposure autoExposure;
    private Bloom bloom;

    void Start()
    {
        // Pastikan volume ada sebelum melanjutkan
        if (volume == null)
        {
            Debug.LogError("PostProcessVolume belum di-assign di SceneEnterFlash.", this);
            return;
        }

        // Coba dapatkan komponen efek dari profile
        if (volume.profile.TryGetSettings(out autoExposure) &&
            volume.profile.TryGetSettings(out bloom))
        {
            // Mulai transisi menggunakan Coroutine
            StartCoroutine(TransitionFlashCoroutine());
        }
        else
        {
            Debug.LogError("Gagal mendapatkan AutoExposure atau Bloom dari PostProcessProfile.", this);
        }
    }

    private IEnumerator TransitionFlashCoroutine()
    {
        // --- Langkah 1: Persiapan Transisi ---

        // Atur nilai awal untuk efek SEBELUM volume diaktifkan
        autoExposure.minLuminance.value = startEV;
        autoExposure.maxLuminance.value = startEV;
        bloom.intensity.value = startBloomIntensity;

        // Aktifkan PostProcessVolume untuk memulai efek
        volume.enabled = true;

        // --- Langkah 2: Animasi Transisi ---

        float timer = 0f;
        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            // Hitung progres transisi (0 sampai 1)
            float t = Mathf.Clamp01(timer / transitionDuration);
            // Gunakan SmoothStep untuk interpolasi yang lebih halus
            float smoothT = Mathf.SmoothStep(0, 1, t);

            // Terapkan nilai efek secara bertahap menggunakan Lerp
            if (autoExposure != null)
            {
                autoExposure.minLuminance.value = Mathf.Lerp(startEV, endEV, smoothT);
                autoExposure.maxLuminance.value = Mathf.Lerp(startEV, endEV, smoothT);
            }

            if (bloom != null)
            {
                bloom.intensity.value = Mathf.Lerp(startBloomIntensity, endBloomIntensity, smoothT);
            }

            // Tunggu frame berikutnya sebelum melanjutkan loop
            yield return null;
        }

        // --- Langkah 3: Finalisasi ---

        // Pastikan nilai akhir tercapai dengan akurat
        autoExposure.minLuminance.value = endEV;
        autoExposure.maxLuminance.value = endEV;
        bloom.intensity.value = endBloomIntensity;

        // Nonaktifkan volume setelah transisi selesai agar tidak mempengaruhi scene
        volume.enabled = false;

        // Debug.Log("Scene enter flash transition complete.");
    }
}
