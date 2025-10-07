using UnityEditor;
using UnityEngine;

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

        transform.GetComponent<ThemeManagerLogic>().LoadThemeToList(goBack);
    }
}