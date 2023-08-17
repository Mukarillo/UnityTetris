using DG.Tweening;
using UnityEngine;

public class CameraHandler
{
    private readonly Camera mCamera;
    private Tween mCameraShakeTween;

    public CameraHandler(Camera camera)
    {
        mCamera = camera;
    }

    public void ShakeCamera()
    {
        mCameraShakeTween?.Kill();
        mCameraShakeTween = mCamera.transform.DOShakePosition(.1f, Vector2.one * .15f, 50, 5);
    }
}