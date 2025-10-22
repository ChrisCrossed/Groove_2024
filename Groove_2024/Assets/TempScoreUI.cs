using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class TempScoreUI : MonoBehaviour
{
    [SerializeField] GameObject GO_Text_CurrentScore;
    [SerializeField] GameObject GO_Text_ScoreExplainer;

    TextMeshProUGUI Text_CurrentScore;
    TextMeshProUGUI Text_ScoreExplainer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Text_CurrentScore = GO_Text_CurrentScore.GetComponent<TextMeshProUGUI>();
        Text_ScoreExplainer = GO_Text_ScoreExplainer.GetComponent<TextMeshProUGUI>();
    }

    public void ApplyNewScore(int _score, int _linePoints, int _mult)
    {
        // Text_CurrentScore.SetText("Current Score: " + _score);
        Text_CurrentScore.SetText("Current Score: " + _score);

        Text_ScoreExplainer.SetText("(" + _linePoints + ")" + " x " + "(" + _mult + ")");
        StartCoroutine( FlashScoreExplainer() );
    }

    private IEnumerator FlashScoreExplainer()
    {
        GO_Text_ScoreExplainer.SetActive(true);

        yield return new WaitForSeconds(3.0f);

        GO_Text_ScoreExplainer.SetActive(false);

        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
