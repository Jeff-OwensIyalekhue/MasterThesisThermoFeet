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

    [Header("References")]
    public CircularPeltierController peltierController;
    public SurveyServerManager surveyServerManager;
    public SurveyClientManager surveyClientManager;
    public ThermalFeedbackManager feedbackManager;

    public TMP_Text connectionInfos;
    public Image directionIndicator;
    public Button directionConfirmationButton;
    public Button likertConfirmationButton;
    public GameObject controlUI;
    public GameObject questionUI;
    public GameObject networkUI;
    public TMP_InputField ipInputField;
    public TMP_InputField portInputField;
    public List<GameObject> controlPanels;
    public bool isConnected;

    public bool isParticipant;

    public float signalStartTime;
    public float signalDetectionTime;
    public float guessSubmissionTime;
    public int guessedDirection;
    public int certaintyOfGuess;
    public bool isTrialFinished = false;

    [Header("System Settings")]
    public string connectionAddress;
    public int connectionPort = 7777;
    public string questionnaireUrl;

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

    private void Start()
    {
        ipInputField.text = NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address;
        portInputField.text = NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Port.ToString();
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
        {
            controlUI.SetActive(true);
            isConnected = NetworkManager.Singleton.StartServer();
        }
    }
#endregion

    public void SwitchControlPanels(int value)
    {
        for (int i = 0; i < controlPanels.Count; i++)
        {
            if (i == value)
                controlPanels[i].SetActive(true);
            else
                controlPanels[i].SetActive(false);
        }
    }

    public void EndApp()
    {
#if !UNITY_ANDROID
        feedbackManager.SaveStudyParams();
#endif
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}
