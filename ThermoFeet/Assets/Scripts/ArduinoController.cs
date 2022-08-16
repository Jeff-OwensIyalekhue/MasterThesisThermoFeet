#if !UNITY_ANDROID
using System.IO.Ports;
#else
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArduinoController : MonoBehaviour
{
    [Header("Serial Connection")]
    public string portName = "COM4";
    public int baudRate = 9600;//115200;
    public TMP_Dropdown dropdownSerialPorts;
#if !UNITY_ANDROID
    private SerialPort serialPort;
#else
#endif
    private string serialInput;

    [Header("Settings")]
    public bool useVisualization = false;

    [Header("UI References")]
    public Image frontVis;
    public Image backVis;
    public Image rightVis;
    public Image leftVis;
    public Image dummyVis;

    public GameObject serialUI;

    public GameObject modularController;
    public GameObject directionalController;

#if !UNITY_ANDROID
    void Start()
    {
        useVisualization = (frontVis != null || backVis != null || rightVis != null || leftVis != null || dummyVis != null);
    }
#else
    void Start()
    {
        serialUI.SetActive(false);
        useVisualization = (frontVis != null || backVis != null || rightVis != null || leftVis != null || dummyVis != null);
    }
#endif

#if !UNITY_ANDROID
    void Update()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            //if (serialUI.activeSelf)
            //    serialUI.SetActive(false);

            serialInput = SerialReadLine();
            if (useVisualization)
                PeltierStatusVis(serialInput);
        }
    }
#else
#endif
    private void OnApplicationQuit()
    {
        StopAllCoroutines();
        PeltierOff("F", "o");
        PeltierOff("B", "-");
        PeltierOff("R", "-");
        PeltierOff("L", "-");

#if !UNITY_ANDROID
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            serialPort.Dispose();
        }
#endif
    }

#if !UNITY_ANDROID
    #region serial connection
    public void GetSerialPorts()
    {
        dropdownSerialPorts.ClearOptions();
        List<TMP_Dropdown.OptionData> optionList = new List<TMP_Dropdown.OptionData>();

        TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData();
        option.text = "none";
        optionList.Add(option);

        string[] portList = SerialPort.GetPortNames();
        foreach (string port in portList)
        {
            option = new TMP_Dropdown.OptionData();
            option.text = port;
            optionList.Add(option);
        }
        dropdownSerialPorts.AddOptions(optionList);
    }
    public void SetSerialPort(int value)
    {
        portName = dropdownSerialPorts.options[value].text;
    }
    public void ConnectToPort()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            serialPort.Dispose();
        }

        try
        {
            serialPort = new SerialPort(portName, baudRate);
            if (!serialPort.IsOpen)
            {
                serialPort.ReadTimeout = 100;
                serialPort.Handshake = Handshake.None;
                serialPort.Open();

                PeltierOff("F", "o");
                PeltierOff("B", "-");
                PeltierOff("R", "-");
                PeltierOff("L", "-");
            }
        }
        catch (System.IO.IOException)
        {
            return;
        }
    }
    public string SerialReadLine()
    {
        try
        {
            return serialPort.ReadLine();
        }
        catch (System.TimeoutException)
        {
            return null;
        }
    }
    #endregion
