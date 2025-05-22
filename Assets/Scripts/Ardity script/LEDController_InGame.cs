using UnityEngine;

public class LEDController_InGame : MonoBehaviour
{
    public SerialController serialController;
    public RenderTexture renderTexture;

    private Texture2D tempTexture;
    private Color32 lastSentColor = new Color32(0, 0, 0, 255);
    private ScoreManager scoreManager;

    private float criticalHP = 250f;  // darah kritis = 250
    private float maxHP = 1000f;      // darah maksimum

    void Start()
    {
        scoreManager = ScoreManager.Instance;

        if (renderTexture != null)
        {
            tempTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
            InvokeRepeating(nameof(SendDominantColor), 0.1f, 0.1f);
            Invoke(nameof(SendStartSignal), 0.3f);
        }
        else
        {
            Debug.LogError("RenderTexture is not assigned!");
        }
    }

    void SendStartSignal()
    {
        serialController.SendSerialMessage("START_STAR");
    }

    void SendDominantColor()
    {
        if (scoreManager == null || renderTexture == null) return;

        // Ambil warna dari layar
        RenderTexture.active = renderTexture;
        tempTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tempTexture.Apply();
        RenderTexture.active = null;

        Color32 avgColor = GetAverageColor(tempTexture);

        float currentHP = scoreManager.slider.value;

        if (currentHP < criticalHP)
        {
            // Blend ke merah saat darah rendah, proporsional
            float blendFactor = Mathf.Clamp01((criticalHP - currentHP) / criticalHP);
            Color32 red = new Color32(255, 0, 0, 255);
            avgColor = Color32.Lerp(avgColor, red, blendFactor);
        }

        // Hanya kirim jika warna berubah
        if (!avgColor.Equals(lastSentColor))
        {
            lastSentColor = avgColor;
            string msg = $"COLOR_{avgColor.r}_{avgColor.g}_{avgColor.b}";
            serialController.SendSerialMessage(msg);
        }
    }

    public void TriggerComboEffect(int combo)
    {
        serialController.SendSerialMessage("START_COMBO");
    }


    Color32 GetAverageColor(Texture2D tex)
    {
        Color[] pixels = tex.GetPixels();
        float r = 0, g = 0, b = 0;
        int count = pixels.Length;

        foreach (Color c in pixels)
        {
            r += c.r;
            g += c.g;
            b += c.b;
        }

        byte avgR = (byte)(Mathf.Clamp01(r / count) * 255);
        byte avgG = (byte)(Mathf.Clamp01(g / count) * 255);
        byte avgB = (byte)(Mathf.Clamp01(b / count) * 255);

        return new Color32(avgR, avgG, avgB, 255);
    }
}
    