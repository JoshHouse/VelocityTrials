using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; // For reading/writing to disk
using System.Xml.Serialization;
using System; // For working with XML files

public class BTManager : MonoBehaviour
{
    // Largst # of scores being displayed
    private const int MAX_BEST_TIMES = 30;

    // Smallest amount of time a new potential Best Time has to be quicker to be the true Best Time
    private const float BT_MARGIN = -.01f;

    // File Path to be appended to PersistentDataPath where XML file is located
    private const string BEST_TIME_FILE_PATH = "/besttimes.xml";

    // List of internally stored Best Times for this play session
    private List<BestTime> bestTimes = new List<BestTime>(MAX_BEST_TIMES);

    // Start is called before the first frame update
    void Start()
    {
        // Load previous session scores
        LoadScores();
    }

    // Add new BestTime to the bestTimes List, or replaces an old BestTime that will now be beaten
    public void AddNewBestTime(int givenLevelNum, float givenBestTime)
    {
        // Guard against givenLevelNum pointing to a non-existent level
        if (givenLevelNum > 0 && givenLevelNum <= MAX_BEST_TIMES)
        {
            // Search current bestTimes for the given level in question
            foreach (BestTime level in bestTimes)
            {
                // If level is found and has a worse BestTime than givenBestTime, replace old with the new
                if (level.levelNum == givenLevelNum && (givenBestTime - level.bestTime) < BT_MARGIN)
                {
                    level.bestTime = givenBestTime;
                }
            }

            // BestTime entry doesn't exist, so create it
            bestTimes.Add(new BestTime { levelNum = givenLevelNum, bestTime = givenBestTime });
        }
    }

    // Returns the Best Time of a given level number as a string if its bestTime entry exists
    public string GetBestTimeText(int givenlevelNum)
    {
        foreach (BestTime level in bestTimes)
        {
            if (level.levelNum == givenlevelNum)
            {
                return string.Format("{0:0.00}", level.bestTime);
            }
        }

        return "";
    }

    // Save scores to disk upon closing game
    void SaveScores()
    {
        // Create Saving Serializer
        BTSerializer bestTimeSerial = new BTSerializer();

        // Populate list with bestTimes (list of Best Times for this game session)
        bestTimeSerial.list = bestTimes;

        // Set up XML file stucture object, designed to serialize BTSerializer objects
        XmlSerializer serializer = new XmlSerializer(typeof(BTSerializer));

        // Set up file object where data is stored (inside a directory where OS allows us to write files for our game for the current user)
        // This works for ALL OS's: Windows, MacOS, Linux, Xbox System, etc.
        // For Windows, persistentDataPath == Users/[Username]/AppData/LocalLow/[CompanyName]/[GameName]
        FileStream bestTimeFile = new FileStream(Application.persistentDataPath + BEST_TIME_FILE_PATH, FileMode.Create);

        // Write to file (serialize to bestTimeFile with data from bestTimeSerial)
        serializer.Serialize(bestTimeFile, bestTimeSerial);
    }

    // This loads bestTimes from previous play session into memory (into the bestTimes list)
    void LoadScores()
    {
        if (File.Exists(Application.persistentDataPath + BEST_TIME_FILE_PATH))
        {
            // Set up XML file stucture object, designed to serialize BTSerializer objects
            XmlSerializer serializer = new XmlSerializer(typeof(BTSerializer));

            // Set up file object where data is stored (inside a directory where OS allows us to write files for our game for the current user)
            // This works for ALL OS's: Windows, MacOS, Linux, Xbox System, etc.
            // For Windows, persistentDataPath == Users/[Username]/AppData/LocalLow/[CompanyName]/[GameName]
            FileStream bestTimeFile = new FileStream(Application.persistentDataPath + BEST_TIME_FILE_PATH, FileMode.Open);

            // Create Loading Serializer by deserializing file as a BTSerializer
            BTSerializer bestTimeSerial = serializer.Deserialize(bestTimeFile) as BTSerializer;

            // Set current game session bestTimes to Best Time list from the file
            bestTimes = bestTimeSerial.list;
        }
    }

    // This function is called when the behaviour becomes disabled or inactive
    // This behaviour is always active in game, so this is called upon application exit or scene reload
    private void OnDisable()
    {
        SaveScores();
    }
}

