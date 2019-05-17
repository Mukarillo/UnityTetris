using UnityEngine;
using System.Collections.Generic;
using TetrisEngine.TetriminosPiece;
using System;
using pooling;

//This class represents a full tetris piece
//Receives information from engine about the positions of the blocks and its color
//Its a PoolingObject, so no object is deleted and few are created
public class TetriminoView : PoolingObject
{
	public override string objectName
	{
		get
		{
			return "TetriminoView";
		}
	}

	public bool isLocked
	{
		get
		{
			return currentTetrimino.isLocked;
		}
	}

    public bool destroyed;
    public Tetrimino currentTetrimino;
	public Action<TetriminoView> OnDestroyTetrimoView;
	public Pooling<TetriminoBlock> blockPool;

	private readonly List<TetriminoBlock> mPieces = new List<TetriminoBlock>();
	private Color mBlockColor;
	private RectTransform mRectTransform;

	private Vector2Int mTetriminoPosition;

	private void Awake()
	{
		mRectTransform = GetComponent<RectTransform>();
	}

    //overrides the Collect method to make sure its anchored in the right place
	public override void OnCollect()
    {
		base.OnCollect();

        destroyed = false;

		mRectTransform.anchorMin = Vector2.zero;
		mRectTransform.anchorMax = Vector2.one;
		mRectTransform.offsetMin = Vector2.zero;
        mRectTransform.offsetMax = Vector2.zero;
    }
    
    //Receives a TetriminoBase from the engine and creates in the view
	public void InitiateTetrimino(Tetrimino tetrimino, bool isPreview = false)
	{
		currentTetrimino = tetrimino;

		if (!isPreview)
			currentTetrimino.OnChangePosition = ChangePosition;
		else
			mRectTransform.SetAsFirstSibling();
		
		currentTetrimino.OnChangeRotation += Draw;
        
		mBlockColor = (isPreview) ? new Color(1,1,1,0.5f) : currentTetrimino.color;
		mPieces.ForEach(x => x.SetColor(mBlockColor));

		ChangePosition();
		Draw();
	}

    //Checks if any block is inside the line that just got deleted in the engine and release the block, if needed
    public void DestroyLine(int y)
	{
		for (int i = 0; i < mPieces.Count; i++)
		{         
			if (mPieces[i].position.y.Equals(y))
			{
				blockPool.Release(mPieces[i]);
				mPieces[i] = null;
				continue;
			}

			if (mPieces[i].position.y <= y)
				MovePiece(mPieces[i], mPieces[i].position.x, mPieces[i].position.y + 1);
		}

		mPieces.RemoveAll(x => x == null);

		if (mPieces.Count == 0)
			OnDestroyTetrimoView.Invoke(this);
	}

    //This method is used for the Preview piece, if forces the position and force a draw
	public void ForcePosition(int x, int y)
	{
		mTetriminoPosition = new Vector2Int(x, y);
        Draw();
	}

    //This is called every time the piece change its position, either if it was a movemente made by user input or normal Step
	private void ChangePosition()
	{
		mTetriminoPosition = currentTetrimino.currentPosition;
		Draw();
	}

    //Draw each block in the correct position based on the engine position and rotation
	private void Draw()
	{         
		var cRot = currentTetrimino.blockPositions[currentTetrimino.currentRotation];
		var currentIndex = 0;
              
        for (int i = 0; i < cRot.Length; i++)
        {
            for (int j = 0; j < cRot[i].Length; j++)
            {
				if (cRot[i][j] == 0) continue;

				var piece = mPieces.Count > currentIndex ? mPieces[currentIndex] : null;
                if(piece == null)
				{
					piece = blockPool.Collect(transform);
					piece.SetColor(mBlockColor);
					mPieces.Add(piece);
				}

				currentIndex++;
				MovePiece(piece, mTetriminoPosition.x + j, mTetriminoPosition.y + i);
            }
        }
	}

    //Internal call to move a specific piece
	private void MovePiece(TetriminoBlock block, int x, int y)
	{
		block.MoveTo(x, y);
	}
}
