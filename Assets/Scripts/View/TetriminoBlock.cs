using pooling;
using UnityEngine;

public class TetriminoBlock : PoolingObject
{
	public override string objectName
	{
		get
		{
			return "TetriminoBlock";
		}      
	}

	private SpriteRenderer mSpriteRenderer;

	public void Awake()
	{
		mSpriteRenderer = GetComponent<SpriteRenderer>();
	}

    public void SetColor(Color c)
	{
		mSpriteRenderer.color = c;
	}
}
