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
    public NetworkVariable<int> nAngle = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> nLikertAnswer = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> nStartTime = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> nEndTime = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);

    private int angle;
    private InputActions inputActions;

    void Awake()
    {
        inputActions = new InputActions();
        inputActions.Standard.Enable();

        directionIndicator = AppManager.Singleton.directionIndicator;

        directionConfirmButton = AppManager.Singleton.directionConfirmationButton;
        directionConfirmButton.onClick.AddListener(() =>
        {
            SendDirectionGuessServerRpc();
            nEndTime.Value = 0;
        });

        likertConfirmButton = AppManager.Singleton.likertConfirmationButton;
        likertConfirmButton.onClick.AddListener(() => { SendLikertAnwserServerRpc(); });

        nEndTime.Value = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            if (nStartTime.Value != AppManager.Singleton.signalStartTime)
                nStartTime.Value = AppManager.Singleton.signalStartTime;

            SetDirection();

            if (nLikertAnswer.Value != AppManager.Singleton.certaintyOfGuess)
                nLikertAnswer.Value = AppManager.Singleton.certaintyOfGuess;
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
            if (nEndTime.Value == 0)
            {
                StopActuationServerRpc();
                nEndTime.Value = Time.time;
            }
            Vector2 pointerRelative = new Vector2(pointer.x - directionIndicator.transform.position.x, pointer.y - directionIndicator.transform.position.y);
            float angleToCenter = Mathf.Atan2(pointerRelative.y, pointerRelative.x) * Mathf.Rad2Deg;
            angle = (int)(angleToCenter + 360) % 360;
            nAngle.Value = angle;
            directionIndicator.rectTransform.eulerAngles = Vector3.forward * angle;
        }
    }

    [ServerRpc]
    public void StopActuationServerRpc()
    {
        AppManager.Singleton.peltierController.Pause(true);
    }

    [ServerRpc]
    public void SendDirectionGuessServerRpc()
    {
        StopActuationServerRpc();
        AppManager.Singleton.guessedDirection = nAngle.Value;
        AppManager.Singleton.signalDetectionTime = nEndTime.Value - nStartTime.Value;
        AppManager.Singleton.surveyServerManager.DisplayLikertClientRpc();
    }

    [ServerRpc]
    public void SendLikertAnwserServerRpc()
    {
        AppManager.Singleton.certaintyOfGuess = nLikertAnswer.Value;
        AppManager.Singleton.isTrialFinished = true;
        AppManager.Singleton.surveyServerManager.DisplayMessageClientRpc();
    }
}
