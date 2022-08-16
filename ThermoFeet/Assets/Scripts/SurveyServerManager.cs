using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class SurveyServerManager : NetworkBehaviour
{
    public TMP_Text text;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.Singleton.IsServer)
            text.text = "Connected clients: " + NetworkManager.Singleton.ConnectedClientsList.Count;
    }
}
