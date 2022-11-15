using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using JetBrains.Annotations;

public class ThermalFeedbackManager : MonoBehaviour
{
    [Header("Values")]
    public ParticipantData participant;
    public TrialDiections trialDirections = new TrialDiections();
    public int timeBetweenTrials = 10;
    public float rangeMaxStartTime = 10;
    public int breakTimeBetweenTrials = 30;
    public float maxActuationTime = 20;
    public bool isTrialRunning = false;
    public bool isGroupRunning = false;
    private int trialsIter = 0;
    private Coroutine coroutineMaxActuation;

    [Header("References")]
    public TMP_Dropdown actuationMethodDropdown;
    public TMP_Text trialInformationText;
    public TMP_Text sessionInfoText;
    public TMP_Text actuationInfoText;
    public TMP_InputField savePathText;

    [SerializeField] private string pathToSaveLocation = "C:/Users/Jeff-Owens/Desktop";

    private void Start()
    {
#if !UNITY_ANDROID
        LoadStudyParams();
#endif
    }

    private void Awake()
    {
        actuationMethodDropdown.onValueChanged.AddListener((int value) => { SetActuationType(value); });
        sessionInfoText.text = "P.-ID " + participant.id + "\nS-Dir: " + trialDirections.trialNumber;
        string s = "trails done:\n";
        if (trialsIter < 4)
        {
            if (participant.actuationType == _ActuationType.DirectionHot)
                s += "hot: " + trialsIter + "/4\ncold: 0/4";
            else
                s += "cold: " + trialsIter + "/4\nhot: 0/4";
        }
        else
        {
            int i = trialsIter % 4;

            if (participant.actuationType == _ActuationType.DirectionHot)
                s += "cold: 4/4\nhot: " + i + "/4";
            else
                s += "hot: 4/4\ncold: " + i + "/4";
        }
        actuationInfoText.text = s;
        savePathText.text = pathToSaveLocation;
    }

    void Update()
    {
        if (isTrialRunning && AppManager.Singleton.isTrialFinished)
        {
            int i = participant.trials.Count - 1;
            participant.trials[i].detectionTime = AppManager.Singleton.surveyClientManager.nDetectionTime.Value;
            participant.trials[i].submissionTime = AppManager.Singleton.surveyClientManager.nSubmitTime.Value;
            participant.trials[i].directionGuessed = AppManager.Singleton.guessedDirection;
            participant.trials[i].certaintyOfGuess = AppManager.Singleton.certaintyOfGuess;
            isTrialRunning = false;
        }
    }

