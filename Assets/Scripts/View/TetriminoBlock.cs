using pooling;
using UnityEngine;

//Single block from pieces in view
public class TetriminoBlock : PoolingObject
{
	public override string objectName
	{
		get
		{
			return "TetriminoBlock";
		}      
	}

	public Vector2Int position { get; private set; }
    
	private SpriteRenderer mSpriteRenderer;
       
    //Gets references to the components
	public void Awake()
	{
	    mSpriteRenderer = GetComponent<SpriteRenderer>();
	}

    //Sets the color of the block
    public void SetColor(Color c)
	{
	    mSpriteRenderer.color = c;
	}

    //Positioning the block
    public void MoveTo(int x, int y)
	{
		position = new Vector2Int(x, y);
	    transform.localPosition = new Vector3(x, -y, 0);
	}
}