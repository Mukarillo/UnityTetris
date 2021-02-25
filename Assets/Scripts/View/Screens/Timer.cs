using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Timer : TimerScreenType<Timer>
{
    public float TimeRemaining
    {
        get
        {
            return mInternalTime;
        }
        set
        {
            mInternalTime = value;
        }
    }

    private float mInternalTime = 0;

    public void SubtractTime(float points)
    {
        if (mInternalTime - points < 0)
        {
            mInternalTime = 0;
        }
        else
        {
            mInternalTime -= points;
        }
        SetTimeText(mInternalTime);
        ShowScreen();
    }

    public void ResetScore()
    {
        mInternalTime = 0;
        SetTimeText(mInternalTime);
    }

    public override void ShowScreen(float timeToTween = 1f)
    {
        base.ShowScreen(timeToTween);
        StopCoroutine(WaitAndHide());
        StartCoroutine(WaitAndHide());
    }

    private IEnumerator WaitAndHide()
    {
        yield return new WaitForSeconds(2f);
        HideScreen();
    }

    protected override void InternalAlphaScreen(float timeToTween, float alpha, TweenCallback callback)
    {
        base.InternalAlphaScreen(timeToTween, Mathf.Clamp(alpha, 0.2f, 1f), callback);
    }
}
