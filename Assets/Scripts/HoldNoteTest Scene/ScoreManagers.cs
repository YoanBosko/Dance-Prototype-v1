using UnityEngine;

public class ScoreManagers : MonoBehaviour {
    public static ScoreManagers instance;
    private int score = 0;
    
    void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }
    
    public void AddScore(int points) {
        score += points;
        Debug.Log("Score: " + score);
    }
    
    public int GetScore() {
        return score;
    }
}
