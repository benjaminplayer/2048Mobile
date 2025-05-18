using UnityEngine;

[System.Serializable]
public class GameData
{
    public int score;

    // when new game -> high score = 0;
    public GameData()
    {
        this.score = 0;
    }


}
