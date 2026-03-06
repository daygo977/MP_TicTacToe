using UnityEngine;
using Unity.Netcode;

public class TicTacToeNetGame : NetworkBehaviour
{
    // NetworkList is synced list (server writes, clients automatically receive updates)
    // Used for the 9 board buttons: 0 empty, 1 X, 2 O
    public NetworkList<int> Board = new NetworkList<int>();

    // Store which clientId is playing X and which is playing O
    // ulong.MaxValue = "not assigned yet"
    public NetworkVariable<ulong> PlayerXID = new NetworkVariable<ulong>(ulong.MaxValue);
    public NetworkVariable<ulong> PlayerOID = new NetworkVariable<ulong>(ulong.MaxValue);

    // Shared turn and scores (everyone sees the same values)
    public NetworkVariable<int> CurrentPlayerMark = new NetworkVariable<int>(1);
    public NetworkVariable<int> XWins = new NetworkVariable<int>(0);
    public NetworkVariable<int> OWins = new NetworkVariable<int>(0);

    // Shared game state: 0 is playing, 1 is win, 2 is draw
    public NetworkVariable<int> GameState = new NetworkVariable<int>(0);
    public NetworkVariable<int> WinnerMark = new NetworkVariable<int>(0);

    public override void OnNetworkSpawn()
    {
        // Only server should set official game state.
        // Clients receive changes automatically through NetworkVars/NetworkList.
        if (!IsServer) return;
    
        // Host/server becomes X
        PlayerXID.Value = NetworkManager.ServerClientId;

        // Initialize board/grid once
        // If we don't do this, Board[index] can error or be missing on clients
        if (Board.Count != 9)
        {
            Board.Clear();
            for (int i = 0; i < 9; i++) Board.Add(0);
        }

        // Listen for clients joining so we can assign Player O
        // If both players are already present, randomize who starts
        NetworkManager.OnClientConnectedCallback += OnClientConnected;
        PickRandomStarterIfReady();
    }

    public override void OnNetworkDespawn()
    {
        // Disconnect when NetworkObject is destroyed (prevents event leaks)
        if (IsServer && NetworkManager != null)
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientID)
    {
        // First non-host client becomes O
        if (clientID != PlayerXID.Value && PlayerOID.Value == ulong.MaxValue)
            PlayerOID.Value = clientID;
        
        // If both players now exist, decide who starts
        PickRandomStarterIfReady();
    }

    private void PickRandomStarterIfReady()
    {
        // We randomize the starter if both players connected.
        if (PlayerXID.Value == ulong.MaxValue || PlayerOID.Value == ulong.MaxValue) return;
        CurrentPlayerMark.Value = Random.Range(1, 3); // 1 or 2
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestMoveServerRpc(int index, ServerRpcParams rpcParams = default)
    {
        // Reject moves if round already ended
        // Reject invalid indices
        // Reject moves on an already-filled button
        if (GameState.Value != 0) return;
        if (index < 0 || index >= 9) return;
        if (Board[index] != 0) return;

        // The client who sent this RPC (who clicked)
        ulong sender = rpcParams.Receive.SenderClientId;

        // Sender must be the correct player for current turn (X=1 / O=2 / no one=0)
        int senderMark = GetMarkForClient(sender);
        if (senderMark == 0) return;    // not X or O
        if (senderMark != CurrentPlayerMark.Value) return;

        // Apply move, server writes the board
        Board[index] = senderMark;

        int winner = CheckWinner(); //check winner
        if (winner != 0)
        {
            WinnerMark.Value = winner;
            GameState.Value = 1;
            //add score to winner
            if (winner == 1) XWins.Value++; else OWins.Value++;
            return;
        }

        if (IsDraw())
        {
            GameState.Value = 2;
            return;
        }

        // Switch turns, if no win or draw
        CurrentPlayerMark.Value = (CurrentPlayerMark.Value == 1) ? 2 : 1;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestResetServerRpc()
    {
        // Keep scores, server resets the board for everyone
        for (int i = 0; i < 9; i++) Board[i] = 0;
        WinnerMark.Value = 0;
        GameState.Value = 0;

        // Random starter each new round (only if both players are connected)
        PickRandomStarterIfReady();
    }

    private int GetMarkForClient(ulong clientID)
    {
        // Map each connected client to their role/mark
        if (clientID == PlayerXID.Value) return 1;  // X
        if (clientID == PlayerOID.Value) return 2;  // O
        return 0;       
    }
    
    /// <summary>
    /// Is draw if no buttons in grid are left
    /// </summary>
    private bool IsDraw()
    {
        for (int i = 0; i < 9; i++)         //check all 9 buttons, if found empty spot, no draw
            if (Board[i] == 0) return false;
        return true;                //no empty spots, then draw, (if no winner)
    }

    /// <summary>
    /// Checks to see if any of the following lines are done by one player, then returns
    /// which ever player completed one of the 8 lines first.
    /// 0 is none, 1 is player X, 2 is player O
    /// </summary>
    private int CheckWinner()
    {
        int[,] lines =    //all 8 possible ways to win Tic-Tac-Toe
        {
            {0,1,2},{3,4,5},{6,7,8},   //rows
            {0,3,6},{1,4,7},{2,5,8},   //columns
            {0,4,8},{2,4,6}            //diagonals
        };

        for (int i = 0; i < 8; i++)   //check each line at a time
        {
            //Pick 3 board positions for this line
            int a = lines[i,0], b = lines[i,1], c = lines[i,2];

            //Look at the first button in that line
            int v = Board[a];

            //If first  button is empty, this line can't win
            //If not empty, check if all 3 buttons match (X or O, or 1 2)
            if (v != 0 && v == Board[b] && v == Board[c]) return v;   //v will be 1 (X) or 2(O) for winner
        }
        return 0;
    }
}
