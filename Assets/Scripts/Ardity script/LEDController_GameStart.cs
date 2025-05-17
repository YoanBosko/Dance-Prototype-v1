using UnityEngine;

public class LEDController_GameStart : MonoBehaviour
{
    public SerialController serialController; // Reference to Ardity SerialController

    void Start()
    {
        // Wait a moment to ensure the serial is ready (optional but safe)
        Invoke(nameof(SendStartSignal), 0.5f);
    }

    void SendStartSignal()
    {
        serialController.SendSerialMessage("START_RAINBOW");
    }
}
