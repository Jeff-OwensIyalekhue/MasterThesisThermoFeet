using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CircularPeltierController : MonoBehaviour
{
    #region Variables 
    [Header("References")]
    public ArduinoController arduinoController;
    public Image directionIndicator;
    public TMP_Text angleText;
    public TMP_Text pauseText;
    public TMP_InputField timerInput;
    public TMP_InputField speedInput;
    public static CircularPeltierController Singleton;
    private InputActions inputActions;

    [Header("Options")]
    public float actuationTime = 1;
    [Range(0f, .9f)]
    public float baseActuation = .5f;
    public float cooldownTime = 0.5f;

    public float timerTime = 0;

    [Header("Options: Actuation Circle")]
    public float trackZoneBottom = -10;
    public float trackZoneTop = 11;
    public int circleOffset = 0;

    [Header("Manual")]
    public bool isPaused = true;
    public bool isPolarityNormal = false;

    public _ActuationType actuationType;

    [Range(0, 360)]
    public int angle = 0;
    private int angleOffseted = 0;


    private bool runningIntervalA = false;
    private bool runningIntervalB = false;
    
    private int prevAngle = 0;
    #endregion

    void Awake()
    {
        if (Singleton == null)
            Singleton = this;
        else
            Destroy(this);

        isPaused = true;
        inputActions = new InputActions();
        inputActions.Standard.Enable();
        angleOffseted = angle + circleOffset;
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

        if (angleOffseted != ((angle + circleOffset) % 360))
            angleOffseted = (angle + circleOffset) % 360;

        if (isPaused)
            return;

        if (actuationType == _ActuationType.Interpolation)
        {
            InterpolationActuation(angleOffseted);
        }
        else if (actuationType == _ActuationType.Direction)
        {
            DirectionActuation(angleOffseted);
        }
        else if (actuationType == _ActuationType.Push)
        {
            PushActuation(angleOffseted);
        }
    }

    private void OnDisable()
    {
        TurnAllOff();
    }

    public void TurnAllOff()
    {
        StopAllCoroutines();
        arduinoController.KillPeltiers();

        runningIntervalA = false;
        runningIntervalB = false;
    }

    #region Pause
    public void StartActuation()
    {
        isPaused = false;
        pauseText.text = "start";
    }

    public void Pause(bool p)
    {
        TurnAllOff();
        isPaused = p;
        if (isPaused)
        {
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
    #endregion
   
    public void SwitchActuationMethod(int value)
    {
        switch (value)
        {
            case 0:
                actuationType = _ActuationType.Direction;
                break;
            case 1:
                actuationType = _ActuationType.Interpolation;
                break;
            case 2:
                actuationType = _ActuationType.Push;
                break;
            default:
                break;
        }
    }

    public void SwitchPolarity()
    {
        Pause(true);
        isPolarityNormal = !isPolarityNormal;
    }

    public void RandomAngle()
    {
        Pause(true);
        angle = Random.Range(0, 360);
        angleOffseted = (angle + circleOffset) % 360;
        directionIndicator.rectTransform.eulerAngles = Vector3.forward * angle;
    }

    #region Peltier Switch Functions
    public void PeltierFrontSwitch(float value)
    {
        if (!isPaused)
            Pause(true);

        if (value == 1f)
        {
            arduinoController.ActuatePeltiers(front: "n");
        }
        else if (value == 0f)
        {
            arduinoController.ActuatePeltiers(front: "r");
        }
        else
        {
            arduinoController.ActuatePeltiers(front: "o");
        }
    }

    public void PeltierBackSwitch(float value)
    {
        if (!isPaused)
            Pause(true);

        if (value == 1f)
        {
            arduinoController.ActuatePeltiers(back: "n");
        }
        else if (value == 0f)
        {
            arduinoController.ActuatePeltiers(back: "r");
        }
        else
        {
            arduinoController.ActuatePeltiers(back: "o");
        }
    }

    public void PeltierLeftSwitch(float value)
    {
        if (!isPaused)
            Pause(true);

        if (value == 1f)
        {
            arduinoController.ActuatePeltiers(left: "n");
        }
        else if (value == 0f)
        {
            arduinoController.ActuatePeltiers(left: "r");
        }
        else
        {
            arduinoController.ActuatePeltiers(left: "o");
        }
    }

    public void PeltierRightSwitch(float value)
    {
        if (!isPaused)
            Pause(true);

        if (value == 1f)
        {
            arduinoController.ActuatePeltiers(right: "n");
        }
        else if (value == 0f)
        {
            arduinoController.ActuatePeltiers(right: "r");
        }
        else
        {
            arduinoController.ActuatePeltiers(right: "o");
        }

    }

    public void PeltierDummySwitch(bool value)
    {
        if (!isPaused)
            Pause(true);

        if (value)
        {
            arduinoController.ActuatePeltiers(dummy: "n");
        }
        else
        {
            arduinoController.ActuatePeltiers(dummy: "o");
        }
    }
    #endregion

    public void SetTimer(string timeText)
    {
        Pause(true);
        try
        {
            int time = int.Parse(timeText);
            timerTime = time;
        }
        catch (System.FormatException)
        {
            timerInput.text = null;
        }
    }

    public void SetSpeed(string speedText)
    {
        try
        {
            float speed = float.Parse(speedText);
            if (speed <= 0)
                speed = 1;
            actuationTime = speed;
        }
        catch (System.FormatException)
        {
            speedInput.text = null;
        }
    }

    public void directionalPeltierControl()
    {
        Vector2 pointer = inputActions.Standard.Pointer.ReadValue<Vector2>();
        float width = directionIndicator.rectTransform.sizeDelta.x * directionIndicator.rectTransform.lossyScale.x;

        if (Vector3.Distance(directionIndicator.transform.position, pointer) > ((width / 2) + trackZoneBottom)
            && Vector3.Distance(directionIndicator.transform.position, pointer) < ((width / 2) + trackZoneTop)
            && inputActions.Standard.Click.IsPressed())
        {
            if (!isPaused)
            {
                Pause(true);
            }
            Vector2 pointerRelative = new Vector2(pointer.x - directionIndicator.transform.position.x, pointer.y - directionIndicator.transform.position.y);
            float angleToCenter = Mathf.Atan2(pointerRelative.y, pointerRelative.x) * Mathf.Rad2Deg;
            angle = (int)(angleToCenter + 360) % 360;
            angleOffseted = (angle + circleOffset) % 360;
        }
    }

    #region DirectionActuation
    public void DirectionActuation(int angle)
    {
        if (runningIntervalA)
            return;
        StartCoroutine(_DirectionActuation(angle));
    }
    IEnumerator _DirectionActuation(int angle)
    {
        runningIntervalA = true;
        string signal = "-";

        if (isPolarityNormal)
            signal = "n";
        else
            signal = "r";


        if (angle == 0 || angle == 360)
        {
            arduinoController.ActuatePeltiers(right: signal, dummy: "n");
        }
        else if (angle > 0 && angle < 90)
        {
            arduinoController.ActuatePeltiers(right: signal, front: signal);
        }
        else if (angle == 90)
        {
            arduinoController.ActuatePeltiers(front: signal, dummy: "n");
        }
        else if (angle > 90 && angle < 180)
        {
            arduinoController.ActuatePeltiers(front: signal, left: signal);
        }
        else if (angle == 180)
        {
            arduinoController.ActuatePeltiers(left: signal, dummy: "n");
        }
        else if (angle > 180 && angle < 270)
        {
            arduinoController.ActuatePeltiers(left: signal, back: signal);
        }
        else if (angle == 270)
        {
            arduinoController.ActuatePeltiers(back: signal, dummy: "n");
        }
        else if (angle > 270 && angle < 360)
        {
            arduinoController.ActuatePeltiers(right: signal, back: signal);
        }

        yield return new WaitForSeconds(actuationTime);

        arduinoController.KillPeltiers();
        yield return new WaitForSeconds(cooldownTime);
        runningIntervalA = false;
    }
    #endregion

    #region InterpolationActuation
    public void InterpolationActuation(int angle)
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

        StartCoroutine(_InterpolationActuationA(segmentProgess, signal1ID));
        StartCoroutine(_InterpolationActuationB(segmentProgess, signal2ID));
    }
    IEnumerator _InterpolationActuationA(float time, string id)
    {
        runningIntervalA = true;

        float intervalFirstPart = actuationTime * baseActuation;
        float intervalSecondPart = actuationTime - intervalFirstPart;

        string dummySignal = "-";
        if (time >= 0.5)
            dummySignal = "o";
        if (time == 0)
            dummySignal = "n";

        if (isPolarityNormal)
            arduinoController.ActuatePeltierIdNormal(id, dummySignal);
        else
            arduinoController.ActuatePeltierIdReverse(id, dummySignal);

        yield return new WaitForSeconds(intervalFirstPart);
        yield return new WaitForSeconds(intervalSecondPart * (1 - time));

        if (time > 0)
        {
            if (time > 0.5)
                dummySignal = "n";
            arduinoController.ActuatePelterIdOff(id, dummySignal);

            yield return new WaitForSeconds(intervalSecondPart * (time));
        }

        arduinoController.ActuatePelterIdOff(id, "o");
        yield return new WaitForSeconds(cooldownTime);
        runningIntervalA = false;
    }
    IEnumerator _InterpolationActuationB(float time, string id)
    {
        if (time > 0)
        {
            runningIntervalB = true;

            float intervalFirstPart = actuationTime * baseActuation;
            float intervalSecondPart = actuationTime - intervalFirstPart;

            string dummySignal = "-";
            if (time < 0.5)
                dummySignal = "o";

            if (isPolarityNormal)
                arduinoController.ActuatePeltierIdNormal(id, dummySignal);
            else
                arduinoController.ActuatePeltierIdReverse(id, dummySignal);

            yield return new WaitForSeconds(intervalFirstPart);
            yield return new WaitForSeconds(intervalSecondPart * (time));

            if (time < 0.5)
                dummySignal = "n";
            arduinoController.ActuatePelterIdOff(id, dummySignal);

            yield return new WaitForSeconds(intervalSecondPart * (1 - time));

            arduinoController.ActuatePelterIdOff(id, "o");
            yield return new WaitForSeconds(cooldownTime);
            runningIntervalB = false;
        }
    }
    #endregion

    #region PushActuation
    public void PushActuation(int angle)
    {
        if (runningIntervalA)
            return;
        StartCoroutine(_PushActuation(angle));
    }
    IEnumerator _PushActuation(int angle)
    {
        runningIntervalA = true;
        string signal = "-";
        string signalRev = "-";

        if (isPolarityNormal)
        {
            signal = "n";
            signalRev = "r";
        }
        else
        {
            signal = "r";
            signalRev = "n";
        }


        if (angle == 0 || angle == 360)
        {
            arduinoController.ActuatePeltiers(left: signalRev, dummy: "n");
            yield return new WaitForSeconds(actuationTime / 2);
            arduinoController.KillPeltiers();
            arduinoController.ActuatePeltiers(right: signal, dummy: "n");
        }
        else if (angle > 0 && angle < 90)
        {
            arduinoController.ActuatePeltiers(left: signalRev, back: signalRev);
            yield return new WaitForSeconds(actuationTime / 2);
            arduinoController.KillPeltiers();
            arduinoController.ActuatePeltiers(right: signal, front: signal);
        }
        else if (angle == 90)
        {
            arduinoController.ActuatePeltiers(back: signalRev, dummy: "n");
            yield return new WaitForSeconds(actuationTime / 2);
            arduinoController.KillPeltiers();
            arduinoController.ActuatePeltiers(front: signal, dummy: "n");
        }
        else if (angle > 90 && angle < 180)
        {
            arduinoController.ActuatePeltiers(back: signalRev, right: signalRev);
            yield return new WaitForSeconds(actuationTime / 2);
            arduinoController.KillPeltiers();
            arduinoController.ActuatePeltiers(front: signal, left: signal);
        }
        else if (angle == 180)
        {
            arduinoController.ActuatePeltiers(right: signalRev, dummy: "n");
            yield return new WaitForSeconds(actuationTime / 2);
            arduinoController.KillPeltiers();
            arduinoController.ActuatePeltiers(left: signal, dummy: "n");
        }
        else if (angle > 180 && angle < 270)
        {
            arduinoController.ActuatePeltiers(right: signalRev, front: signalRev);
            yield return new WaitForSeconds(actuationTime / 2);
            arduinoController.KillPeltiers();
            arduinoController.ActuatePeltiers(left: signal, back: signal);
        }
        else if (angle == 270)
        {
            arduinoController.ActuatePeltiers(front: signalRev, dummy: "n");
            yield return new WaitForSeconds(actuationTime / 2);
            arduinoController.KillPeltiers();
            arduinoController.ActuatePeltiers(back: signal, dummy: "n");
        }
        else if (angle > 270 && angle < 360)
        {
            arduinoController.ActuatePeltiers(left: signalRev, front: signalRev);
            yield return new WaitForSeconds(actuationTime / 2);
            arduinoController.KillPeltiers();
            arduinoController.ActuatePeltiers(right: signal, back: signal);
        }

        yield return new WaitForSeconds(actuationTime/2);

        arduinoController.KillPeltiers();
        yield return new WaitForSeconds(cooldownTime);
        runningIntervalA = false;
    }
    #endregion

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


        if (isPolarityNormal)
            arduinoController.ActuatePeltierIdNormal(signal1ID, "-");
        else
            arduinoController.ActuatePeltierIdReverse(signal1ID, "-");

        arduinoController.ActuatePelterIdOff(signal2ID, "n");

        yield return new WaitForSeconds(actuationTime * (1 - segmentProgess));


        if (segmentProgess > 0)
        {
            arduinoController.ActuatePelterIdOff(signal1ID, "n");

            if (isPolarityNormal)
                arduinoController.ActuatePeltierIdNormal(signal2ID, "-");
            else
                arduinoController.ActuatePeltierIdReverse(signal2ID, "-");

            yield return new WaitForSeconds(actuationTime * segmentProgess);
        }

        runningIntervalA = false;
    }
}
