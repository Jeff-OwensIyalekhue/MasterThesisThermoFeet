using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class ThermalFeedbackManager : MonoBehaviour
{
    [Header("Values")]
    public ParticipantData participant;
    public int amountTrials = 8;
    public List<int> trialDirections;
    public float timeBetweenTrials = 10;
    public float rangeMaxStartTime = 10;

    public bool isTrialRunning = false;


    [SerializeField] private string pathToSaveLocation = "C:/Users/Jeff-Owens/Desktop";

    void Update()
    {
        if (isTrialRunning && AppManager.Singleton.isTrialFinished)
        {
            int i = participant.trials.Count - 1;
            participant.trials[i].detectionTime = AppManager.Singleton.signalDetectionTime;
            participant.trials[i].directionGuessed = AppManager.Singleton.guessedDirection;
            participant.trials[i].certaintyOfGuess = AppManager.Singleton.certaintyOfGuess;
            isTrialRunning = false;
        }
    }

    public void StartTrial(int direction)
    {
        StartCoroutine(RunTrial(direction));
    }

    IEnumerator RunTrial(int direction)
    {
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
    }

    public void TestFunc()
    {
        StartMethodTrials(_ActuationType.Push, trialDirections);
    }

    public void StartMethodTrials(_ActuationType actuationType, List<int> directionList)
    {
        if (directionList.Count <= 0)
            return;
        participant.actuationType = actuationType;
        StartCoroutine(RunMethodTrial(directionList));
    }


    IEnumerator RunMethodTrial(List<int> directionsLeft)
    {
        Debug.Log("" + directionsLeft.Count + " runs left");
        int i = UnityEngine.Random.Range(0, directionsLeft.Count);

        StartCoroutine(RunTrial(directionsLeft[i]));

        yield return new WaitWhile(() => { return isTrialRunning; });

        yield return new WaitForSeconds(timeBetweenTrials);

        directionsLeft.RemoveAt(i);
        StartMethodTrials(participant.actuationType, directionsLeft);
    }

    public void SetParticpantId(int value)
    {
        participant.id = value;
    }

    public void SetActuationType(int value)
    {
        switch (value)
        {
            case 0:
                participant.actuationType = _ActuationType.Direction;
                break;
            case 1:
                participant.actuationType = _ActuationType.Interpolation;
                break;
            case 2:
                participant.actuationType = _ActuationType.Push;
                break;
            default:
                break;
        }

    }

    #region LogFile Functions
#if !UNITY_ANDROID
    public void SaveStudyParams()
    {

    }
    public void LoadStudyParams()
    {

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

    public void LoadLogFile()
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

        string path = pathFolder + "/Participant" + participant.id + "_" + participant.actuationType + ".csv";

        int x = 0;
        while (File.Exists(path))
        {
            x++;
            path = pathFolder + "/Participant" + participant.id + "_" + participant.actuationType + "(" + x + ").csv";
        }

        using (StreamWriter fileCSV = new StreamWriter(path, true))
        {
            fileCSV.WriteLine("participantID;actuationMethode;trialID;detectionTime;direction;guessedDirection;certaintyOfGuess");
            string line = "";
            foreach (TrialInformation trial in participant.trials)
            {
                line += participant.id + ";";
                line += participant.actuationType + ";";
                line += trial.trialID + ";";
                line += trial.detectionTime + ";";
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
    public int certaintyOfGuess;

    public TrialInformation(int trialID, int direction)
    {
        this.trialID = trialID;
        this.direction = direction;
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
public enum _ActuationType
{
    Direction, Interpolation, Push, Pull
}