using UnityEngine;

public class LEDController_SelectSong : MonoBehaviour
{
    public SerialController serialController;

    void Start()
    {
        serialController.SendSerialMessage("START_FLAME");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))  // GANTI tombol sesuai keinginanmu
        {
            serialController.SendSerialMessage("NEXT_COLOR");
        }
    }
}
