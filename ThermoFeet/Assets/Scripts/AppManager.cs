using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class AppManager : MonoBehaviour
{
    public GameObject ServerUI;
    public GameObject ClientConnection;
    public GameObject ServerConnection;

    public TMP_Text connectionInfos;

    private void Awake()
    {
#if UNITY_ANDROID
        ClientConnection.SetActive(true);
        ServerConnection.SetActive(false);
#else
        ClientConnection.SetActive(false);
        ServerConnection.SetActive(true);
#endif
    }

    private void Update()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            if (ServerUI.activeSelf)
                ServerUI.SetActive(false);
        }
        if (NetworkManager.Singleton.IsServer)
        {
            if(!ServerUI.activeSelf)
                ServerUI.SetActive(true);

            connectionInfos.text = NetworkManager.Singleton.ConnectedClients.Count.ToString();
        }

    }

    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    public void EndApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}
