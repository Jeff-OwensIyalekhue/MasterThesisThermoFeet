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
    public bool usingVisualization = false;

    [Header("UI References")]
    public Image frontVis;
    public Image backVis;
    public Image rightVis;
    public Image leftVis;
    public Image dummyVis;

    public GameObject serialUI;

#if !UNITY_ANDROID
    void Start()
    {
        usingVisualization = (frontVis != null || backVis != null || rightVis != null || leftVis != null || dummyVis != null);

        GetSerialPorts();
        if (dropdownSerialPorts.options.Count > 1)
        {
            SetSerialPort(1);
            dropdownSerialPorts.value = 1;
            ConnectToPort();
        }
        else
            SetSerialPort(0);
    }
#else
    void Start()
    {
        serialUI.SetActive(false);
        usingVisualization = (frontVis != null || backVis != null || rightVis != null || leftVis != null || dummyVis != null);
    }
#endif

#if !UNITY_ANDROID
    void Update()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialInput = ReadSerialLine();
            if (usingVisualization)
                VisualizePeltierStatus(serialInput);
        }
    }
#else
#endif
    private void OnApplicationQuit()
    {
        StopAllCoroutines();
        KillPeltiers();

#if !UNITY_ANDROID
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            serialPort.Dispose();
        }
#endif
    }

    #region serial connection
#if !UNITY_ANDROID
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

                KillPeltiers();
            }
        }
        catch (System.IO.IOException)
        {
            return;
        }
    }
    public string ReadSerialLine()
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
#else
#endif
    #endregion

    private void VisualizePeltierStatus(string signal)
    {
#if !UNITY_ANDROID
        if (frontVis == null || backVis == null || rightVis == null || leftVis == null)
            return;

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
        {
            Color c;
            for (int i = 0; i < signal.Length; i++)
            {
                // get current status
                switch (i)
                {
                    case 0:
                        c = frontVis.color;
                        break;
                    case 1:
                        c = backVis.color;
                        break;
                    case 2:
                        c = rightVis.color;
                        break;
                    case 3:
                        c = leftVis.color;
                        break;
                    case 4:
                        c = dummyVis.color;
                        break;
                    default:
                        c = Color.yellow;
                        break;
                }

                // select colors according to current signal
                if (i == signal.Length - 1)
                {
                    switch (signal)
                    {
                        case "n":
                            c = Color.green;
                            break;
                        case "o":
                            c = Color.gray;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (signal[i])
                    {
                        case 'n':
                            c = Color.red;
                            break;
                        case 'o':
                            c = Color.white;
                            break;
                        case 'r':
                            c = Color.blue;
                            break;
                        default:
                            break;
                    }
                }

                // change colors
                switch (i)
                {
                    case 0:
                        frontVis.color = c;
                        break;
                    case 1:
                        backVis.color = c;
                        break;
                    case 2:
                        rightVis.color = c;
                        break;
                    case 3:
                        leftVis.color = c;
                        break;
                    case 4:
                        dummyVis.color = c;
                        break;
                    default:
                        break;
                }
            }
        }
#endif
    }

    #region Peltier Base Functions
    /** Describtion
     * send a serial message to the arduino to change the current flow of the curuit of the ThermalFee(t/d)back
     * "-": don't change current flow
     * "n": let the current flow
     * "r": reverse the current flow
     * "o": break the current flow
     */
    public void ActuatePeltiers(string front = "-", string back = "-", string right = "-", string left = "-", string dummy = "-")
    {
        string signal = front + back + right + left + dummy + "\n";
#if !UNITY_ANDROID
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Write(signal);
            serialPort.BaseStream.Flush();
        }
        else
#endif
            if (usingVisualization)
            VisualizePeltierStatus(signal);
    }

    public void KillPeltiers()
    {
        ActuatePeltiers("o", "o", "o", "o", "o");
    }

    public void ActuatePeltierIdNormal(string id, string hookedDummy = "-")
    {
        switch (id)
        {
            case "F":
                ActuatePeltiers(front: "n", dummy: hookedDummy);
                break;
            case "B":
                ActuatePeltiers(back: "n", dummy: hookedDummy);
                break;
            case "R":
                ActuatePeltiers(right: "n", dummy: hookedDummy);
                break;
            case "L":
                ActuatePeltiers(left: "n", dummy: hookedDummy);
                break;
            default:
                break;
        }
    }
    public void ActuatePelterIdOff(string id, string hookedDummy = "-")
    {
        switch (id)
        {
            case "F":
                ActuatePeltiers(front: "o", dummy: hookedDummy);
                break;
            case "B":
                ActuatePeltiers(back: "o", dummy: hookedDummy);
                break;
            case "R":
                ActuatePeltiers(right: "o", dummy: hookedDummy);
                break;
            case "L":
                ActuatePeltiers(left: "o", dummy: hookedDummy);
                break;
            default:
                break;
        }
    }
    public void ActuatePeltierIdReverse(string id, string hookedDummy = "-")
    {
        switch (id)
        {
            case "F":
                ActuatePeltiers(front: "r", dummy: hookedDummy);
                break;
            case "B":
                ActuatePeltiers(back: "r", dummy: hookedDummy);
                break;
            case "R":
                ActuatePeltiers(right: "r", dummy: hookedDummy);
                break;
            case "L":
                ActuatePeltiers(left: "r", dummy: hookedDummy);
                break;
            default:
                break;
        }
    }

    #endregion
}