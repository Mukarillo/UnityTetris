using UnityEngine;
using DG.Tweening;

//Abstract class to be implemented by Screens
//Has a simple Singleton system and some functionalities that are common inside screens
public abstract class BaseScreen<T> : MonoBehaviour where T : Component
{
	public const float TIME_TO_TWEEN = 1f;
	public static T instance;
    
	protected CanvasGroup mCanvasGroup;

	protected virtual void Awake()
	{
		instance = GetComponent<T>();
		mCanvasGroup = GetComponent<CanvasGroup>();
	}   

	public virtual void ShowScreen(float timeToTween = TIME_TO_TWEEN)
	{
		InternalAlphaScreen(timeToTween, 1f, () => {
            mCanvasGroup.interactable = true;
        });
	}

	public virtual void HideScreen(float timeToTween = TIME_TO_TWEEN)
	{
		InternalAlphaScreen(timeToTween, 0f, () => {
			mCanvasGroup.interactable = false;
		});
	}

	protected virtual void InternalAlphaScreen(float timeToTween, float alpha, TweenCallback callback)
	{
		mCanvasGroup.DOFade(alpha, timeToTween).OnComplete(callback);
	}
}
