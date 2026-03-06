using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToeNetUI : MonoBehaviour
{
    [Header("Board")]
    [SerializeField] private Button[] cellButtons = new Button[9];
    [SerializeField] private TMP_Text[] cellLabels = new TMP_Text[9];

    [Header("UI")]
    [SerializeField] private TMP_Text playerXWinsText;
    [SerializeField] private TMP_Text playerOWinsText;
    [SerializeField] private TMP_Text playerXStatusText; // TURN/WIN/LOSE/DRAW
    [SerializeField] private TMP_Text playerOStatusText;
    [SerializeField] private Button newGameButton;

    private TicTacToeNetGame game;

    private void Awake()
    {
        for (int i = 0; i < 9; i++)
        {
            int idx = i;
            cellButtons[i].onClick.AddListener(() => OnCellClicked(idx));
        }

        if (newGameButton != null)
            newGameButton.onClick.AddListener(OnNewGameClicked);
    }

    private IEnumerator Start()
    {
        while (game == null)
        {
            game = FindFirstObjectByType<TicTacToeNetGame>();
            yield return null;
        }

        game.Board.OnListChanged += _ => Refresh();
        game.PlayerOID.OnValueChanged += (_, __) => Refresh();
        game.CurrentPlayerMark.OnValueChanged += (_, __) => Refresh();
        game.XWins.OnValueChanged += (_, __) => Refresh();
        game.OWins.OnValueChanged += (_, __) => Refresh();
        game.GameState.OnValueChanged += (_, __) => Refresh();
        game.WinnerMark.OnValueChanged += (_, __) => Refresh();

        Refresh();
    }

    private void OnCellClicked(int index)
    {
        if (game == null) return;
        if (!NetworkManager.Singleton.IsConnectedClient) return;
        game.RequestMoveServerRpc(index);
    }

    private void OnNewGameClicked()
    {
        if (game == null) return;
        if (!NetworkManager.Singleton.IsConnectedClient) return;
        game.RequestResetServerRpc(); // server resets board + random starter
    }

    private void Refresh()
    {
        if (game == null) return;

        bool bothPlayers = game.PlayerOID.Value != ulong.MaxValue;

        // Scores
        if (playerXWinsText) playerXWinsText.text = game.XWins.Value.ToString();
        if (playerOWinsText) playerOWinsText.text = game.OWins.Value.ToString();

        // Status
        if (!bothPlayers)
        {
            playerXStatusText.text = "WAIT";
            playerOStatusText.text = "WAIT";
        }
        else if (game.GameState.Value == 0)
        {
            playerXStatusText.text = (game.CurrentPlayerMark.Value == 1) ? "TURN" : "";
            playerOStatusText.text = (game.CurrentPlayerMark.Value == 2) ? "TURN" : "";
        }
        else if (game.GameState.Value == 1)
        {
            playerXStatusText.text = (game.WinnerMark.Value == 1) ? "WIN" : "LOSE";
            playerOStatusText.text = (game.WinnerMark.Value == 2) ? "WIN" : "LOSE";
        }
        else
        {
            playerXStatusText.text = "DRAW";
            playerOStatusText.text = "DRAW";
        }

        // Board + interactable (only your turn, only if both players connected)
        ulong me = NetworkManager.Singleton.LocalClientId;
        int myMark = (me == game.PlayerXID.Value) ? 1 : (me == game.PlayerOID.Value) ? 2 : 0;

        for (int i = 0; i < 9; i++)
        {
            int v = (game.Board.Count == 9) ? game.Board[i] : 0;
            cellLabels[i].text = (v == 1) ? "X" : (v == 2) ? "O" : "";

            bool canClick =
                bothPlayers &&
                game.GameState.Value == 0 &&
                myMark != 0 &&
                myMark == game.CurrentPlayerMark.Value &&
                v == 0;

            cellButtons[i].interactable = canClick;
        }
    }
}