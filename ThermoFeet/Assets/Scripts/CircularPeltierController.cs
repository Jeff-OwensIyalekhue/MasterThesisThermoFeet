using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CircularPeltierController : MonoBehaviour
{
    #region Variables 
    [Header("Refernces")]
    public ArduinoController arduinoController;
    public Image directionIndicator;
    public TMP_Text angleText;
    public TMP_Text pauseText;
    public TMP_InputField timerInput;
    public TMP_InputField speedInput;

    [Header("Options")]
    public float trackZoneBottom = -10;
    public float trackZoneTop = 11;
    public float intervalTime = 1;

    [Header("Manual")]
    public bool isPaused = false;
    public bool isHeating = false;
    public bool isParallel = true;
    public bool useManualAngle = false;
    [Range(0, 360)]
    public int angle = 0;

    private InputActions inputActions;

    private bool runningIntervalA = false;
    private bool runningIntervalB = false;

    private float timerTime = 0;
    private int prevAngle = 0;
    private bool isAngleChanging = false;
    #endregion

    void Awake()
    {
        inputActions = new InputActions();
        inputActions.Standard.Enable();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (prevAngle != angle)
        {
            prevAngle = angle;
            directionIndicator.rectTransform.eulerAngles = Vector3.forward * angle;
            angleText.text = "" + angle + "°";
        }

        directionalPeltierControl();

        SetTimer();
        SetSpeed();

        if (isPaused || isAngleChanging)
            return;

        if (isParallel)
        {
            TemperaturDirectionParallel(angle);
        }
        else if (!runningIntervalA)
            StartCoroutine(TemperaturDirectionAlternating(angle));
    }

    private void OnDisable()
    {
        TurnAllOff();
    }

    private void Pause(bool p)
    {
        isPaused = p;
        if (isPaused)
        {
            TurnAllOff();
            pauseText.text = "start";
        }
        else
            pauseText.text = "stop";
    }
    public void TogglePause()
    {
        isPaused = !isPaused;
        Pause(isPaused);
    }

    public void SetTimer()
    {
        if (timerInput.text != "")
        {
            Pause(true);
            string timeText = timerInput.text;
            try
            {
                int time = int.Parse(timeText);
            }
            catch (System.FormatException)
            {
                timerInput.text = null;

            }
        }
    }
    private void RunTimer()
    {
        if (timerTime > 0)
        {
            TimedActivation(timerTime);
        }
    }
    public void SetSpeed()
    {
        if (speedInput.text != "")
        {
            string speedText = speedInput.text;
            try
            {
                float speed = float.Parse(speedText);
                if (speed <= 0)
                    speed = 1;
                intervalTime = speed;
            }
            catch (System.FormatException)
            {
                speedInput.text = null;

            }
        }
    }

    public void TurnAllOff()
    {
        StopAllCoroutines();

        arduinoController.PeltierOff("F", "o");
        arduinoController.PeltierOff("B", "-");
        arduinoController.PeltierOff("R", "-");
        arduinoController.PeltierOff("L", "-");

        runningIntervalA = false;
        runningIntervalB = false;
    }

    public void TimedActivation(float time)
    {
        StartCoroutine(_TimedActivation(time));
    }
    IEnumerator _TimedActivation(float time)
    {
        if (time > 0)
        {
            yield return new WaitForSeconds(1);
            time--;
            timerInput.text = "" + time;
            _TimedActivation(time);
        }
        else
        {
            timerInput.text = "" + timerTime;
            TogglePause();
        }
    }

    public void SwitchPolarity()
    {
        TogglePause();
        isHeating = !isHeating;
    }

    public void RandomAngle()
    {
        TurnAllOff();
        angle = Random.Range(0, 360);
        directionIndicator.rectTransform.eulerAngles = Vector3.forward * angle;
    }

    public void directionalPeltierControl()
    {
        Vector2 pointer = inputActions.Standard.Pointer.ReadValue<Vector2>();
        float width = directionIndicator.rectTransform.sizeDelta.x * directionIndicator.rectTransform.lossyScale.x;

        if (Vector3.Distance(directionIndicator.transform.position, pointer) > ((width / 2) + trackZoneBottom)
            && Vector3.Distance(directionIndicator.transform.position, pointer) < ((width / 2) + trackZoneTop)
            && inputActions.Standard.Click.IsPressed())
        {
            if (!isAngleChanging)
            {
                TurnAllOff();
                isAngleChanging = true;
            }
            Vector2 pointerRelative = new Vector2(pointer.x - directionIndicator.transform.position.x, pointer.y - directionIndicator.transform.position.y);
            float angleToCenter = Mathf.Atan2(pointerRelative.y, pointerRelative.x) * Mathf.Rad2Deg;
            angle = (int)(angleToCenter + 360) % 360;
        }
        else if (inputActions.Standard.Click.WasReleasedThisFrame())
            isAngleChanging = false;
    }
    public void TemperaturDirectionParallel(int angle)
    {
        if (runningIntervalA || runningIntervalB)
            return;

        string signal1ID = "o";
        string signal2ID = "o";

        float segmentProgess = ((angle) % 90) / 90f;
        //int t = (int)(segmentProgess*1000);
        //float g = ((float)t)/1000f;
        //Debug.Log("base: "+segmentProgess+" int: "+t+" rounded: "+g);

        if (angle >= 0 && angle < 90)
        {
            signal1ID = "R";
            signal2ID = "F";
        }
        else if (angle >= 90 && angle < 180)
        {
            signal1ID = "F";
            signal2ID = "L";

        }
        else if (angle >= 180 && angle < 270)
        {
            signal1ID = "L";
            signal2ID = "B";

        }
        else if (angle >= 270 && angle < 360)
        {
            signal1ID = "B";
            signal2ID = "R";

        }

        StartCoroutine(TemperaturDirectionParalellA(segmentProgess, signal1ID));
        StartCoroutine(TemperaturDirectionParalellB(segmentProgess, signal2ID));
    }
    IEnumerator TemperaturDirectionParalellA(float time, string id)
    {
        runningIntervalA = true;

        string dummySignal = "-";
        if (time >= 0.5)
            dummySignal = "o";
        if (time == 0)
            dummySignal = "n";

        if (isHeating)
            arduinoController.PeltierOn(id, dummySignal);
        else
            arduinoController.PeltierReverse(id, dummySignal);

        yield return new WaitForSeconds(intervalTime * (1 - time));

        if (time > 0)
        {
            if (time > 0.5)
                dummySignal = "n";
            arduinoController.PeltierOff(id, dummySignal);

            yield return new WaitForSeconds(intervalTime * (time));
        }
        runningIntervalA = false;
    }
    IEnumerator TemperaturDirectionParalellB(float time, string id)
    {
        if (time > 0)
        {
            runningIntervalB = true;

            string dummySignal = "-";
            if (time < 0.5)
                dummySignal = "o";

            if (isHeating)
                arduinoController.PeltierOn(id, dummySignal);
            else
                arduinoController.PeltierReverse(id, dummySignal);

            yield return new WaitForSeconds(intervalTime * (time));

            if (time < 0.5)
                dummySignal = "n";
            arduinoController.PeltierOff(id, dummySignal);

            yield return new WaitForSeconds(intervalTime * (1 - time));

            runningIntervalB = false;
        }
    }
    IEnumerator TemperaturDirectionAlternating(int angle)
    {
        runningIntervalA = true;

        string signal1ID = "o";
        string signal2ID = "o";

        float segmentProgess = ((angle) % 90) / 90f; ;

        if (angle >= 0 && angle < 90)
        {
            signal1ID = "R";
            signal2ID = "F";
        }
        else if (angle >= 90 && angle < 180)
        {
            signal1ID = "F";
            signal2ID = "L";

        }
        else if (angle >= 180 && angle < 270)
        {
            signal1ID = "L";
            signal2ID = "B";

        }
        else if (angle >= 270 && angle < 360)
        {
            signal1ID = "B";
            signal2ID = "R";

        }


        if (isHeating)
            arduinoController.PeltierOn(signal1ID, "-");
        else
            arduinoController.PeltierReverse(signal1ID, "-");

        arduinoController.PeltierOff(signal2ID, "n");

        yield return new WaitForSeconds(intervalTime * (1 - segmentProgess));


        if (segmentProgess > 0)
        {
            arduinoController.PeltierOff(signal1ID, "n");

            if (isHeating)
                arduinoController.PeltierOn(signal2ID, "-");
            else
                arduinoController.PeltierReverse(signal2ID, "-");

            yield return new WaitForSeconds(intervalTime * segmentProgess);
        }

        runningIntervalA = false;
    }
}
