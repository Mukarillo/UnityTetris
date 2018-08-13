public class GameOver : ScoreScreenType<GameOver> {

	public override void ShowScreen(float timeToTween = TIME_TO_TWEEN)
	{
		SetScoreText(Score.instance.PlayerScore);
		base.ShowScreen(timeToTween);
	}
}
