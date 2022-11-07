using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DirectionAnswerButtons : MonoBehaviour
{
    public Button[] buttons;
    public int[] directions = { 0, 45, 90, 135, 180, 225, 275, 315 };
    private Button activeDirections;

    private void OnEnable()
    {
        if (activeDirections != null)
        {
            activeDirections.image.color = Color.white;
            activeDirections.interactable = true;
        }
    }

    public void SelectDirection(int id)
    {
        AppManager.Singleton.surveyClientManager.SetDirection(directions[id]);
        buttons[id].image.color = Color.green;
        buttons[id].interactable = false;
        if (activeDirections != null && activeDirections != buttons[id])
        {
            activeDirections.image.color = Color.white;
            activeDirections.interactable = true;
        }
        activeDirections = buttons[id];
    }
}
