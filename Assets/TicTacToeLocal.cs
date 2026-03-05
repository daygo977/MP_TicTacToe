using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToeLocal : MonoBehaviour
{
    [Header("Board")]
    [SerializeField] private Button[] cellButtons = new Button[9];
    [SerializeField] private TMP_Text[] cellLabels = new TMP_Text[9];

    [Header("UI")]
    [SerializeField] private TMP_Text playerXWinsText;
    [SerializeField] private TMP_Text playerOWinsText;

    [SerializeField] private TMP_Text playerXStatusText;
    [SerializeField] private TMP_Text playerOStatusText;

    [SerializeField] private Button newGameButton;

    private int[] board = new int[9]; // 0 empty, 1 is X, 2 is O
    private int currentPlayer = 1;    //1 is place holder, its randomized once game starts and same for game resets
    private bool gameOver;

    private int xWins, oWins;        //Keeps count of wins per player

    private void Awake()
    {
        for (int i = 0; i < 9; i++) //hooks up grid buttons
        {
            int idx = i;
            cellButtons[i].onClick.AddListener(() => OnCellClicked(idx));
        }

        if (newGameButton != null)
            newGameButton.onClick.AddListener(ResetRound); //clicking new game button resets grid

        ResetRound(); //start with clean grid
    }

    /// <summary>
    /// Logic for when a button in 3x3 grid is pressed
    /// </summary>
    /// <param name="index"></param>
    private void OnCellClicked(int index)
    {
        //return if game is over, also if button is already pressed
        if (gameOver) return;
        if (board[index] != 0) return;

        //save move by current player
        board[index] = currentPlayer;
        cellLabels[index].text = (currentPlayer == 1) ? "X" : "O";  //Shows bwhich player clicked the button (P1 is X, P2 is O)
        cellButtons[index].interactable = false;    //lock the button

        int winner = CheckWinner();    //check winner
        if (winner != 0)
        {
            gameOver = true; //end round

            if (winner == 1) xWins++;   //add score to winner
            else oWins++;

            UpdateScoreUI();            //update score text
            SetWinLoseStatus(winner);   // X WIN / O LOSE etc
            DisableAllCells();          //disable all buttons on grid after winner is determined
            return;
        }

        if (IsDraw())           //check draw
        {   
            gameOver = true;    //end round
            SetDrawStatus();    //show DRAW in status text
            return;
        }

        currentPlayer = (currentPlayer == 1) ? 2 : 1;   //switch turns (random)
        SetTurnStatus();   //set TURN to correct player
    }

    /// <summary>
    /// Resets grid by setting index values back to 0 (1 is X and 2 is O), removes text to blank, and allows buttons
    /// to be interactable again.
    /// Also randomizes who goes first after reset, and updates score and sets who ever got picked first
    /// </summary>
    private void ResetRound()
    {
        for (int i = 0; i < 9; i++)
        {
            board[i] = 0;
            cellLabels[i].text = "";
            cellButtons[i].interactable = true;
        }

        gameOver = false;
        currentPlayer = Random.Range(1, 3); // returns 1 or 2

        UpdateScoreUI();
        SetTurnStatus();
    }

    /// <summary>
    /// Updates score of how many times each player has won
    /// </summary>
    private void UpdateScoreUI()
    {
        if (playerXWinsText) playerXWinsText.text = xWins.ToString();
        if (playerOWinsText) playerOWinsText.text = oWins.ToString();
    }

    /// <summary>
    /// Based on which player is currently placing their X/O, a status text will appear for said player
    /// </summary>
    private void SetTurnStatus()
    {
        if (playerXStatusText) playerXStatusText.text = (!gameOver && currentPlayer == 1) ? "TURN" : "";
        if (playerOStatusText) playerOStatusText.text = (!gameOver && currentPlayer == 2) ? "TURN" : "";
    }

    /// <summary>
    /// Which every player wins by forming a row of 3, will change the strings to correct win/loss status
    /// </summary>
    private void SetWinLoseStatus(int winner)
    {
        if (playerXStatusText) playerXStatusText.text = (winner == 1) ? "WIN" : "LOSE";
        if (playerOStatusText) playerOStatusText.text = (winner == 2) ? "WIN" : "LOSE";
    }

    /// <summary>
    /// If all buttons are filled, and no one wins, then draw
    /// </summary>
    private void SetDrawStatus()
    {
        if (playerXStatusText) playerXStatusText.text = "DRAW";
        if (playerOStatusText) playerOStatusText.text = "DRAW";
    }
    
    /// <summary>
    /// Disable 3x3 grid
    /// </summary>
    private void DisableAllCells()
    {
        for (int i = 0; i < 9; i++)
            cellButtons[i].interactable = false;
    }

    /// <summary>
    /// Is draw if no buttons in grid are left
    /// </summary>
    private bool IsDraw()
    {
        for (int i = 0; i < 9; i++)             //check all 9 buttons, if found empty spot, no draw
            if (board[i] == 0) return false;
        return true;                            //no empty spots, then draw, (if no winner)
    }

    /// <summary>
    /// Checks to see if any of the following lines are done by one player, then returns
    /// which ever player completed one of the 8 lines first.
    /// 0 is none, 1 is player X, 2 is player O
    /// </summary>
    private int CheckWinner()
    {
        int[,] lines =     //all 8 possible ways to win Tic-Tac-Toe
        {
            {0,1,2},{3,4,5},{6,7,8},    //rows
            {0,3,6},{1,4,7},{2,5,8},    //columns
            {0,4,8},{2,4,6}             //diagonals
        };

        for (int i = 0; i < 8; i++)     //check each line at a time
        {
            //Pick 3 board positions for this line
            int a = lines[i, 0], b = lines[i, 1], c = lines[i, 2];

            //Look at the first button in that line
            int v = board[a];

            //If first  button is empty, this line can't win
            //If not empty, check if all 3 buttons match (X or O, or 1 2)
            if (v != 0 && v == board[b] && v == board[c])
                return v;   //v will be 1 (X) or 2(O) for winner
        }
        return 0;
    }
}