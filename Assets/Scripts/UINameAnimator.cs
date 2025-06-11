using UnityEngine;
using TMPro; // Gunakan ini jika Anda memakai TextMeshPro
// using UnityEngine.UI; // Gunakan ini jika Anda memakai UI Text biasa
using System.Collections;
using UnityEngine.UI;

public class UINameAnimator : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Komponen RectTransform dari teks nama pemain.")]
    public RectTransform nameTextTransform;
    [Tooltip("Komponen Text dari nama pemain untuk mengubah isinya.")]
    public Text nameTextComponent; // Ganti ke Text jika memakai UI Text biasa

    [Header("Animation Settings")]
    [Tooltip("Durasi pergerakan teks dari samping ke tengah (dan sebaliknya).")]
    public float moveDuration = 0.5f;
    [Tooltip("Durasi teks diam di tengah layar.")]
    public float holdDuration = 1.0f;
    [Tooltip("Tipe easing untuk pergerakan yang lebih halus.")]
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine activeAnimationCoroutine;

    void Start()
    {
        // Pastikan teks tidak terlihat saat scene dimulai
        // Di dalam UINameAnimator.cs -> Start()
        if (nameTextComponent != null)
        {
            nameTextComponent.enabled = false;
        }
    }

    /// <summary>
    /// Memulai animasi nama pemain.
    /// </summary>
    /// <param name="playerName">Nama yang akan ditampilkan.</param>
    /// <param name="onComplete">Aksi yang akan dipanggil setelah animasi selesai.</param>
    public void PlayNameAnimation(string playerName, System.Action onComplete = null)
    {
        if (nameTextTransform == null || nameTextComponent == null)
        {
            Debug.LogError("Komponen teks belum di-assign di UINameAnimator!", this);
            onComplete?.Invoke(); // Langsung panggil onComplete jika komponen tidak ada
            return;
        }

        if (activeAnimationCoroutine != null)
        {
            StopCoroutine(activeAnimationCoroutine);
        }
        activeAnimationCoroutine = StartCoroutine(AnimateNameCoroutine(playerName, onComplete));
    }

    private IEnumerator AnimateNameCoroutine(string playerName, System.Action onComplete)
    {
        // --- Persiapan ---
        nameTextComponent.text = playerName;
        // nameTextTransform.gameObject.SetActive(true);
        nameTextComponent.enabled = true;

        // Dapatkan lebar Canvas untuk menentukan posisi di luar layar
        RectTransform canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        float halfScreenWidth = canvasRect.rect.width / 2;
        float textWidth = nameTextTransform.rect.width / 2;
        
        Vector2 startPos = new Vector2(halfScreenWidth + textWidth, nameTextTransform.anchoredPosition.y);
        Vector2 middlePos = new Vector2(0, nameTextTransform.anchoredPosition.y);
        Vector2 endPos = new Vector2(-halfScreenWidth - textWidth, nameTextTransform.anchoredPosition.y);

        nameTextTransform.anchoredPosition = startPos;

        // --- Animasi Masuk (Kanan ke Tengah) ---
        float timer = 0f;
        while (timer < moveDuration)
        {
            timer += Time.deltaTime;
            float t = moveCurve.Evaluate(timer / moveDuration);
            nameTextTransform.anchoredPosition = Vector2.Lerp(startPos, middlePos, t);
            yield return null;
        }
        nameTextTransform.anchoredPosition = middlePos;

        // --- Diam di Tengah ---
        yield return new WaitForSeconds(holdDuration);

        // --- Animasi Keluar (Tengah ke Kiri) ---
        timer = 0f;
        while (timer < moveDuration)
        {
            timer += Time.deltaTime;
            float t = moveCurve.Evaluate(timer / moveDuration);
            nameTextTransform.anchoredPosition = Vector2.Lerp(middlePos, endPos, t);
            yield return null;
        }
        nameTextTransform.anchoredPosition = endPos;

        // --- Selesai ---
        nameTextTransform.gameObject.SetActive(false);
        activeAnimationCoroutine = null;

        // Panggil callback setelah animasi selesai
        onComplete?.Invoke();
    }
}
