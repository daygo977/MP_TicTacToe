using Unity.Netcode;
using UnityEngine;

// Makes sure that script is on the same game object as NetworkManager
[RequireComponent(typeof(NetworkManager))]
public class TicTacToeSpawner : MonoBehaviour
{   
    [SerializeField] private NetworkObject gamePrefab;

    private NetworkManager nm;      //network manager
    private NetworkObject spawned;  // prevents quick spam to spawn more than one

    private void Awake()
    {
        // Get NetworkManager directly (helps avoid singleton issues)
        nm = GetComponent<NetworkManager>();
        // When server starts, spawn game prefab once
        nm.OnServerStarted += OnServerStarted;
    }

    private void OnDestroy()
    {
        // Disconnect to prevent leaks if object is destroyed
        if (nm != null)
            nm.OnServerStarted -= OnServerStarted;
    }

    private void OnServerStarted()
    {
        // Only spawn once
        if (spawned != null) return;

        if (gamePrefab == null)
        {
            Debug.LogError("TicTacToeSpawner: Game Prefab is not assigned!");
            return;
        }

        // Server creates game object and netcode
        // spawns it across network so client also recieves it
        spawned = Instantiate(gamePrefab);
        spawned.Spawn(destroyWithScene: true);
    }
}