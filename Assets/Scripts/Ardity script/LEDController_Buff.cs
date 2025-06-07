using UnityEngine;

public class LEDController_Buff : MonoBehaviour
{
    public SerialController serialController;

    void Start()
    {
        serialController.SendSerialMessage("START_MIDDLEFLAME");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.J))  // GANTI tombol sesuai keinginanmu
        {
            serialController.SendSerialMessage("NEXT_COLOR2");
        }
    }
}
