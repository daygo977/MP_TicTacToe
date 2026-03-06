using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private TMP_InputField addressInput;
    [SerializeField] private ushort port = 7777;

    private void Awake()
    {
        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);
    }

    private void StartHost()
    {
        // UnityTransport is network "socket" layer NGO uses for connections
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        // SetConnectionData(clientAddress, port, listenAddress)
        // clientAddress:
        //   The address the *local client* (the host is also a client) uses to connect.
        //   127.0.0.1 = "this same machine" (always valid).
        //
        // listenAddress:
        //   Where the server listens for incoming connections.
        //   0.0.0.0 = "listen on ALL network adapters" (Ethernet + Wi-Fi),
        //   which is required for other devices to connect over LAN.
        transport.SetConnectionData("127.0.0.1", port, "0.0.0.0");

        Debug.Log($"HOST listening on UDP {port}");

        // StartHost = start server + start a local client in the same app
        NetworkManager.Singleton.StartHost();
    }

    private void StartClient()
    {
        // Read address typed by user (client needs to know where the host is)
        // Examples:
        //   Same device testing: leave blank -> 127.0.0.1
        //   Two devices on LAN: type host IPv4 like 192.168.*.*
        string addr = addressInput != null ? addressInput.text.Trim() : "";

        // If blank, default localhost (connect to a host running on this same machine)
        if (string.IsNullOrEmpty(addr)) addr = "127.0.0.1";

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        // For clients, we only set the target host address + port
        transport.SetConnectionData(addr, port);

        Debug.Log($"CLIENT connecting to {addr}:{port}");

        // StartClient = attempt to connect to the host
        NetworkManager.Singleton.StartClient();
    }
}