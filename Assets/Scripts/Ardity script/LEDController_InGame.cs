using UnityEngine;

public class LEDController_InGame : MonoBehaviour
{
    public SerialController serialController; // Ardity component
    public RenderTexture renderTexture;       // Assign RenderTexture directly from Inspector

    private Texture2D tempTexture;
    private Color32 lastSentColor = new Color32(0, 0, 0, 255);

    void Start()
    {
        if (renderTexture != null)
        {
            tempTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);

            InvokeRepeating(nameof(SendDominantColor), 0.1f, 0.1f); // Update every 100ms
            Invoke(nameof(SendStartSignal), 0.3f);                  // Trigger STAR animation
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
        RenderTexture.active = renderTexture;
        tempTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tempTexture.Apply();
        RenderTexture.active = null;

        Color32 avgColor = GetAverageColor(tempTexture);

        if (!avgColor.Equals(lastSentColor))
        {
            lastSentColor = avgColor;
            string msg = $"COLOR_{avgColor.r}_{avgColor.g}_{avgColor.b}";
            serialController.SendSerialMessage(msg);
        }
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

        byte avgR = (byte)Mathf.Clamp(r / count * 255f, 0, 255);
        byte avgG = (byte)Mathf.Clamp(g / count * 255f, 0, 255);
        byte avgB = (byte)Mathf.Clamp(b / count * 255f, 0, 255);

        return new Color32(avgR, avgG, avgB, 255);
    }
}
