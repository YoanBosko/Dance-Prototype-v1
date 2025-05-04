using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class InputHandler : MonoBehaviour
{
    // Referensi ke InputField (drag dari Inspector)
    public InputField inputField1;
    public InputField inputField2;
    public InputField inputField3;
    
    void Start()
    {
        // Anda bisa menambahkan listener untuk event perubahan
        inputField1.onValueChanged.AddListener(OnInput1Changed);
    }
    
    void OnInput1Changed(string newValue)
    {
        Debug.Log("Input 1 berubah: " + newValue);
    }
    
    public void GetInputValues()
    {
        string value1 = inputField1.text;
        string value2 = inputField2.text;
        string value3 = inputField3.text;
        
        Debug.Log("Input 1: " + value1);
        Debug.Log("Input 2: " + value2);
        Debug.Log("Input 3: " + value3);
    }
}