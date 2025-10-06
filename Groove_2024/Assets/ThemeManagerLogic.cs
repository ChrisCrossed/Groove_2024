using System.Collections.Generic;
using UnityEngine;

public struct GrooveTheme
{
    // Chosen Randomized Seed
    // [SerializeField]
    int ThemeSeed;

    // Theme Music
    public AudioSource ThemeAudioSource;
    public AudioClip ThemeAudioClip;

    public GrooveTheme(AudioClip _themeMusic, int _playerSeed = -1, int _themeSeed = -1)
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
    }
}


public class ThemeManagerLogic : MonoBehaviour
{
    List<GrooveTheme> GrooveThemeList = new List<GrooveTheme>();
    GrooveTheme currTheme;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void StartNextTheme()
    {
        if (GrooveThemeList.Count > 0)
        {
            currTheme = GrooveThemeList[0];
            GrooveThemeList.RemoveAt(0);

            currTheme.ThemeAudioSource = transform.GetComponent<AudioSource>();

            currTheme.ThemeAudioSource.clip.LoadAudioData();

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
}
