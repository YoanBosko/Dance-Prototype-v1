using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using UnityEngine.UI;

public class BuffLoader : MonoBehaviour
{
    public GameObject newBuffPrefab;
    public BuffCardComponents buffCardComponents;

    // [Header("Components")]

    // public Text cardTitleText;
    // public Text cardDescriptionText;
    // public GameObject cardMoreDescription;
    // public Image cardMoreDescriptionImage;
    // public Text cardMoreDescriptionText;

    public void AddBuffToList()
    {
        GameManager.Instance.AddBuff(newBuffPrefab);
    }
    void Start()
    {
        Image imageBG = GetComponent<Image>();
        imageBG.sprite = buffCardComponents.cardImageBG;
        Text[] texts = GetComponentsInChildren<Text>();
        foreach (Text text in texts)
        {
            text.gameObject.SetActive(false);
        }
        // cardTitleText.text = buffCardComponents.cardTitleText;
        // cardDescriptionText.text = buffCardComponents.cardDescriptionText;
        // cardMoreDescriptionText.text = buffCardComponents.cardMoreDescriptionText;
        // cardMoreDescriptionImage.sprite = buffCardComponents.cardMoreDescriptionImage;
    }
    public void ActivateMoreDescription()
    {
        // cardMoreDescription.SetActive(true);
        Debug.Log("deskripsi aktif");
    }
    public void DeactivateMoreDescription()
    {
        // cardMoreDescription.SetActive(false);
    }
}