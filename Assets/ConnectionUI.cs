using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class ConnectionUI : MonoBehaviour
{
    // UI buttons
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    private void Awake()
    {   
        //When host/client is clicked start as host/client (for host, server and local client, and for client connect to host)
        hostButton.onClick.AddListener(() => NetworkManager.Singleton.StartHost());
        clientButton.onClick.AddListener(() => NetworkManager.Singleton.StartClient());
    }
}
