using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;

/// <summary>
/// Mengontrol efek flash post-processing saat scene pertama kali dimuat.
/// Versi ini menganimasikan parameter efek (Auto Exposure, Bloom) secara langsung.
/// </summary>
public class SceneEnterFlash : MonoBehaviour
{
    [Header("Referensi Komponen")]
    [Tooltip("Post Process Volume yang akan dikontrol. Pastikan sudah memiliki profil dengan Auto Exposure dan Bloom.")]
    [SerializeField] private PostProcessVolume volume;

    [Header("Pengaturan Transisi")]
    [Tooltip("Durasi animasi flash dalam detik.")]
    [SerializeField] private float transitionDuration = 1.0f;

    // Nilai awal dan akhir untuk setiap efek, dianimasikan secara linear.
    [Header("Auto Exposure (Dari -> Ke)")]
    [SerializeField] private float startEV = -5f;
    [SerializeField] private float endEV = 1f;

    [Header("Bloom (Dari -> Ke)")]
    [SerializeField] private float startBloomIntensity = 0.8f;
    [SerializeField] private float endBloomIntensity = 0f;

    // Variabel privat untuk menyimpan referensi ke efek post-processing.
    private AutoExposure autoExposure;
    private Bloom bloom;

    private void Start()
    {
        // Validasi: Pastikan PostProcessVolume sudah di-assign.
        if (volume == null)
        {
            Debug.LogError("PostProcessVolume belum di-assign pada script SceneEnterFlash.", this);
            return; // Hentikan eksekusi jika volume tidak ada.
        }

        // Coba dapatkan setting AutoExposure dan Bloom dari profile volume.
        bool profileValid = volume.profile.TryGetSettings(out autoExposure) &&
                            volume.profile.TryGetSettings(out bloom);

        if (profileValid)
        {
            // Jika berhasil, mulai coroutine untuk transisi.
            StartCoroutine(AnimateEffectParameters());
        }
        else
        {
            // Tampilkan error jika salah satu atau kedua efek tidak ditemukan di profile.
            Debug.LogError("Gagal mendapatkan AutoExposure atau Bloom dari PostProcessProfile. Pastikan kedua efek sudah ditambahkan ke profile.", this);
        }
    }

    /// <summary>
    /// Coroutine yang menganimasikan parameter efek secara langsung dari nilai awal ke akhir.
    /// </summary>
    private IEnumerator AnimateEffectParameters()
    {
        // --- LANGKAH 1: Inisialisasi ---
        // Pastikan volume aktif sepenuhnya agar efeknya terlihat.
        // Weight tidak akan dianimasikan, hanya parameternya.
        volume.weight = 1f;

        // --- LANGKAH 2: Animasi Linear Berbasis Parameter ---
        float elapsedTime = 0f;
        while (elapsedTime < transitionDuration)
        {
            // Hitung progres transisi secara linear (nilai dari 0 ke 1).
            float progress = Mathf.Clamp01(elapsedTime / transitionDuration);

            // Animasikan setiap parameter efek secara individual.
            // Ini akan membuat slider di Inspector bergerak sesuai transisi.
            autoExposure.minLuminance.value = Mathf.Lerp(startEV, endEV, progress);
            autoExposure.maxLuminance.value = Mathf.Lerp(startEV, endEV, progress);
            bloom.intensity.value = Mathf.Lerp(startBloomIntensity, endBloomIntensity, progress);
            
            /*
            // --- PROSES SKRIP SEBELUMNYA (DIJADIKAN KOMENTAR) ---
            // Logika ini menganimasikan weight dari volume, bukan parameternya.
            volume.weight = Mathf.Lerp(1f, 0f, progress);
            */

            // Tambahkan waktu yang telah berlalu sejak frame terakhir.
            elapsedTime += Time.deltaTime;

            // Tunggu hingga frame berikutnya untuk melanjutkan loop.
            yield return null;
        }

        // --- LANGKAH 3: Finalisasi ---
        // Pastikan nilai akhir tercapai dengan akurat.
        autoExposure.minLuminance.value = endEV;
        autoExposure.maxLuminance.value = endEV;
        bloom.intensity.value = endBloomIntensity;
        
        // Nonaktifkan volume setelah transisi selesai.
        volume.weight = 0f;

        // Opsional: uncomment baris di bawah untuk notifikasi saat transisi selesai.
        // Debug.Log("Transisi flash saat masuk scene telah selesai.");
    }
}
