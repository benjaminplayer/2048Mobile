using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Windows;
using Unity.VisualScripting;

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
    [SerializeField] private bool reached2048 = false, checking = false, lost = false, canGenerate = true, isShifting = false;
    [SerializeField] private float moveDuration = 0.2f;
    private Color overlayColor;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        tmpOverlayText.enabled = false;
        overlayColor = overlay.GetComponent<SpriteRenderer>().color;
        overlay.GetComponent<SpriteRenderer>().color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0);
        FillboardFromRows();
        InitBoard();
    }

    /*private void Start()
    {
        //DataPresistanceManager.Instance.LoadGame();
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

    }*/

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
        /*board[0, 3] = 2;
         board[0, 1] = 2;
         board[1, 3] = 2;
         /*board[0, 3] = 2;
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
    private IEnumerator ShiftLeft()
    {
        Debug.Log("Inside shift left");

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

        //Naredi list coroutines, da jih lahko nato executa use hkrati
        List<IEnumerator> moveCoroutines = new List<IEnumerator>();

        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = shiftIdx[i]; j < board.GetLength(1); j++)
            {
                if (board[i, shiftIdx[i]] == 0)
                {
                    if (board[i, j] != 0 && j != shiftIdx[i])
                    {
                        board[i, shiftIdx[i]] = board[i, j];
                        moveCoroutines.Add(MoveTile(i, j, i, shiftIdx[i], moveDuration));
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
                        moveCoroutines.Add(MoveTile(i, j, i, shiftIdx[i] + 1, moveDuration));
                        board[i, j] = 0;
                        shiftIdx[i]++; // posodobi shift idx;
                        affectedRows++;

                    }
                }
            }
        }

        foreach (IEnumerator c in moveCoroutines)
        {
            StartCoroutine(c);
        }


        yield return new WaitForSeconds(moveDuration); // pocaka za duration movementa preden konca coroutine

    }

    private IEnumerator ShiftRight()
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

        List<IEnumerator> moveCoroutines = new List<IEnumerator>();

        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = shiftIdx[i]; j >= 0; j--)
            {
                if (board[i, shiftIdx[i]] == 0)
                {
                    if (board[i, j] != 0 && j != shiftIdx[i])
                    {
                        board[i, shiftIdx[i]] = board[i, j];
                        moveCoroutines.Add(MoveTile(i, j, i, shiftIdx[i], moveDuration));
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
                        moveCoroutines.Add(MoveTile(i, j, i, shiftIdx[i] - 1, moveDuration));
                        board[i, j] = 0;
                        shiftIdx[i]--;
                        affectedRows++;
                    }
                }
            }
        }

        foreach (IEnumerator c in moveCoroutines)
        {
            StartCoroutine(c);
        }


        yield return new WaitForSeconds(moveDuration); // pocaka za duration movementa preden konca coroutine

    }

    private IEnumerator ShiftUp()
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

        List<IEnumerator> moveCoroutines = new List<IEnumerator>();

        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = shiftIdx[i] + 1; j < board.GetLength(1); j++)
            {
                if (board[shiftIdx[i], i] == 0)
                {
                    if (board[j, i] != 0)
                    {
                        board[shiftIdx[i], i] = board[j, i];
                        moveCoroutines.Add(MoveTile(j, i, shiftIdx[i], i, moveDuration));
                        board[j, i] = 0;
                        shiftIdx[i]++;
                        affectedRows++;
                    }
                }
                else if (board[j, i] != 0)
                {
                    board[shiftIdx[i] + 1, i] = board[j, i];
                    moveCoroutines.Add(MoveTile(j, i, shiftIdx[i] + 1, i, moveDuration));
                    board[j, i] = 0;
                    shiftIdx[i]++;
                    affectedRows++;
                }
            }

        }

        foreach (IEnumerator c in moveCoroutines)
        {
            StartCoroutine(c);
        }


        yield return new WaitForSeconds(moveDuration); // pocaka za duration movementa preden konca coroutine
    }

    private IEnumerator ShiftDown()
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

        List<IEnumerator> moveCoroutines = new List<IEnumerator>();

        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = shiftIdx[i]; j >= 0; j--)
            {
                if (board[shiftIdx[i], i] == 0)
                {
                    if (board[j, i] != 0 && j != shiftIdx[i])
                    {
                        board[shiftIdx[i], i] = board[j, i];
                        moveCoroutines.Add(MoveTile(j, i, shiftIdx[i], i, moveDuration));
                        board[j, i] = 0;
                        shiftIdx[i]--;
                        affectedRows++;
                    }
                }
                else if (board[j, i] != 0 && j != shiftIdx[i])
                {
                    board[shiftIdx[i] - 1, i] = board[j, i];
                    moveCoroutines.Add(MoveTile(j, i, shiftIdx[i] - 1, i, moveDuration));
                    board[j, i] = 0;
                    shiftIdx[i]--;
                    affectedRows++;
                }
            }

        }

        foreach (IEnumerator c in moveCoroutines)
        {
            StartCoroutine(c);
        }


        yield return new WaitForSeconds(moveDuration); // pocaka za duration movementa preden konca coroutine

    }
    #endregion
    private IEnumerator Shift(int dir)
    {
        isShifting = true;
        if (!checking)
            SetUndoBoard();

        Debug.Log("Shift logic called!");
        Debug.Log("Cheking? " + checking);

        switch (dir)
        {
            case 0:
                yield return StartCoroutine(ShiftUp());
                yield return StartCoroutine(Merge(dir));
                yield return StartCoroutine(ShiftUp());
                break;
            case 1:
                Debug.Log("Shift left called");
                yield return StartCoroutine(ShiftLeft());
                yield return StartCoroutine(Merge(dir));
                yield return StartCoroutine(ShiftLeft());
                break;
            case 2:
                yield return StartCoroutine(ShiftDown());
                yield return StartCoroutine(Merge(dir));
                yield return StartCoroutine(ShiftDown());
                break;
            case 3:
                yield return StartCoroutine(ShiftRight());
                yield return StartCoroutine(Merge(dir));
                yield return StartCoroutine(ShiftRight());
                break;
            default:
                throw new System.Exception("It appears I have made an oopsie");
        }

        if (reached2048)
        {
            //TODO: Create some sort of logic here :/
            OnWin();
            yield break;
        }

        if (!checking)
        {
            Debug.Log("Spawn new tile");
            Debug.Log("Affected rows: " + affectedRows);
            NewNumber();
            affectedRows = 0;
        }

        if (lost)
        {
            //Debug.Log("Lost :(");
            OnLoss();
        }
        isShifting = false;
    }

    public void TryShift(int val)
    {
        if (isShifting) return;
        StartCoroutine(Shift(val));
    }

    private IEnumerator MoveTile(int x, int y, int x1, int y1, float duration)
    {

        Transform tile1 = objectboard[x, y].transform;
        Transform tile2 = objectboard[x1, y1].transform;

        Vector3 startPos = tile1.localPosition;
        Vector3 endPos = tile2.localPosition;

        float time = 0, t = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            t = time / duration;
            tile1.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        SetSprite(tile1.gameObject, 0); //i, j, i, shiftIdx[i], moveDuration -> x y x1, y1
        SetSprite(tile2.gameObject, board[x1, y1]);

        tile1.transform.localPosition = startPos; // reset the og position
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

    private IEnumerator Merge(int dir)
    {
        Debug.Log("Merge called with and arg: " + dir);

        List<IEnumerator> mergeMoves = new List<IEnumerator>();

        if (dir == 2)
        {
            for (int i = board.GetLength(0) - 1; i >= 0; i--)
            {
                for (int j = board.GetLength(1) - 1; j > 0; j--)
                {
                    if (board[j, i] == board[j - 1, i])
                    {
                        board[j, i] = board[j - 1, i] * 2;

                        mergeMoves.Add(MoveTile(j - 1, i, j, i, moveDuration));
                        if (!checking && board[j - 1, i] == 2048)
                            reached2048 = true;

                        UpdateScore(board[j - 1, i]);
                        board[j - 1, i] = 0;
                        j--;
                        affectedRows++;
                    }
                }
            }
        }

        if (dir == 1)
        {
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (j + 1 < board.GetLength(1) && board[i, j] != 0 && board[i, j] == board[i, j + 1])
                    {
                        board[i, j] = board[i, j + 1] * 2;

                        mergeMoves.Add(MoveTile(i, j + 1, i, j, moveDuration));

                        if (!checking && board[i, j + 1] == 2048)
                            reached2048 = true;
                        UpdateScore(board[i, j + 1]);
                        board[i, j + 1] = 0;
                        j++;
                        affectedRows++;
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
                        board[j, i] = board[j + 1, i] * 2;

                        mergeMoves.Add(MoveTile(j + 1, i, j, i, moveDuration));

                        if (!checking && board[j + 1, i] == 2048)
                            reached2048 = true;

                        UpdateScore(board[j + 1, i]);
                        board[j + 1, i] = 0;
                        j++;
                        affectedRows++;
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
                        board[i, j] = board[i, j - 1] * 2;

                        mergeMoves.Add(MoveTile(i, j - 1, i, j, moveDuration));

                        if (!checking && board[i, j - 1] == 2048)
                            reached2048 = true;

                        UpdateScore(board[i, j]);
                        board[i, j - 1] = 0;
                        j--;
                        affectedRows++;
                    }
                }
            }
        }

        foreach (IEnumerator e in mergeMoves)
        {
            StartCoroutine(e);
        }

        yield return new WaitForSeconds(moveDuration);

        // Update board
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                SetSprite(objectboard[i, j], board[i, j]);
            }
        }

    }

    private void NewNumber()
    {
        Debug.Log("New number method");

        List<Vector2Int> emptyCells = new List<Vector2Int>();

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (board[i, j] == 0)
                    emptyCells.Add(new Vector2Int(i, j));
            }
        }

        if (emptyCells.Count == 0)
        {
            CheckAvailableMoves();
            return;
        }

        if (affectedRows == 0)
            return;


        Vector2Int pos = emptyCells[UnityEngine.Random.RandomRange(0, emptyCells.Count)]; // zbere random vrednost iz lista, kjer hranimo koordinate praznih polj
        int number = GenNumber();
        board[pos.x, pos.y] = number;
        SetSprite(objectboard[pos.x, pos.y], number);
        StartCoroutine(PopNewTile(objectboard[pos.x, pos.y].transform, 0.1f));

        Debug.Log("Placed a new tile at " + pos.x + " " + pos.y + "; value: " + number);

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
                objectboard[i, j].GetComponent<SpriteRenderer>().sprite = null;
            }
        }

    }

    private void EmptyBoard()
    {
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                board[i, j] = 0;
            }
        }
    }

    private void UpdateScore(int val)
    {
        if (checking) return;
        score += val * 2;
        scoreTmp.text = "Score: " + score;
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

        if (checking)
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
        ////DataPresistanceManager.Instance.SaveGame(); //poklice metodo save game v classu DataPresistenceManager
        SceneManager.LoadScene(0);
    }

    private void CheckAvailableMoves()
    {
        int[,] temp = new int[4, 4];
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
                temp[i, j] = board[i, j];

        bool canMove = false;
        for (int d = 0; d < 4; d++)
        {
            int[,] copy = (int[,])temp.Clone(); // naredi kopijo boarda; d -> smer preverjanja
            if (TryShift(copy, d))
            {
                canMove = true;
                break;
            }
        }

        lost = !canMove;

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
                board[i, j] = undoBoard[i, j];
                SetSprite(objectboard[i, j], board[i, j]);
            }
        }
        score = undoScore;
        scoreTmp.text = "Score: " + score;

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
        scoreTmp.SetText("Score: " + score);
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
        //DataPresistanceManager.Instance.SaveGame(); // on quit save board, da on next start lahko upor nadaljuje od prejsne igre
    }

    private void OnApplicationPause(bool pause)
    {
        //if (pause) // ce app gre v background, save game data -> redundand za OnApplicationQuit, ki ne dela all the time
        //DataPresistanceManager.Instance.SaveGame();
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
                array[idx] = ar[i, j];
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

    #region logical Shift
    private bool TryShift(int[,] tempBoard, int dir)
    {
        // Rotate the board to reuse left-shift logic
        switch (dir)
        {
            case 0: RotateBoard(tempBoard); RotateBoard(tempBoard); break; // Up
            case 2: RotateBoard(tempBoard); break;                         // Down
            case 3: RotateBoard(tempBoard); RotateBoard(tempBoard); RotateBoard(tempBoard); break; // Right
        }

        bool changed = TryShiftLeft(tempBoard);

        // Rotate back
        switch (dir)
        {
            case 0: RotateBoard(tempBoard); RotateBoard(tempBoard); break;
            case 2: RotateBoard(tempBoard); RotateBoard(tempBoard); RotateBoard(tempBoard); break;
            case 3: RotateBoard(tempBoard); break;
        }

        return changed;
    }

    private void RotateBoard(int[,] board)
    {
        int size = 4;
        int[,] temp = new int[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                temp[j, size - 1 - i] = board[i, j];
            }
        }

        // Copy back
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                board[i, j] = temp[i, j];
            }
        }
    }

    private bool TryShiftLeft(int[,] tempBoard)
    {
        bool changed = false;

        for (int i = 0; i < 4; i++)
        {
            int[] row = new int[4];
            int idx = 0;

            // Collect non-zero values
            for (int j = 0; j < 4; j++)
            {
                if (tempBoard[i, j] != 0)
                {
                    row[idx++] = tempBoard[i, j];
                }
            }

            // Merge identical values
            for (int j = 0; j < 3; j++)
            {
                if (row[j] != 0 && row[j] == row[j + 1])
                {
                    row[j] *= 2;
                    row[j + 1] = 0;
                    j++; // Skip next to prevent double merge
                }
            }

            // Pack again after merge
            int[] newRow = new int[4];
            idx = 0;
            for (int j = 0; j < 4; j++)
            {
                if (row[j] != 0)
                {
                    newRow[idx++] = row[j];
                }
            }

            // Update tempBoard row and detect change
            for (int j = 0; j < 4; j++)
            {
                if (tempBoard[i, j] != newRow[j])
                {
                    changed = true;
                }
                tempBoard[i, j] = newRow[j];
            }
        }

        return changed;
    }

    #endregion
}