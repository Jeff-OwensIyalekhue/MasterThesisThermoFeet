using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class SurveyServerManager : NetworkBehaviour
{
    public GameObject directionUI;
    public GameObject likertUI;
    public GameObject messageUI;
    public TMP_Text messageText;

    void Awake()
    {
        if(AppManager.Singleton.surveyServerManager == null)
            AppManager.Singleton.surveyServerManager = this;
    }

    [ClientRpc]
    public void SendSignalStartClientRpc()
    {
        AppManager.Singleton.signalStartTime = Time.time;
    }

    [ClientRpc]
    public void SendSignalDetectionClientRpc()
    {
        AppManager.Singleton.signalDetectionTime = Time.time - AppManager.Singleton.signalStartTime;
    }

    [ClientRpc]
    public void SendGuessSubmissionClientRpc()
    {
        AppManager.Singleton.guessSubmissionTime = Time.time - AppManager.Singleton.signalStartTime;
    }

    [ClientRpc]
    public void DisplayLikertClientRpc()
    {
        messageUI.SetActive(false);
        directionUI.SetActive(false);
        likertUI.SetActive(true);
    }

    [ClientRpc]
    public void DisplayDirectionClientRpc()
    {
        messageUI.SetActive(false);
        directionUI.SetActive(true);
        likertUI.SetActive(false);
    }

    [ClientRpc]
    public void DisplayMessageClientRpc(string message = "Wait for further instructions.")
    {
        messageText.text = message;
        messageUI.SetActive(true);
        directionUI.SetActive(false);
        likertUI.SetActive(false);

    }
}
