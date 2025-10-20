using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class GrooveThemes : MonoBehaviour
{
    static float MoveTimer_MAX = 0.03f;

    [SerializeField] string themeName;
    [SerializeField] AudioClip audioClip;

    [SerializeField] bool twoByTwo;
    [SerializeField] bool threeWide;
    [SerializeField] bool threeTall;

    [SerializeField] int boardWidth;

    private void Start()
    {
        GrooveTheme goBack = new GrooveTheme();
        goBack.ThemeAudioSource = transform.GetComponent<AudioSource>();
        goBack.ThemeAudioSource.clip = audioClip;

        goBack.BoardWidth = boardWidth;

        goBack.ThreeWide = threeWide;
        goBack.ThreeTall = threeTall;
        goBack.TwoByTwo = twoByTwo;

        goBack.ThemeAudioSource.clip.LoadAudioData();

        #region Action Timers
        // Create a list to populate
        List<ThemeTimerAction> allThemeTimerActions = new List<ThemeTimerAction>();

        // Create new groups of each ThemeTimerAction that I want to have exist
        List<float> themeTiming = new List<float>();

        // Purple (The colors used in Sony Vegas to find the timing, in groups for ease of use)
        themeTiming.Add(ReturnTimeWithFrameRateConversion(7, 4));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 17));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 16));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 13));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 16));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 17));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 16));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 17));

        // Strawberry
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 17));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 16));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 13));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 18));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 13));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 16));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 16));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 16));

        // Red
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 16));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 15));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 15));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 16));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 17));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 15));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 16));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 15));

        // Orange
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 16));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 16));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 16));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 15));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 18));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 14));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 16));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 16));

        // Lellow
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 6));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 6));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 4));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(3, 1));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 27));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 28));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 25));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 24));

        // Grrveen
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 23));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 22));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 23));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 23));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 22));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 23));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 22));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 23));
        
        // Blue
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 24));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 22));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 23));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 23));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 22));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 23));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 22));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 24));

        // Other Blue
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 23));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 20));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 23));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 24));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 22));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 23));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 23));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 23));
        themeTiming.Add(ReturnTimeWithFrameRateConversion(2, 24));

        ThemeTimerAction themeAction_A = new ThemeTimerAction(ThemeTimerAction.ThemeTimerActionType.SpecificTiming, themeTiming);
        allThemeTimerActions.Add(themeAction_A);

        // Apply the list to the GrooveTheme object
        goBack.ThemeActions = allThemeTimerActions;

        #endregion Action Timers

        // Load the theme
        transform.GetComponent<ThemeManagerLogic>().LoadThemeToList(goBack);
    }

    float ReturnTimeWithFrameRateConversion(float seconds, float milliseconds, float framerate = 30f)
    {
        return (seconds + (milliseconds / framerate));
    }
}