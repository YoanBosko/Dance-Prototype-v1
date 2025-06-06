using UnityEngine;
using System.Collections.Generic; // Diperlukan untuk menggunakan List

// Kelas kecil ini digunakan untuk mengelompokkan KeyCode dengan AudioClip-nya.
// [System.Serializable] membuatnya bisa dilihat dan diatur di Inspector Unity.
[System.Serializable]
public class KeySoundPair
{
    [Tooltip("Tombol keyboard yang akan memicu suara.")]
    public KeyCode key;

    [Tooltip("Klip suara yang akan dimainkan saat tombol ditekan.")]
    public AudioClip soundClip;
}

public class KeySoundEffectPlayer : MonoBehaviour
{
    [Header("Audio Source")]
    [Tooltip("Satu-satunya AudioSource yang akan digunakan untuk memutar semua sound effect.")]
    public AudioSource audioSource;

    [Header("Key to Sound Mappings")]
    [Tooltip("Daftar pasangan tombol dan klip suara. Atur di sini.")]
    public List<KeySoundPair> keySoundPairs;

    void Start()
    {
        // Validasi untuk memastikan AudioSource sudah di-assign
        if (audioSource == null)
        {
            Debug.LogError("AudioSource belum di-assign di KeySoundEffectPlayer! Coba tambahkan AudioSource ke GameObject ini atau assign secara manual.", this);
            // Coba cari AudioSource di GameObject yang sama sebagai fallback
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                // Jika masih tidak ada, nonaktifkan skrip ini agar tidak menimbulkan error di Update()
                Debug.LogError("Tidak dapat menemukan AudioSource. Skrip KeySoundEffectPlayer akan dinonaktifkan.", this);
                enabled = false;
            }
        }
    }

    void Update()
    {
        // Loop melalui setiap pasangan Key-Sound yang telah Anda atur
        foreach (KeySoundPair pair in keySoundPairs)
        {
            // Cek apakah tombol untuk pasangan ini baru saja ditekan
            if (Input.GetKeyDown(pair.key))
            {
                // Panggil fungsi untuk memainkan suara
                PlaySound(pair.soundClip);

                // Hentikan loop setelah menemukan dan memainkan suara pertama.
                // Ini mencegah pemutaran beberapa suara jika ada tombol yang sama di daftar.
                break;
            }
        }
    }

    /// <summary>
    /// Memainkan klip suara yang diberikan melalui AudioSource yang ditentukan.
    /// Akan menghentikan suara yang sedang diputar sebelumnya.
    /// </summary>
    /// <param name="clipToPlay">Klip suara yang akan dimainkan.</param>
    private void PlaySound(AudioClip clipToPlay)
    {
        // Pastikan AudioSource dan AudioClip tidak null
        if (audioSource != null && clipToPlay != null)
        {
            // Mengganti klip di AudioSource dengan yang baru
            audioSource.clip = clipToPlay;

            // Memainkan klip. Ini akan otomatis menghentikan klip sebelumnya
            // yang sedang diputar di AudioSource yang sama.
            audioSource.Play();
        }
        else
        {
            if (audioSource == null) Debug.LogWarning("Tidak bisa memainkan suara karena AudioSource null.", this);
            if (clipToPlay == null) Debug.LogWarning("Tidak bisa memainkan suara karena AudioClip null.", this);
        }
    }
}
