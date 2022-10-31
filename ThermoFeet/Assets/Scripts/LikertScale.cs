using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LikertScale : MonoBehaviour
{
    public List<Toggle> toggles = new List<Toggle>();

    void Awake()
    {
        //for (int i = 0; i < toggles.Count; i++)
        //{
        //    toggles[i].onValueChanged.AddListener((bool isOn) =>
        //    {
        //        SetAnswer(i);
        //    });
        //}
        toggles[2].isOn = true;
        SetAnswer(2);
    }

    public void SetAnswer(int i)
    {
        if (toggles[i].isOn)
            AppManager.Singleton.certaintyOfGuess = i;
        //Debug.Log("likert" + i);

    }
}
