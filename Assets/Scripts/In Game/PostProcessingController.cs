using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;
using UnityEngine.UI; // Tambahkan ini di atas

public class PostProcessingController : MonoBehaviour
{
    public static PostProcessingController Instance;

    public PostProcessVolume volume;
    private ChromaticAberration chromaticAberration;
    private DepthOfField depthOfField;
    private Vignette vignette;
    public AudioSource heartbeatAudio;
    public RawImage backgroundImage; // Drag RawImage dari Background ke sini

    private bool isHeartbeatPlaying = false;
    private Coroutine heartbeatFlashCoroutine;

    private void Awake()
    {
        Instance = this;

        // Mengakses efek dari volume
        volume.profile.TryGetSettings(out chromaticAberration);
        volume.profile.TryGetSettings(out depthOfField);
        volume.profile.TryGetSettings(out vignette);
    }

    private Color HexToColor(string hex)
    {   
        hex = hex.Replace("#", "");

        if (hex.Length != 6)
            throw new System.Exception("Invalid hex color");

        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        return new Color32(r, g, b, 255);
    }

    public void TriggerEffect()
    {
        StopAllCoroutines();
        StartCoroutine(EffectRoutine());
    }

    public void UpdateLowHealthEffect(float health)
    {
        if (vignette == null || depthOfField == null) return;

        if (health < 250)
        {
            float t = Mathf.InverseLerp(250f, 0f, health);

            // Vignette
            vignette.intensity.value = Mathf.Lerp(0.25f, 1f, t);
            vignette.color.value = Color.red;
            vignette.smoothness.value = 0.7f;

            // Blur (DoF)
            depthOfField.aperture.value = Mathf.Lerp(10f, 30f, t);
            depthOfField.focalLength.value = Mathf.Lerp(50f, 75f, t);

            // Heartbeat Volume (semakin rendah HP, semakin besar volume)
            if (heartbeatAudio != null)
            {
                heartbeatAudio.volume = Mathf.Lerp(0f, 3f, 1f - t); // volume naik saat HP turun
            }

            // Play heartbeat if not already playing
            if (!isHeartbeatPlaying && heartbeatAudio != null)
            {
                heartbeatAudio.loop = true; // biar terus muter
                heartbeatAudio.Play();
                isHeartbeatPlaying = true;

                if (heartbeatFlashCoroutine != null)
                    StopCoroutine(heartbeatFlashCoroutine);
                heartbeatFlashCoroutine = StartCoroutine(HeartbeatBackgroundFlash(60f)); // Sesuaikan BPM
            }

        }
        else
        {
            vignette.intensity.value = 0f;
            depthOfField.aperture.value = 10f;
            depthOfField.focalLength.value = 50f;

            if (heartbeatAudio != null)
            {
                heartbeatAudio.volume = 0f;
            }

            // Stop heartbeat if it's playing
            if (isHeartbeatPlaying && heartbeatAudio != null)
            {
                heartbeatAudio.Stop();
                isHeartbeatPlaying = false;
            }

            if (heartbeatFlashCoroutine != null)
            {
                StopCoroutine(heartbeatFlashCoroutine);
                heartbeatFlashCoroutine = null;
            }
            if (backgroundImage != null)
            {
                backgroundImage.color = Color.white; // Kembalikan ke putih
            }

        }
    }

    private IEnumerator HeartbeatBackgroundFlash(float bpm)
    {
        float interval = 60f / bpm / 2f; // Waktu setengah detak (merah ke putih)
        Color customRed = HexToColor("FF7979");
        
        while (true)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = customRed;
                yield return new WaitForSeconds(interval);
                backgroundImage.color = Color.white;
                yield return new WaitForSeconds(interval);
            }
            else
            {
                yield break;
            }
        }
    }

    private IEnumerator EffectRoutine()
    {
        float duration = 0.1f;
        float t = 0;

        while (t < duration / 2f)
        {
            t += Time.deltaTime;
            float normalized = t / (duration / 2f);

            if (chromaticAberration != null)
                chromaticAberration.intensity.value = Mathf.Lerp(0f, 1f, normalized);
            if (depthOfField != null)
                depthOfField.aperture.value = Mathf.Lerp(24f, 0.5f, normalized);

            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        t = 0;
        while (t < duration / 2f)
        {
            t += Time.deltaTime;
            float normalized = t / (duration / 2f);

            if (chromaticAberration != null)
                chromaticAberration.intensity.value = Mathf.Lerp(1f, 0f, normalized);
            if (depthOfField != null)
                depthOfField.aperture.value = Mathf.Lerp(0.5f, 24f, normalized);

            yield return null;
        }
    }
}
