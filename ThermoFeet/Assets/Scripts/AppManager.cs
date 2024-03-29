using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class AppManager : MonoBehaviour
{
    public static AppManager Singleton;

    public TMP_Text connectionInfos;
    public Image directionIndicator;
    public GameObject questionUI;
    public GameObject networkUI;
    public bool isConnected;

    public bool isParticipant;
    private string connectionAddress;
    private int connectionPort = 7777;

    private void Awake()
    {
        if (Singleton == null)
            Singleton = this;
        else
            Destroy(this);

#if UNITY_ANDROID
        isParticipant = true;
#else
        isParticipant = false;
#endif
    }

    private void Update()
    {
        if (connectionAddress == null)
        {
            UnityTransport uT = NetworkManager.Singleton.GetComponent<UnityTransport>();
            connectionAddress = uT.ConnectionData.Address;
            connectionPort = uT.ConnectionData.Port;
        }

        if (NetworkManager.Singleton.IsServer)
            connectionInfos.text = "Connected clients: " + NetworkManager.Singleton.ConnectedClientsList.Count;
    }

    #region Network Functions
    public void SetIP(string ip)
    {
        connectionAddress = ip;
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(connectionAddress, (ushort)connectionPort);
    }
    public void SetPort(string port)
    {
        connectionPort = int.Parse(port);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(connectionAddress, (ushort)connectionPort);
    }
    public void StartConnection()
    {
        networkUI.SetActive(false);
        if (isParticipant)
        {
            questionUI.SetActive(true);
            isConnected = NetworkManager.Singleton.StartClient();
        }
        else
            isConnected = NetworkManager.Singleton.StartServer();
    }
    #endregion

    public void EndApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}
