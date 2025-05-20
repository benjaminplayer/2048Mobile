using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Windows;

public class GameLogic : MonoBehaviour, IDataPresistance
{

    [SerializeField] private GameObject overlay;
    [SerializeField] private TextMeshProUGUI tmpOverlayText;
    private GameObject[,] objectboard = new GameObject[4, 4];
    private int[,] board = new int[4, 4];
    private int[,] undoBoard = new int[4, 4];
    [SerializeField] GameObject[] rows = new GameObject[16];
    [SerializeField] Sprite[] sprites = new Sprite[11];
    [SerializeField] private int score = 0, undoScore = 0;
    [SerializeField] private TextMeshProUGUI scoreTmp, hiScoreTMP;
    [SerializeField] private int affectedRows = 0, ar, hiScore;
    [SerializeField] private bool reached2048 = false, checking = false, lost = false, canGenerate = true;
    private Color overlayColor;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        tmpOverlayText.enabled = false;
        overlayColor = overlay.GetComponent<SpriteRenderer>().color;
        overlay.GetComponent<SpriteRenderer>().color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0);
        FillboardFromRows();
    }

    private void Start()
    {
        DataPresistanceManager.Instance.LoadGame();
        hiScoreTMP.text = "High Score: " + hiScore;

        if (board == null)
        {
            board = new int[4, 4];
            undoBoard = new int[4, 4];
            InitBoard();
        }

        else if (undoBoard == null)
        { 
            undoBoard = new int[4, 4];
            SetUndoBoard();
            LoadBoard();
        }
        else
        {
            LoadBoard();
        }

    }

    private void FillboardFromRows()
    {
        int index = 0;

        for (int y = 0; y < board.GetLength(0); y++)
        {
            for (int x = 0; x < board.GetLength(1); x++)
            {
                if (index < rows.Length)
                {
                    objectboard[y, x] = rows[index];
                    index++;
                }
                else
                {
                    // Optional: break or fill with null
                    objectboard[y, x] = null;
                }
            }
        }
    }

    private void InitBoard()
    {
        EmptySprites();
        #region DebugValues
        /*board[0, 0] = 32;
        board[0, 3] = 2;
        board[0, 2] = 4;
        board[0, 3] = 2;
        board[1, 0] = 2;
        board[1, 1] = 32;
        board[1, 2] = 8;
        board[1, 3] = 64;
        board[2, 0] = 8;
        board[2, 1] = 4;
        board[2, 2] = 32;
        board[2, 3] = 2;
        board[3, 0] = 2;
        board[3, 1] = 128;
        board[3, 2] = 2;
        board[3, 3] = 2;

        //board[0, 0] = 1024;
        // board[0, 1] = 1024;

        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                SetSprite(objectboard[i,j], board[i,j]);
                Debug.Log("object :" + objectboard[i, j]);
                Debug.Log("Sprite :" + board[i, j]);
            }    
           
        }*/
        #endregion

        int row, col, num;
        HashSet<string> set = new HashSet<string>();
        for (int i = 0; i < 2; i++)
        {
            row = UnityEngine.Random.Range(0, 4);
            col = UnityEngine.Random.Range(0, 4);

            if (!set.Contains(row + "" + col))
            {
                num = GenNumber();
                board[row, col] = num;
                SetSprite(objectboard[row, col], num);
                set.Add(row + "" + col);

            }
            else
            {
                i--;
            }
        }
        
    }

    private int GenNumber()
    {
        int number = UnityEngine.Random.Range(1, 101);
        if (number <= 10)
            return 4;
        else
            return 2;
    }

    private void SetSprite(GameObject tile, int val)
    {
        if (checking) return;
        SpriteRenderer sr = tile.transform.GetComponent<SpriteRenderer>();

        if (val == 0)
        {
            sr.sprite = null;
            return;
        }

        for (int i = 0, j = 2; i < sprites.Length; i++, j *= 2)
        {
            if (val == j)
            {
                sr.sprite = sprites[i];
                break;
            }
        }

    }

    #region Shift Logic
    private void ShiftLeft()
    {
        int[] shiftIdx = new int[4];
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                if (board[i, j] != 0)
                {
                    shiftIdx[i] = j;
                }
                else
                    break;
            }
        }

        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = shiftIdx[i]; j < board.GetLength(1); j++)
            {
                if (board[i, shiftIdx[i]] == 0)
                {
                    if (board[i, j] != 0 && j != shiftIdx[i])
                    {
                        board[i, shiftIdx[i]] = board[i, j];
                        SetSprite(objectboard[i, shiftIdx[i]], board[i, j]);
                        SetSprite(objectboard[i, j], 0);
                        board[i, j] = 0;
                        shiftIdx[i]++;
                        affectedRows++;
                    }
                }
                else if (board[i, shiftIdx[i]] != 0)
                {
                    if (j + 1 < 4 && board[i, j + 1] != 0)
                    {
                        board[i, shiftIdx[i] + 1] = board[i, j];
                        SetSprite(objectboard[i, shiftIdx[i] + 1], board[i, j]);
                        SetSprite(objectboard[i, j], 0);
                        board[i, j] = 0;
                        shiftIdx[i]++; // posodobi shift idx;
                        affectedRows++;

                    }
                }
            }
        }


    }

    private void ShiftRight()
    {
        int[] shiftIdx = new int[4];

        for (int i = 0; i < shiftIdx.Length; i++)
        {
            shiftIdx[i] = 3;
        }

        for (int i = 0; i < board.GetLength(0); i++)
        { // dobi zacetke shifta na desni strani
            for (int j = board.GetLength(1) - 1; j >= 0; j--)
            {
                if (board[i, j] != 0)
                {
                    shiftIdx[i] = j;
                }
                else // ce dobi prazno polje breaka iz loopa;
                    break;
            }
        }

        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = shiftIdx[i]; j >= 0; j--)
            {
                if (board[i, shiftIdx[i]] == 0)
                {
                    if (board[i, j] != 0 && j != shiftIdx[i])
                    {
                        board[i, shiftIdx[i]] = board[i, j];
                        SetSprite(objectboard[i,shiftIdx[i]], board[i,j]);
                        SetSprite(objectboard[i, j], 0);
                        board[i, j] = 0;
                        shiftIdx[i]--;
                        affectedRows++;
                    }
                }
                else if (board[i, shiftIdx[i]] != 0)
                {
                    if (j - 1 >= 0 && board[i, j - 1] != 0)
                    {
                        board[i, shiftIdx[i] - 1] = board[i, j];
                        SetSprite(objectboard[i, shiftIdx[i] -1], board[i, j]);
                        SetSprite(objectboard[i, j], 0);
                        board[i, j] = 0; 
                        shiftIdx[i]--;
                        affectedRows++;
                    }
                }
            }
        }

    }

    private void ShiftUp()
    {
        int[] shiftIdx = new int[4];
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                if (board[j, i] != 0)
                    shiftIdx[i] = j;
                else
                    break;
            }
        }

        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = shiftIdx[i] + 1; j < board.GetLength(1); j++)
            {
                if (board[shiftIdx[i], i] == 0)
                {
                    if (board[j, i] != 0)
                    {
                        board[shiftIdx[i], i] = board[j, i];
                        SetSprite(objectboard[shiftIdx[i], i], board[j, i]);
                        SetSprite(objectboard[j, i], 0);
                        board[j, i] = 0;
                        shiftIdx[i]++;
                        affectedRows++;
                    }
                }
                else if (board[j, i] != 0)
                {
                    board[shiftIdx[i] + 1, i] = board[j, i];
                    SetSprite(objectboard[shiftIdx[i] + 1, i], board[j, i]);
                    SetSprite(objectboard[j, i], 0);
                    board[j, i] = 0;
                    shiftIdx[i]++;
                    affectedRows++;
                }
            }

        }
    }

    private void ShiftDown()
    {
        int[] shiftIdx = new int[4];

        for (int i = 0; i < shiftIdx.Length; i++)
        {
            shiftIdx[i] = 3;
        }

        for (int i = 0; i < board.GetLength(0); i++)
        { // dobi zacetke shifta na desni strani
            for (int j = board.GetLength(1) - 1; j >= 0; j--)
            {
                if (board[j, i] != 0)
                {
                    shiftIdx[i] = j;
                }
                else // ce dobi prazno polje breaka iz loopa;
                    break;
            }
        }

        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = shiftIdx[i]; j >= 0; j--)
            {
                if (board[shiftIdx[i], i] == 0)
                {
                    if (board[j, i] != 0 && j != shiftIdx[i])
                    {
                        board[shiftIdx[i], i] = board[j, i];
                        SetSprite(objectboard[shiftIdx[i], i], board[j, i]);
                        SetSprite(objectboard[j, i], 0);
                        board[j, i] = 0;
                        shiftIdx[i]--;
                        affectedRows++;
                    }
                }
                else if (board[j, i] != 0 && j != shiftIdx[i])
                {
                    board[shiftIdx[i] - 1, i] = board[j, i];
                    SetSprite(objectboard[shiftIdx[i] - 1, i], board[j, i]);
                    SetSprite(objectboard[j, i], 0);
                    board[j, i] = 0;
                    shiftIdx[i]--;
                    affectedRows++;
                }
            }

        }

    }
    #endregion

    public void Shift(int dir) 
    {
        if(!checking)
            SetUndoBoard();

        switch (dir) 
        {
            case 0:
                ShiftUp();
                Merge(dir);
                ShiftUp();
                break;
            case 1:
                ShiftLeft();
                Merge(dir);
                ShiftLeft();
                break;
            case 2:
                ShiftDown();
                Merge(dir);
                ShiftDown();
                break;
            case 3:
                ShiftRight();
                Merge(dir);
                ShiftRight();
                break;
            default:
                throw new System.Exception("It appears I have made an oopsie");
        }

        if (reached2048)
        {
            //TODO: Create some sort of logic here :/
            OnWin();
            return;
        }

        if (!checking)
        {
            NewNumber();
            affectedRows = 0;
        }

        if (lost)
        {
            //Debug.Log("Lost :(");
            OnLoss();
        }

        //PrintBoard();
    }

    public void PrintBoard()
    {
        int rows = board.GetLength(0);
        int cols = board.GetLength(1);

        for (int y = 0; y < rows; y++)
        {
            string line = "";
            for (int x = 0; x < cols; x++)
            {
                line += board[y, x].ToString().PadLeft(4);
            }
            Debug.Log(line);
        }
    }

    private void Merge(int dir)
    {

        if (dir == 2)
        {
            for (int i = board.GetLength(0) - 1; i >= 0; i--)
            {
                for (int j = board.GetLength(1) - 1; j > 0; j--)
                {
                    if (board[j,i] == board[j - 1, i])
                    {
                        board[j - 1, i] = board[j, i] * 2;
                        if (!checking && board[j - 1, i] == 2048)
                            reached2048 = true;

                        UpdateScore(board[j, i]);
                        SetSprite(objectboard[j - 1, i], board[j, i] * 2);
                        SetSprite(objectboard[j,i], 0);
                        board[j, i] = 0;
                        j--;
                    }
                }
            }
        }

        if (dir == 1)
        {
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j ++)
                {
                    if (j+1 < board.GetLength(1) && board[i, j] != 0 && board[i, j] == board[i, j + 1])
                    {
                        board[i, j + 1] = board[i, j] * 2;
                        if (!checking &&  board[i, j + 1] == 2048)
                            reached2048 = true;
                        UpdateScore(board[i, j]);
                        SetSprite(objectboard[i, j+1], board[i, j] * 2);
                        SetSprite(objectboard[i, j], 0);
                        board[i, j] = 0;
                        j++;
                    }
                }
            }
        }

        if (dir == 0)
        {
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1) - 1; j++)
                {
                    if (board[j, i] == board[j + 1, i])
                    {
                        board[j + 1, i] = board[j, i] * 2;
                        if (!checking && board[j + 1, i] == 2048)
                            reached2048 = true;

                        UpdateScore(board[j, i]);
                        SetSprite(objectboard[j + 1, i], board[j, i] * 2);
                        SetSprite(objectboard[j, i], 0);
                        board[j, i] = 0;
                        j++;
                    }
                }
            }
        }

        if (dir == 3)
        {
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = board.GetLength(1) - 1; j > 0; j--)
                {
                    if (board[i, j] != 0 && board[i, j] == board[i, j - 1])
                    {
                        board[i, j - 1] = board[i, j] * 2;

                        if (!checking && board[i, j - 1] == 2048)
                            reached2048 = true;

                        UpdateScore(board[i,j]);
                        SetSprite(objectboard[i, j - 1], board[i,j] * 2);
                        SetSprite(objectboard[i, j], 0);
                        board[i, j] = 0;
                        board[i, j] = 0;
                        j--;
                    }
                }
            }
        }

    }

    private void NewNumber() 
    {
        int row = UnityEngine.Random.Range(0, 4);
        int col = UnityEngine.Random.Range(0, 4);
        int num = 0;
        HashSet<string> set = new HashSet<string>();
        while (true)
        {
            if (set.Count == 16)
            {
                CheckAvailableMoves();
                canGenerate = false;
                break;
            }

            if (board[row,col] == 0)
            {
                num = GenNumber();
                break;
            }
            else
            {
                set.Add(row+""+col);
                row = UnityEngine.Random.Range(0, 4);
                col = UnityEngine.Random.Range(0, 4);
            }

        }

        if (affectedRows == 0)
            canGenerate = false;
        else 
            canGenerate = true;

        if (canGenerate) 
        {
            board[row, col] = num;
            SetSprite(objectboard[row, col], num);
            StartCoroutine(PopNewTile(objectboard[row, col].transform, .1f));
        }
            
    }

    private IEnumerator PopNewTile(Transform tile, float duration)
    {
        Vector3 scale = new Vector3(0f, 0f, 0f);
        Vector3 endScale = Vector3.one;
        float time = 0;
        float t;

        while (time < duration)
        { 
            time += Time.deltaTime;
            t = time / duration;
            tile.localScale = Vector3.Lerp(scale, endScale, t);
            yield return null;
        }

    }

    private void EmptySprites()
    {
        for (int i = 0; i < objectboard.GetLength(0); i++)
        {
            for (int j = 0; j < objectboard.GetLength(1); j++)
            {
                objectboard[i,j].GetComponent<SpriteRenderer>().sprite = null;
            }
        }

    }

    private void EmptyBoard()
    {
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++) 
            {
                board[i,j] = 0;
            }
        }
    }

    private void UpdateScore(int val)
    {
        if (checking) return;
        score += val * 2;
        scoreTmp.text = "Score: "+score;
    }

    public void RestartGame()
    {
        SetHighScore();
        if (lost || reached2048)
        {
            //Debug.Log("If lost");
            lost = false;
            tmpOverlayText.enabled = false;
            overlay.GetComponent<SpriteRenderer>().color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0);
        }

        if(checking)
            checking = false;

        score = 0;
        scoreTmp.SetText("Score: 0");
        EmptyBoard();
        //Debug.Log("Calling init");
        InitBoard();
        SetUndoBoard();
    }

    public void ExitGame()
    {
        Application.targetFrameRate = 60;
        lost = false;
        tmpOverlayText.enabled = false;
        SetHighScore();
        DataPresistanceManager.Instance.SaveGame(); //poklice metodo save game v classu DataPresistenceManager
        SceneManager.LoadScene(0);
    }

    private void CheckAvailableMoves()
    {
        ar = 0;
        int[,] temp = new int[4,4];
        for (int i = 0; i < temp.GetLength(0); i++)
        {
            for (int j = 0; j < temp.GetLength(1); j++)
            {
                temp[i,j] = board[i,j];
            }
        }

        checking = true;

        Shift(0);
        Shift(1);
        Shift(2);
        Shift(3);

        if (affectedRows == 0)
        {
            lost = true;
            //Debug.Log("Lost set");
            return;
        }

        for (int i = 0; i < temp.GetLength(0); i++)
        {
            for (int j = 0; j < temp.GetLength(1); j++)
            {
                board[i, j] = temp[i, j];
            }
        }

        checking = false;

    }

    public bool GetLostStatus() 
    {
        return lost;
    }

    private void OnLoss()
    {
        overlay.GetComponent<SpriteRenderer>().color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0.6f);
        tmpOverlayText.SetText("YOU LOST");
        tmpOverlayText.enabled = true;
    }

    private void OnWin()
    {
        overlay.GetComponent<SpriteRenderer>().color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0.6f);
        tmpOverlayText.SetText("YOU  WON! \n AGAIN?");
        tmpOverlayText.enabled = true;
    }

    #region Interface Methods
    public void LoadData(GameData data)
    {
        this.hiScore = data.hiScore;
        this.score = data.score;
        this.board = ConvertTo2DArray(data.boardFlat);
        this.undoBoard = ConvertTo2DArray(data.undoBoardFlat);
    }

    public void SaveData(ref GameData data) 
    {
        data.hiScore = this.hiScore;
        data.score = this.score;
        data.boardFlat = Flatten2DArray(board);
        data.undoBoardFlat = Flatten2DArray(undoBoard);
    }

    #endregion
    
    private void SetHighScore()
    {
        //Debug.Log("Hi score: " + hiScore);

        if (hiScore < score)
        { 
            hiScore = score;
        }

        hiScoreTMP.text = "High Score: " + hiScore;

    }

    public void UndoMove()
    {
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                board[i,j] = undoBoard[i,j];
                SetSprite(objectboard[i,j], board[i,j]);
            }
        }
        score = undoScore;
        scoreTmp.text = "Score: "+score;

    }


    private void SetUndoBoard()
    {
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                undoBoard[i, j] = board[i, j];
            }
        }
        undoScore = score;
    }

    private void LoadBoard()
    {
        scoreTmp.SetText("Score: "+score);
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                SetSprite(objectboard[i, j], board[i, j]);
            }
        }
    }

    #region Application Quit Handler
    private void OnApplicationQuit()
    {
        DataPresistanceManager.Instance.SaveGame(); // on quit save board, da on next start lahko upor nadaljuje od prejsne igre
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) // ce app gre v background, save game data -> redundand za OnApplicationQuit, ki ne dela all the time
            DataPresistanceManager.Instance.SaveGame();
    }
    #endregion

    #region Array Transform methods
    private int[] Flatten2DArray(int[,] ar)
    {
        int[] array = new int[ar.GetLength(0) * ar.GetLength(1)];

        int idx = 0;

        for (int i = 0; i < ar.GetLength(0); i++)
        {
            for (int j = 0; j < ar.GetLength(1); j++)
            { 
                array[idx] = ar[i,j];
                idx++;
            }
        }
        return array;
    }

    private int[,] ConvertTo2DArray(int[] array)
    {
        int[,] newArray = new int[objectboard.GetLength(0), objectboard.GetLength(1)];

        if (array == null)
        {
            return null;
        }

        for (int i = 0; i < newArray.GetLength(0); i++)
        {
            for (int j = 0; j < newArray.GetLength(1); j++)
            {
                newArray[i, j] = array[i * newArray.GetLength(1) + j];
            }
        }

        return newArray;
    }
    #endregion
}
