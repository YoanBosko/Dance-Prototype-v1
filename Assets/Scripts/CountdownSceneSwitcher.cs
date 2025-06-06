using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events; // Diperlukan untuk UnityEvent
using TMPro;              // Diperlukan untuk TextMeshPro
using System.Collections; // Diperlukan untuk Coroutine
using UnityEngine.UI;

public class CountdownSceneSwitcher : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("Waktu hitung mundur dalam detik.")]
    public float countdownTime = 10.0f;

    //[Tooltip("Nama scene yang akan dimuat setelah waktu habis.")]
    //public string nextSceneName;

    [Header("UI Display (Opsional)")]
    [Tooltip("Komponen TextMeshPro untuk menampilkan waktu. Boleh tidak diisi.")]
    public Text countdownText;

    [Header("Events")]
    [Tooltip("Event yang akan dipanggil tepat sebelum berpindah scene.")]
    public UnityEvent onTimerEnd;

    void Start()
    {
        // Memulai coroutine hitung mundur secara otomatis
        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        float currentTime = countdownTime;

        while (currentTime > 0)
        {
            // Update UI Text jika sudah di-assign
            if (countdownText != null)
            {
                // Menampilkan angka bulat ke atas dari waktu yang tersisa
                countdownText.text = Mathf.CeilToInt(currentTime).ToString();
            }

            // Kurangi waktu dengan waktu yang telah berlalu sejak frame terakhir
            currentTime -= Time.deltaTime;

            // Tunggu hingga frame berikutnya
            yield return null;
        }

        // --- Waktu Habis ---

        // Pastikan teks menampilkan 0 di akhir
        if (countdownText != null)
        {
            countdownText.text = "0";
        }

        Debug.Log("Timer selesai!");

        // Panggil event yang telah diatur di Inspector
        // Tanda '?' adalah null-conditional operator, memastikan event hanya dipanggil jika tidak null.
        onTimerEnd?.Invoke();

        // Pindah ke scene berikutnya
        //if (!string.IsNullOrEmpty(nextSceneName))
        //{
        //    SceneManager.LoadScene(nextSceneName);
        //}
        //else
        //{
        //    Debug.LogWarning("Nama scene berikutnya (Next Scene Name) belum diatur. Tidak ada scene yang dimuat.", this);
        //}
    }
}
