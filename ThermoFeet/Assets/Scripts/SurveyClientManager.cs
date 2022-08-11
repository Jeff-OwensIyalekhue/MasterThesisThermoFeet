using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class SurveyClientManager : NetworkBehaviour
{
    private InputActions inputActions;
    public Image directionIndicator;
    public int angle;
    //public NetworkVariable<int> nAngle;

    public float trackZoneBottom = -10;
    public float trackZoneTop = 11;

    public GameObject uiHolder;

    void Awake()
    {
        inputActions = new InputActions();
        inputActions.Standard.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            SetDirection();
        }
        else
        {
            if (uiHolder.activeSelf)
                uiHolder.SetActive(false);
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
            Vector2 pointerRelative = new Vector2(pointer.x - directionIndicator.transform.position.x, pointer.y - directionIndicator.transform.position.y);
            float angleToCenter = Mathf.Atan2(pointerRelative.y, pointerRelative.x) * Mathf.Rad2Deg;
            angle = (int)(angleToCenter + 360) % 360;
            //nAngle.Value = angle;
            directionIndicator.rectTransform.eulerAngles = Vector3.forward * angle;
        }
    }

    public void ClientCon()
    {
        if (IsOwner && !IsClient)
            NetworkManager.Singleton.StartClient();
    }
}
