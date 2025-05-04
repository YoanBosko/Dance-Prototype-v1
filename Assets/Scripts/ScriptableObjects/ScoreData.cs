// ScoreData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "ScoreData", menuName = "GameData/ScoreData")]
public class ScoreData : ScriptableObject
{
    public int score;
    public float accuracy;
    public int perfectHits;
    public int goodHits;
    public int badHits;
    public int missHits;

    public void ResetScore()
    {
        score = 0;
        accuracy = 0;
        perfectHits = 0;
        goodHits = 0;
        badHits = 0;
        missHits = 0;
    }
}