#else
#endif
    public void SwitchController(int mode)
    {
        StopAllCoroutines();
        PeltierOff("F", "o");
        PeltierOff("B", "-");
        PeltierOff("R", "-");
        PeltierOff("L", "-");

        switch (mode)
        {
            case 0:
                modularController.SetActive(true);
                directionalController.SetActive(false);
                break;
            case 1:
                modularController.SetActive(false);
                directionalController.SetActive(true);
                break;
            default:
                modularController.SetActive(true);
                directionalController.SetActive(false);
                break;
        }
    }
    private void PeltierStatusVis(string signal)
    {
#if !UNITY_ANDROID
        if (serialPort != null && serialPort.IsOpen)
        {
            switch (signal)
            {
                case "0 is on":
                    frontVis.color = Color.red;
                    break;
                case "0 is off":
                    frontVis.color = Color.white;
                    break;
                case "0 is reversed":
                    frontVis.color = Color.blue;
                    break;
                case "1 is on":
                    backVis.color = Color.red;
                    break;
                case "1 is off":
                    backVis.color = Color.white;
                    break;
                case "1 is reversed":
                    backVis.color = Color.blue;
                    break;
                case "2 is on":
                    rightVis.color = Color.red;
                    break;
                case "2 is off":
                    rightVis.color = Color.white;
                    break;
                case "2 is reversed":
                    rightVis.color = Color.blue;
                    break;
                case "3 is on":
                    leftVis.color = Color.red;
                    break;
                case "3 is off":
                    leftVis.color = Color.white;
                    break;
                case "3 is reversed":
                    leftVis.color = Color.blue;
                    break;
                case "5 is on":
                    dummyVis.color = Color.green;
                    break;
                case "5 is off":
                    dummyVis.color = Color.gray;
                    break;
                default:
                    break;
            }
        }
        else
#else
        if (true)
#endif
        {
            string pCommand = signal.Substring(0, 2);
            string dCommand = signal.Substring(2, 1);
            switch (pCommand)
            {
                case "Fn":
                    frontVis.color = Color.red;
                    break;
                case "Fo":
                    frontVis.color = Color.white;
                    break;
                case "Fr":
                    frontVis.color = Color.blue;
                    break;
                case "Bn":
                    backVis.color = Color.red;
                    break;
                case "Bo":
                    backVis.color = Color.white;
                    break;
                case "Br":
                    backVis.color = Color.blue;
                    break;
                case "Rn":
                    rightVis.color = Color.red;
                    break;
                case "Ro":
                    rightVis.color = Color.white;
                    break;
                case "Rr":
                    rightVis.color = Color.blue;
                    break;
                case "Ln":
                    leftVis.color = Color.red;
                    break;
                case "Lo":
                    leftVis.color = Color.white;
                    break;
                case "Lr":
                    leftVis.color = Color.blue;
                    break;
                default:
                    break;
            }
            switch (dCommand)
            {
                case "n":
                    dummyVis.color = Color.green;
                    break;
                case "o":
                    dummyVis.color = Color.gray;
                    break;
                default:
                    break;
            }
        }
    }

    #region Peltier Switch Functions
    public void PeltierFrontSwitch(float value)
    {
        if (value == 1f)
        {
            PeltierOn("F", "-");
        }
        else if (value == 0f)
        {
            PeltierReverse("F", "-");
        }
        else
        {
            PeltierOff("F", "-");
        }
    }

    public void PeltierBackSwitch(float value)
    {
        if (value == 1f)
        {
            PeltierOn("B", "-");
        }
        else if (value == 0f)
        {
            PeltierReverse("B", "-");
        }
        else
        {
            PeltierOff("B", "-");
        }
    }

    public void PeltierLeftSwitch(float value)
    {
        if (value == 1f)
        {
            PeltierOn("L", "-");
        }
        else if (value == 0f)
        {
            PeltierReverse("L", "-");
        }
        else
        {
            PeltierOff("L", "-");
        }
    }

    public void PeltierRightSwitch(float value)
    {
        if (value == 1f)
        {
            PeltierOn("R", "-");
        }
        else if (value == 0f)
        {
            PeltierReverse("R", "-");
        }
        else
        {
            PeltierOff("R", "-");
        }
    }
    #endregion
    #region Peltier Base Functions
    public void PeltierOn(string id, string dummy)
    {
        string signal = id + "n" + dummy + "\n";
#if !UNITY_ANDROID
        if (serialPort!= null && serialPort.IsOpen)
        {
            serialPort.Write(signal);
            serialPort.BaseStream.Flush();
        }
        else
#endif
            if (useVisualization)
            PeltierStatusVis(signal);
    }

    public void PeltierOff(string id, string dummy)
    {
        string signal = id + "o" + dummy + "\n";
#if !UNITY_ANDROID
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Write(signal);
            serialPort.BaseStream.Flush();
        }
        else
#endif
            if (useVisualization)
            PeltierStatusVis(signal);
    }

    public void PeltierReverse(string id, string dummy)
    {
        string signal = id + "r" + dummy + "\n";
#if !UNITY_ANDROID
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Write(signal);
            serialPort.BaseStream.Flush();
        }
        else
#endif
            if (useVisualization)
            PeltierStatusVis(signal);
    }
    #endregion
}