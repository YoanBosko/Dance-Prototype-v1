using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "New BuffCard Components", menuName = "BuffCard/BuffCard Components")]

public class BuffCardComponents : ScriptableObject
{

    [Header("Image")]
    public Sprite cardImageBG;
    public Sprite cardMoreDescriptionImage;


    [Header("Text")]
    public string cardTitleText;
    public string cardDescriptionText;
    public string cardMoreDescriptionText;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
