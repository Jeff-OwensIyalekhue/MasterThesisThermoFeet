using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class SurveyClientManager : NetworkBehaviour
{
    [Header("Settings")]
    public float trackZoneBottom = -10;
    public float trackZoneTop = 11;
    public Image directionIndicator;
    public Button directionConfirmButton;
    public Button likertConfirmButton;

    [Header("Network Vairables")]
    public NetworkVariable<int> nGuessedDirection = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> nCertaintyOfGuess = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> nStartTime = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> nDetectionTime = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> nSubmitTime = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);

    private int angle;
    private InputActions inputActions;

    void Awake()
    {
        inputActions = new InputActions();
        inputActions.Standard.Enable();

        if (AppManager.Singleton.surveyClientManager == null)
            AppManager.Singleton.surveyClientManager = this;

        directionIndicator = AppManager.Singleton.directionIndicator;

        directionConfirmButton = AppManager.Singleton.directionConfirmationButton;
        directionConfirmButton.onClick.AddListener(() =>
        {
            SendDirectionGuessServerRpc();
            directionConfirmButton.gameObject.SetActive(false);
        });

        likertConfirmButton = AppManager.Singleton.likertConfirmationButton;
        likertConfirmButton.onClick.AddListener(() => { SendLikertAnwserServerRpc(); });
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            if (nStartTime.Value != AppManager.Singleton.signalStartTime)
                nStartTime.Value = AppManager.Singleton.signalStartTime;

            if (nDetectionTime.Value != AppManager.Singleton.signalDetectionTime)
                nDetectionTime.Value = AppManager.Singleton.signalDetectionTime;

            if (nSubmitTime.Value != AppManager.Singleton.guessSubmissionTime)
                nSubmitTime.Value = AppManager.Singleton.guessSubmissionTime;

            SetDirection();

            if (nCertaintyOfGuess.Value != AppManager.Singleton.certaintyOfGuess)
                nCertaintyOfGuess.Value = AppManager.Singleton.certaintyOfGuess;
        }
    }

    public void SetDirection()
    {
        Vector2 pointer = inputActions.Standard.Pointer.ReadValue<Vector2>();
        float width = directionIndicator.rectTransform.sizeDelta.x * directionIndicator.rectTransform.lossyScale.x;

        if (Vector3.Distance(directionIndicator.transform.position, pointer) > ((width / 2) + trackZoneBottom)
            && Vector3.Distance(directionIndicator.transform.position, pointer) < ((width / 2) + trackZoneTop)
            && inputActions.Standard.Click.IsPressed())
        {
            if (!directionConfirmButton.gameObject.activeSelf)
            {
                SendDetectionServerRpc();
                directionConfirmButton.gameObject.SetActive(true);
            }
            Vector2 pointerRelative = new Vector2(pointer.x - directionIndicator.transform.position.x, pointer.y - directionIndicator.transform.position.y);
            float angleToCenter = Mathf.Atan2(pointerRelative.y, pointerRelative.x) * Mathf.Rad2Deg;
            angle = (int)(angleToCenter + 360) % 360;
            nGuessedDirection.Value = angle;
            directionIndicator.rectTransform.eulerAngles = Vector3.forward * angle;
        }
    }

    [ServerRpc]
    public void StopActuationServerRpc()
    {
        AppManager.Singleton.peltierController.Pause(true);
    }

    [ServerRpc]
    public void SendDetectionServerRpc()
    {
        AppManager.Singleton.surveyServerManager.SendSignalDetectionClientRpc();
    }

    [ServerRpc]
    public void SendDirectionGuessServerRpc()
    {
        StopActuationServerRpc();
        AppManager.Singleton.guessedDirection = nGuessedDirection.Value;
        AppManager.Singleton.surveyServerManager.SendGuessSubmissionClientRpc();
        AppManager.Singleton.surveyServerManager.DisplayLikertClientRpc();
        AppManager.Singleton.signalDetectionTime = nDetectionTime.Value;
        AppManager.Singleton.guessSubmissionTime = nSubmitTime.Value;
        //erverRpc: A client invoked remote procedure call received by and executed on the server-side !!! CHANGE!!
    }

    [ServerRpc]
    public void SendLikertAnwserServerRpc()
    {
        AppManager.Singleton.certaintyOfGuess = nCertaintyOfGuess.Value;
        AppManager.Singleton.isTrialFinished = true;
        AppManager.Singleton.surveyServerManager.DisplayMessageClientRpc();
    }
}