    #region Set Functions
    public void SetSavePath(string path)
    {
        string tmp = pathToSaveLocation;
        pathToSaveLocation = path;
        if (!Directory.Exists(path))
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                pathToSaveLocation = tmp;
                savePathText.text = pathToSaveLocation;
                Debug.Log("The process failed: " + e.ToString());
            }
            finally { }
        }
    }

    public void SetParticpantId(string value)
    {
        participant.id = int.Parse(value);
        sessionInfoText.text = "P.-ID " + participant.id + "\nS-Dir: " + trialDirections.trialNumber;
    }
    public void SetTrialsDirections(string value)
    {
        trialDirections.trialNumber = int.Parse(value);
        sessionInfoText.text = "P.-ID " + participant.id + "\nS-Dir: " + trialDirections.trialNumber;
    }
    public void SetTrialIter(string value)
    {
        trialsIter = int.Parse(value);
        string s = "trails done:\n";
        if (trialsIter < 4)
        {
            s += "hot: " + trialsIter + "/4\ncold: 0/4";
        }
        else
        {
            int i = trialsIter % 4;

            s += "hot: 4/4\ncold: " + i + "/4";
        }
        actuationInfoText.text = s;
    }

    public void SetActuationType(int value)
    {
        switch (value)
        {
            case 0:
                participant.actuationType = _ActuationType.DirectionHot;
                break;
            case 1:
                participant.actuationType = _ActuationType.DirectionCold;
                break;
            case 2:
                participant.actuationType = _ActuationType.Interpolation;
                break;
            case 3:
                participant.actuationType = _ActuationType.PushCH;
                break;
            case 4:
                participant.actuationType = _ActuationType.Push;
                break;
            default:
                break;
        }

    }
    #endregion

    public void ShowURL()
    {
        AppManager.Singleton.surveyServerManager.OpenQuestionnaireClientRpc();
    }

    public void StartSingleTrial(int direction)
    {
        StartCoroutine(RunSingleTrial(direction));
    }
    IEnumerator RunSingleTrial(int direction)
    {
        if (coroutineMaxActuation != null)
            StopCoroutine(coroutineMaxActuation);

        if (isTrialRunning)
            yield return null;

        AppManager.Singleton.isTrialFinished = false;
        isTrialRunning = true;
        AppManager.Singleton.peltierController.actuationType = participant.actuationType;
        AppManager.Singleton.peltierController.angle = direction;

        int trialID = participant.trials.Count;
        participant.trials.Add(new TrialInformation(trialID, direction));

        AppManager.Singleton.surveyServerManager.DisplayDirectionClientRpc();

        float t = UnityEngine.Random.Range(0, rangeMaxStartTime);
        yield return new WaitForSeconds(t);

        AppManager.Singleton.surveyServerManager.SendSignalStartClientRpc();
        AppManager.Singleton.peltierController.StartActuation();
        coroutineMaxActuation = StartCoroutine(MaximumActuationStop());
    }

    IEnumerator MaximumActuationStop()
    {
        yield return new WaitForSeconds(maxActuationTime);
        AppManager.Singleton.peltierController.Pause(true);
    }

    public void CompleteStudyRun()
    {
        if (isGroupRunning)
        {
            StartCoroutine(WaitBetweenTrials());
        }
        else
        {
            isGroupRunning = true;
            InitTrails();
        }
    }
    IEnumerator WaitBetweenTrials()
    {
        for (int t = breakTimeBetweenTrials; t > 0; t--)
        {
            string msg = "New trials will start in: " + t + " seconds";
            AppManager.Singleton.surveyServerManager.DisplayMessageClientRpc(msg);
            yield return new WaitForSeconds(1);
        }
        InitTrails();
    }

    public void ForcePause()
    {
        StopAllCoroutines();
        isTrialRunning = false;
        isGroupRunning = false;
        AppManager.Singleton.peltierController.Pause(true);
        AppManager.Singleton.surveyServerManager.DisplayMessageClientRpc();
    }

    public void InitTrails()
    {
        List<int> t = new List<int>();

        foreach (int e in trialDirections.getTrialDirections())
        {
            t.Add(e);
        }

        participant.trials.Clear();

        RecusiveTrials(participant.actuationType, t);
    }
    public void RecusiveTrials(_ActuationType actuationType, List<int> directionList)
    {
        if (directionList.Count <= 0)
        {
#if !UNITY_ANDROID
            StudyLogging();
#endif
            trialInformationText.text = "start trial group";
            trialsIter++;
            trialDirections.incrementTrialNumber();
            if (trialsIter == 4)
            {
                if (participant.actuationType == _ActuationType.DirectionHot)
                    participant.actuationType = _ActuationType.DirectionCold;
                else
                    participant.actuationType = _ActuationType.DirectionHot;
            }
            if (trialsIter == 8)
            {
                participant.id++;
                sessionInfoText.text = "P.-ID " + participant.id + "\nS-Dir: " + trialDirections.trialNumber;
                trialsIter = 0;
                isGroupRunning = false;
            }

            string s = "trails done:\n";
            if (trialsIter < 4)
            {
                s += "hot: " + trialsIter + "/4\ncold: 0/4";
            }
            else
            {
                int i = trialsIter % 4;

                s += "hot: 4/4\ncold: " + i + "/4";
            }
            actuationInfoText.text = s;
            AppManager.Singleton.surveyServerManager.DisplayMessageClientRpc("This was the last trial for this group.");

            if (isGroupRunning && trialsIter <= 7)
            {
                CompleteStudyRun();
            }

            return;
        }

        participant.actuationType = actuationType;
        StartCoroutine(TrialsCoroutine(directionList));
    }
    IEnumerator TrialsCoroutine(List<int> directionsLeft)
    {
        int trialCount = 8 - directionsLeft.Count + 1;
        trialInformationText.text = "Trial " + trialCount + "/" + 8;

        for (int t = timeBetweenTrials; t > 0; t--)
        {
            string msg = "A new Signal will come in: " + t + " seconds";
            AppManager.Singleton.surveyServerManager.DisplayMessageClientRpc(msg);
            yield return new WaitForSeconds(1);
        }

        StartCoroutine(RunSingleTrial(directionsLeft[0]));

        yield return new WaitWhile(() => { return isTrialRunning; });

        directionsLeft.RemoveAt(0);

        RecusiveTrials(participant.actuationType, directionsLeft);
    }

    #region LogFile Functions
