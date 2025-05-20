using UnityEngine;

[System.Serializable]
public class GameData
{
    public int score;
    public int hiScore;
    public int[] boardFlat;
    public int[] undoBoardFlat;

    // when new game -> high score = 0;
    public GameData()
    {
        this.score = 0;
        this.hiScore = 0;
        this.boardFlat = null;
        this.undoBoardFlat = null;
    }

}
