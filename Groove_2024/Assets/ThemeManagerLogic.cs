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
    public float[] TimerActions;

    public ThemeTimerAction(ThemeTimerActionType _actionType, float _actionLoopTime)
    {
        ThemeType = _actionType;

        float[] timerAction = new float[1];
        timerAction[0] = _actionLoopTime;
        TimerActions = timerAction;
    }

    public ThemeTimerAction(ThemeTimerActionType _actionType, float[] _timerActionTiming)
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

            currTheme.ThemeAudioSource = transform.GetComponent<AudioSource>();

            currTheme.ThemeAudioSource.clip.LoadAudioData();

            foreach(ThemeTimerAction themeAction in currTheme.ThemeActions)
            {
                if(themeAction.ThemeType == ThemeTimerAction.ThemeTimerActionType.Loop)
                    StartCoroutine( RunThemeTimerAction_Loop(themeAction) );
                else if(themeAction.ThemeType == ThemeTimerAction.ThemeTimerActionType.SpecificTiming)
                    StartCoroutine( RunThemeTimerAction_SpecificTiming(themeAction) );
            }
            
            print(currTheme.ThemeAudioSource.clip.name);

            // Apply Settings and Start Provess
            currTheme.ThemeAudioSource.Play();
        }
    }

    public void LoadThemeToList(GrooveTheme _grooveTheme)
    {
        GrooveThemeList.Add(_grooveTheme);

        // TODO: Change when appropriate
        StartNextTheme();
    }

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

    IEnumerator RunThemeTimerAction_SpecificTiming(ThemeTimerAction themeTimerAction)
    {
        bool stillRunning = true;

        while (stillRunning)
        {
            yield return new WaitForSecondsRealtime(themeTimerAction.TimerActions[0]);
        }

        yield return null;
    }
}
