using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;

public class SurveyServerManager : NetworkBehaviour
{
    private bool isShowingDirection = false;
    public TMP_Text displayChangeText;
    public Button displayChangeButton;

    public GameObject directionUI;
    public GameObject likertUI;
    public GameObject messageUI;
    public TMP_Text messageText;

    void Awake()
    {
        displayChangeButton.onClick.AddListener(SwitchDisplay);
    }

    void Update()
    {

    }

    [ClientRpc]
    public void SendSignalStartClientRpc()
    {
        AppManager.Singleton.signalStartTime = Time.time;
    }

    public void SwitchDisplay()
    {
        isShowingDirection = !isShowingDirection;
        if (isShowingDirection)
        {
            displayChangeText.text = "show message";
            DisplayDirectionClientRpc();
        }
        else
        {
            displayChangeText.text = "show direction";
            DisplayMessageClientRpc();
        }
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
    public void DisplayMessageClientRpc()
    {
        messageText.text = "Hello" + Random.Range(0, 10);
        messageUI.SetActive(true);
        directionUI.SetActive(false);
        likertUI.SetActive(false);
    }
}
