using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ThemeTimerAction
{
    public enum ThemeTimerActionType
    {
        None = 0,
        Loop,
        SpecificTiming
    }

    public ThemeTimerActionType ThemeType;
    public List<float> TimerActions;

    public ThemeTimerAction(ThemeTimerActionType _actionType, float _actionLoopTime)
    {
        ThemeType = _actionType;

        TimerActions = new List<float>();
        TimerActions.Add(_actionLoopTime);
    }

    public ThemeTimerAction(ThemeTimerActionType _actionType, List<float> _timerActionTiming)
    {
        ThemeType = _actionType;
        TimerActions = _timerActionTiming;
    }
}

public struct GrooveTheme
{
    // Chosen Randomized Seed
    // [SerializeField]
    int ThemeSeed;

    // Theme Music
    public AudioSource ThemeAudioSource;
    public AudioClip ThemeAudioClip;

    public List<ThemeTimerAction> ThemeActions;

    public GrooveTheme(AudioClip _themeMusic, List<ThemeTimerAction> _themeActions, int _playerSeed = -1, int _themeSeed = -1)
    {
        #region Apply Random or Applied Seed
        // (If no other Seed exists, randomize one)
        ThemeSeed = Random.Range(10000000, 99999999);

        // Theme-Applied Seed
        // (Just in-case a Seed is pre-applied, such as for a challenge, gametype override, or something applied)
        if (_themeSeed != -1)
            ThemeSeed = _themeSeed;

        // Player-Created Seed
        // (If a player entered their own seed)
        if (_playerSeed != -1)
            ThemeSeed = _playerSeed;
        #endregion

        #region Theme Music
        ThemeAudioSource = new AudioSource();
        ThemeAudioClip = _themeMusic;
        // ThemeAudioClip.clip = _themeMusic;
        #endregion

        #region Theme Timer Actions
        ThemeActions = _themeActions;
        // Need to store list of actions to perform (Shift Left/Right, etc...)
        #endregion Theme Timer Actions
    }
}


public class ThemeManagerLogic : MonoBehaviour
{
    List<GrooveTheme> GrooveThemeList = new List<GrooveTheme>();
    GrooveTheme currTheme;

    void StartNextTheme()
    {
        if (GrooveThemeList.Count > 0)
        {
            currTheme = GrooveThemeList[0];
            GrooveThemeList.RemoveAt(0);

            // Apply Settings and Start Process
            StartCoroutine( Thread_AudioClip(currTheme.ThemeAudioSource) );
        }
    }


    IEnumerator Thread_AudioClip(AudioSource _audioSource)
    {
        float musicLength = _audioSource.clip.length;

        currTheme.ThemeAudioSource = transform.GetComponent<AudioSource>();
        _audioSource.clip.LoadAudioData();

        while(_audioSource.clip.loadState != AudioDataLoadState.Loaded)
            yield return new WaitForEndOfFrame();

        print("PLAYING: " + currTheme.ThemeAudioSource.clip.name);

        currTheme.ThemeAudioSource.Play();

        print("Starting Theme Action Timers");

        foreach (ThemeTimerAction themeAction in currTheme.ThemeActions)
        {
            if (themeAction.ThemeType == ThemeTimerAction.ThemeTimerActionType.Loop)
                StartCoroutine(RunThemeTimerAction_Loop(themeAction));
            else if (themeAction.ThemeType == ThemeTimerAction.ThemeTimerActionType.SpecificTiming)
                StartCoroutine( RunThemeTimerAction_SpecificTiming(themeAction) );
        }

        print("Waiting until song ends");

        yield return new WaitForSecondsRealtime( musicLength );

        print("Song Ended: " + Time.fixedTime);

        currTheme.ThemeAudioSource.clip.UnloadAudioData();
        currTheme.ThemeAudioSource = null;

        print("Song Unloaded: " + Time.fixedTime);

        yield return null;
    }

    public void LoadThemeToList(GrooveTheme _grooveTheme)
    {
        GrooveThemeList.Add(_grooveTheme);

        // TODO: Change when appropriate
        StartNextTheme();
    }

    /// <summary>
    /// A single timing that is looped. For example, 3.0f seconds of a repeated loop until the song completes.
    /// ALL timing is in Milliseconds
    /// </summary>
    /// <param name="themeTimerAction"></param>
    /// <returns></returns>
    IEnumerator RunThemeTimerAction_Loop(ThemeTimerAction themeTimerAction)
    {
        bool stillRunning = true;
        int testCounter = 10;
        GameObject gameLogic = GameObject.Find("GameLogic");
        GameLogic gameLogicScript = gameLogic.GetComponent<GameLogic>();

        while (stillRunning)
        {
            if(testCounter > 0)
            {
                testCounter--;

                if(testCounter < 0)
                    stillRunning = false;

                yield return new WaitForSecondsRealtime(themeTimerAction.TimerActions[0]);

                // TODO: Remove this functionality and replace with Looped action
                gameLogicScript.ShiftBoardLeft();
            }
        }

        yield return null;
    }

    /// <summary>
    /// A long list of specific timed actions. Used if beats are inconsistent or more precise timing is desired.
    /// ALL timing is in Milliseconds
    /// </summary>
    /// <param name="themeTimerAction"></param>
    /// <returns></returns>
    IEnumerator RunThemeTimerAction_SpecificTiming(ThemeTimerAction themeTimerAction)
    {
        print("Starting Theme Timer Action - Specific Timing");
        print("Count: " + themeTimerAction.TimerActions.Count);

        bool stillRunning = true;

        GameObject gameLogic = GameObject.Find("GameLogic");
        GameLogic gameLogicScript = gameLogic.GetComponent<GameLogic>();

        while (stillRunning)
        {
            foreach (float themeTimer in themeTimerAction.TimerActions)
            {
                yield return new WaitForSecondsRealtime(themeTimer);

                gameLogicScript.ShiftBoardLeft();
            }

            stillRunning = false;
        }

        yield return null;
    }
}
