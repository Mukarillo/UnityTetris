using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class TimerScreenType<T> : BaseScreen<T> where T : Component
{
	[SerializeField]
    protected TextMeshProUGUI timeText;

	protected string mTimePrefix = "Time Remaining:{0}:{1}\n";

	protected void SetTimeText(float value)
    {
        int minutes = (int)(value / 60);
        int seconds = (int)(value - (minutes * 60));
        timeText.text = string.Format(mTimePrefix, minutes, seconds.ToString("00"));
    }
}
