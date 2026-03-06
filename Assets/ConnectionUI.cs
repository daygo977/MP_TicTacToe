using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private TMP_InputField addressInput; // blank = localhost

    private void Awake()
    {
        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);
    }

    private void StartHost()
    {
        SetAddressIfProvided(); // optional; host can leave blank
        NetworkManager.Singleton.StartHost();
    }

    private void StartClient()
    {
        SetAddressIfProvided(); // client should enter host LAN IP
        NetworkManager.Singleton.StartClient();
    }

    private void SetAddressIfProvided()
    {
        if (addressInput == null) return;

        string addr = addressInput.text.Trim();
        if (string.IsNullOrEmpty(addr)) addr = "127.0.0.1";

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = addr; // port stays 7777 unless you changed it
    }
}