#if !UNITY_ANDROID
    public void SaveStudyParams()
    {
        SessionData data = new SessionData(participant.id, pathToSaveLocation, trialDirections);
        BinaryFormatter bF = new BinaryFormatter();
        FileStream file = File.Create(pathToSaveLocation + "/sessionInfo.dat");

        bF.Serialize(file, data);
        file.Close();
    }

    public void LoadStudyParams()
    {
        if (File.Exists(pathToSaveLocation + "/sessionInfo.dat"))
        {
            BinaryFormatter bF = new BinaryFormatter();
            FileStream file = File.Open(pathToSaveLocation + "/sessionInfo.dat", FileMode.Open);
            SessionData data = (SessionData)bF.Deserialize(file);
            file.Close();

            participant.id = data.currentParticipant;
            pathToSaveLocation = data.savePath;
            trialDirections = new TrialDiections();
            trialDirections.trialNumber = data.trialDiections.trialNumber;

            sessionInfoText.text = "P.-ID " + participant.id + "\nS-Dir: " + trialDirections.trialNumber;
            savePathText.text = pathToSaveLocation;
        }

    }

    public void SaveParticpantData()
    {
        string pathFolder = pathToSaveLocation + "ParticipantData";
        if (!Directory.Exists(pathFolder))
        {
            try
            {
                Directory.CreateDirectory(pathFolder);
            }
            catch (Exception e)
            {
                Debug.Log("The process failed: " + e.ToString());
            }
            finally { }
        }
        if (Directory.Exists(pathFolder))
        {
            BinaryFormatter bF = new BinaryFormatter();
            FileStream file = File.Create(pathFolder + "/Participant" + participant.id + ".dat");

            bF.Serialize(file, participant);
            file.Close();
        }
    }
    public void LoadParticipantData()
    {
        if (File.Exists(pathToSaveLocation + "/generatedTasks.dat"))
        {
            BinaryFormatter bF = new BinaryFormatter();
            FileStream file = File.Open(pathToSaveLocation + "/generatedTasks.dat", FileMode.Open);
            //GeneratedTasks data = (GeneratedTasks)bF.Deserialize(file);
            //file.Close();

            //generatedTasks = data;
        }
    }

    public void StudyLogging()
    {
        string pathFolder = pathToSaveLocation + "/Participant" + participant.id;
        if (!Directory.Exists(pathFolder))
        {
            try
            {
                Directory.CreateDirectory(pathFolder);
            }
            catch (Exception e)
            {
                Debug.Log("The process failed: " + e.ToString());
            }
            finally { }
        }

        int n = trialsIter % 4;
        string path = pathFolder + "/Participant" + participant.id + "_" + participant.actuationType + "_" + n + ".csv";

        int x = 0;
        while (File.Exists(path))
        {
            x++;
            path = pathFolder + "/Participant" + participant.id + "_" + participant.actuationType + "_" + n + "(" + x + ").csv";
        }

        using (StreamWriter fileCSV = new StreamWriter(path, true))
        {
            fileCSV.WriteLine("participantID;actuationMethode;trialID;detectionTime;submissionTime;direction;guessedDirection;certaintyOfGuess");
            string line = "";
            foreach (TrialInformation trial in participant.trials)
            {
                line += participant.id + ";";
                line += participant.actuationType + ";";
                line += trial.trialID + ";";
                line += trial.detectionTime + ";";
                line += trial.submissionTime + ";";
                line += trial.direction + ";";
                line += trial.directionGuessed + ";";
                line += trial.certaintyOfGuess + ";";
                fileCSV.WriteLine(line);
                line = "";
            }

        }
    }
