using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class GrooveThemes : MonoBehaviour
{
    [SerializeField] string themeName;
    [SerializeField] AudioClip audioClip;

    [SerializeField] bool twoByTwo;
    [SerializeField] bool threeWide;
    [SerializeField] bool threeTall;

    private void Start()
    {
        GrooveTheme goBack = new GrooveTheme();
        goBack.ThemeAudioSource = transform.GetComponent<AudioSource>();
        goBack.ThemeAudioSource.clip = audioClip;

        goBack.ThemeAudioSource.clip.LoadAudioData();

        // Create a list to populate
        List<ThemeTimerAction> allThemeTimerActions = new List<ThemeTimerAction>();
        
        // Create new groups of each ThemeTimerAction that I want to have exist
        ThemeTimerAction themeAction_A = new ThemeTimerAction(ThemeTimerAction.ThemeTimerActionType.Loop, 3.0f);
        allThemeTimerActions.Add(themeAction_A);
        
        // Apply the list to the GrooveTheme object
        goBack.ThemeActions = allThemeTimerActions;

        // Load the theme
        transform.GetComponent<ThemeManagerLogic>().LoadThemeToList(goBack);
    }
}