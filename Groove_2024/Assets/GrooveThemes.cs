using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GrooveThemes : MonoBehaviour
{
    static float MoveTimer_MAX = 0.03f;

    [SerializeField] string themeName;
    [SerializeField] string songName;
    [SerializeField] string artistName;
    [SerializeField] string albumName;


    [SerializeField] AudioClip audioClip;

    [SerializeField] bool twoByTwo;
    [SerializeField] bool threeWide;
    [SerializeField] bool threeTall;

    [SerializeField] int boardWidth;

    private void Start()
    {
        ReadDataFromFile();

        GrooveTheme goBack = new GrooveTheme();
        goBack.ThemeAudioSource = transform.GetComponent<AudioSource>();
        

        goBack.BoardWidth = boardWidth;

        goBack.ThreeWide = threeWide;
        goBack.ThreeTall = threeTall;
        goBack.TwoByTwo = twoByTwo;

        goBack.ThemeAudioClip = audioClip;

        // goBack.ThemeAudioSource.clip.LoadAudioData();

        if(themeName == "GoBack")
        {
            #region Action Timers
            // Create a list to populate
            List<ThemeTimerAction> allThemeTimerActions = new List<ThemeTimerAction>();

            // Create new groups of each ThemeTimerAction that I want to have exist
            List<float> themeTiming = new List<float>();

            List<Vector2Int> themeTimingValues = ReadDataFromFile();

            foreach (Vector2Int timingValue in themeTimingValues)
                themeTiming.Add(ReturnTimeWithFrameRateConversion(timingValue));

            if(themeTiming.Count > 0)
            {
                ThemeTimerAction themeAction_A = new ThemeTimerAction(ThemeTimerAction.ThemeTimerActionType.SpecificTiming, themeTiming);
                allThemeTimerActions.Add(themeAction_A);

                // Apply the list to the GrooveTheme object
                goBack.ThemeActions = allThemeTimerActions;
            }
            #endregion Action Timers
        }

        // Load the theme
        transform.GetComponent<ThemeManagerLogic>().LoadThemeToList(goBack);
    }

    List<Vector2Int> ReadDataFromFile()
    {
        List<Vector2Int> outputList = new List<Vector2Int>();

        TextAsset strReader = Resources.Load<TextAsset>("Themes/TimingActions/GoBack/ShiftBoardLeft");
        //StreamReader strReader = new StreamReader(Application.dataPath + "\\Resources\\Themes\\TimingActions\\GoBack\\ShiftBoardLeft.csv");

        string[] splitLine = strReader.text.Split('\n');

        foreach(string line in splitLine)
        {
            string[] dataValues = line.Split(',');

            int x = int.Parse(dataValues[0]);
            int y = int.Parse(dataValues[1]);

            outputList.Add(new Vector2Int(x, y));
        }

        return outputList;
    }

    float ReturnTimeWithFrameRateConversion(Vector2Int timing, float framerate = 30f)
    {
        return ReturnTimeWithFrameRateConversion((float)timing.x, (float)timing.y, framerate);
    }

    float ReturnTimeWithFrameRateConversion(float seconds, float milliseconds, float framerate = 30f)
    {
        return (seconds + (milliseconds / framerate));
    }
}