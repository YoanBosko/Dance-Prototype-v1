using UnityEngine;

public class UIOrderManager : MonoBehaviour
{
    [SerializeField] private GameObject logoPanel;

    void Start()
    {
        // Logo akan dipindah ke urutan paling atas dalam hierarchy UI
        if (logoPanel != null)
        {
            logoPanel.transform.SetAsLastSibling();
        }
        else
        {
            Debug.LogWarning("Logo Panel belum di-assign di inspector.");
        }
    }
}