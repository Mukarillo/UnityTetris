using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class ScoreScreenType<T> : BaseScreen<T> where T : Component
{
	[SerializeField]
    protected TextMeshProUGUI scoreText;

	protected string mScorePrefix = "SCORE\n";

	protected void SetScoreText(int value)
    {
        Debug.Log(scoreText);
        scoreText.text = mScorePrefix + ((value == int.MaxValue) ? "Max Score" : value.ToString());
    }
}
