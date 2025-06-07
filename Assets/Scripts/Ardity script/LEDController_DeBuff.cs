using UnityEngine;

public class LEDController_DeBuff : MonoBehaviour
{
    public SerialController serialController;

    void Start()
    {
        serialController.SendSerialMessage("START_MIDDLEFLAME3");
    }

    void Update()
    {
    }
}
