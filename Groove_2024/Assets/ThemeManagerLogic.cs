using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
    public int BoardWidth;

    public bool ThreeWide;
    public bool ThreeTall;
    public bool TwoByTwo;

    // Theme Music
    public AudioSource ThemeAudioSource;
    public AudioClip ThemeAudioClip;

    public List<ThemeTimerAction> ThemeActions;

    public GrooveTheme(AudioClip _themeMusic, List<ThemeTimerAction> _themeActions, bool _threeWide, bool _threeTall, bool _twoByTwo, int _boardWidth = 10, int _playerSeed = -1, int _themeSeed = -1)
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

        #region Gameplay Settings
        BoardWidth = _boardWidth;

        ThreeWide = _threeWide;
        ThreeTall = _threeTall;
        TwoByTwo = _twoByTwo;
        #endregion Gameplay Settings

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

    GameObject GO_GameLogic;
    GameLogic GameLogic;

    private void Start()
    {
        GO_GameLogic = GameObject.Find("GameLogic");
        GameLogic = GO_GameLogic.GetComponent<GameLogic>();
    }

    
    IEnumerator StartNextTheme()
    {
        if (GrooveThemeList.Count > 0)
        {
            currTheme = GrooveThemeList[0];
            GrooveThemeList.RemoveAt(0);

            BoardWidthResizeComplete = false;
            SoundClipLoadedToMemory = false;

            print(currTheme.ThreeWide + " " + currTheme.ThreeTall + " " + currTheme.TwoByTwo);

            // TODO: Determine when to override future Block List.
            GameLogic.SetValidActiveBlockTypes(currTheme.ThreeWide, currTheme.ThreeTall, currTheme.TwoByTwo);

            // Resize board and wait until completion
            print( "Setting Board Width: " + currTheme.BoardWidth );
            StartCoroutine( GameLogic.SetNewBoardWidthForTheme( currTheme.BoardWidth ) );

            print( "Loading SoundClip to Memory..." );
            StartCoroutine( LoadSoundClip(currTheme.ThemeAudioSource) );

            // Update this with all necessary wait scenarios to ensure the board & game is ready before continuing.
            while ( !(BoardWidthResizeComplete && SoundClipLoadedToMemory) )
                yield return new WaitForSeconds(0.05f);

            GameLogic.ThemeLoaded();

            // Apply Settings and Start Process
            StartCoroutine( Thread_AudioClip(currTheme.ThemeAudioSource) );
        }

        yield return null;
    }

    // Tells the GameLogic to change the BoardWidth and waits patiently until it's done
    IEnumerator Thread_ResizeBoard()
    {
        yield return true;
    }

    IEnumerator Thread_AudioClip(AudioSource _audioSource)
    {
        float musicLength = _audioSource.clip.length;

        print("PLAYING: " + currTheme.ThemeAudioSource.clip.name);

        currTheme.ThemeAudioSource.Play();

        print("Starting Theme Action Timers");

        if (currTheme.ThemeActions == null)
            print("Is Null");

        if (currTheme.ThemeActions != null)
        {
            foreach (ThemeTimerAction themeAction in currTheme.ThemeActions)
            {
                if (themeAction.ThemeType == ThemeTimerAction.ThemeTimerActionType.Loop)
                    StartCoroutine(RunThemeTimerAction_Loop(themeAction));
                else if (themeAction.ThemeType == ThemeTimerAction.ThemeTimerActionType.SpecificTiming)
                    StartCoroutine(RunThemeTimerAction_SpecificTiming(themeAction));
            }
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
        StartCoroutine( StartNextTheme() );
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

    bool BoardWidthResizeComplete;
    public void BoardWidthResizeCompleted()
    {
        BoardWidthResizeComplete = true;
    }

    bool SoundClipLoadedToMemory;
    private IEnumerator LoadSoundClip(AudioSource audioSource)
    {
        audioSource.clip.LoadAudioData();

        while (audioSource.clip.loadState != AudioDataLoadState.Loaded)
            yield return new WaitForEndOfFrame();

        SoundClipLoadedToMemory = true;

        yield return null;
    }

    #region Helper Functions

    bool timedOut;
    CancellationToken ct;
    private IEnumerator PerformTimedAction(System.Action action, int timeout = 1)
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        ct = cts.Token;

        Coroutine timeoutCoroutine = StartCoroutine(TimeoutChecker(timeout));
        var t = Task.Run(action, ct);
        yield return new WaitWhile(() => t.Status != TaskStatus.RanToCompletion && !timedOut);

        if (timedOut)
        {
            cts.Cancel();
            Debug.Log("Task Timed Out");
        }
        else
        {
            StopCoroutine(timeoutCoroutine);
            Debug.Log("Task successfully completed");
        }
    }

    private IEnumerator TimeoutChecker(float timeout)
    {
        timedOut = false;
        while (timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }
        timedOut = true;
    }

    #endregion Helper Functions
}