#endif
    #endregion
}

[Serializable]
public class TrialInformation
{
    public int trialID;
    public int direction;
    public int directionGuessed;
    public float detectionTime;
    public float submissionTime;
    public int certaintyOfGuess;

    public TrialInformation(int trialID, int direction)
    {
        this.trialID = trialID;
        this.direction = direction;
    }
}

[Serializable]
public class TrialDiections
{
    public int trialNumber;
    public int[] permutaion1 = { 0, 45, 270, 180, 135, 315, 90, 225 };
    public int[] permutaion2 = { 45, 180, 0, 315, 270, 225, 135, 90 };
    public int[] permutaion3 = { 180, 315, 45, 225, 0, 90, 270, 135 };
    public int[] permutaion4 = { 315, 225, 180, 90, 45, 135, 0, 270 };
    public int[] permutaion5 = { 225, 90, 315, 135, 180, 270, 45, 0 };
    public int[] permutaion6 = { 90, 135, 225, 270, 315, 0, 180, 45 };
    public int[] permutaion7 = { 135, 270, 90, 0, 225, 45, 315, 180 };
    public int[] permutaion8 = { 270, 0, 135, 45, 90, 180, 225, 315 };

    public TrialDiections()
    {
        permutaion1 = new int[] { 0, 45, 270, 180, 135, 315, 90, 225 };
        permutaion2 = new int[] { 45, 180, 0, 315, 270, 225, 135, 90 };
        permutaion3 = new int[] { 180, 315, 45, 225, 0, 90, 270, 135 };
        permutaion4 = new int[] { 315, 225, 180, 90, 45, 135, 0, 270 };
        permutaion5 = new int[] { 225, 90, 315, 135, 180, 270, 45, 0 };
        permutaion6 = new int[] { 90, 135, 225, 270, 315, 0, 180, 45 };
        permutaion7 = new int[] { 135, 270, 90, 0, 225, 45, 315, 180 };
        permutaion8 = new int[] { 270, 0, 135, 45, 90, 180, 225, 315 };
    }

    public int[] getTrialDirections()
    {
        switch (trialNumber)
        {
            case 0:
                return permutaion1;
            case 1:
                return permutaion2;
            case 2:
                return permutaion3;
            case 3:
                return permutaion4;
            case 4:
                return permutaion5;
            case 5:
                return permutaion6;
            case 6:
                return permutaion7;
            case 7:
                return permutaion8;
            default:
                return permutaion1;
        }
    }

    public void incrementTrialNumber()
    {
        trialNumber = (trialNumber + 1) % 8;
    }
}

[Serializable]
public class ParticipantData
{
    public int id;
    public _ActuationType actuationType;
    public List<TrialInformation> trials;
}

[Serializable]
public class SessionData
{
    public int currentParticipant;
    public string savePath;
    public TrialDiections trialDiections;

    public SessionData(int currentParticipant, string savePath, TrialDiections trialDiections)
    {
        this.currentParticipant = currentParticipant;
        this.savePath = savePath;
        this.trialDiections = trialDiections;
    }
}

[Serializable]
public enum _ActuationType
{
    DirectionHot, DirectionCold, Interpolation, PushCH, Push
}
