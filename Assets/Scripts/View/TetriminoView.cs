using UnityEngine;
using System.Collections.Generic;
using TetrisEngine.Tetriminos;
using System;

public class TetriminoView : MonoBehaviour
{
    public bool isLocked
	{
		get
		{
			return currentTetrimino.isLocked;
		}
	}

	public TetriminoBase currentTetrimino;
	public Action<TetriminoView> OnDestroyTetrimoView;

	[SerializeField]
	private GameObject prefabReference;

	private List<GameObject> mPieces = new List<GameObject>();
	private Color mBlockColor;

	public void InitiateTetrimino(TetriminoBase tetrimino, bool isPreview = false)
	{
		currentTetrimino = tetrimino;
		if (!isPreview)
			currentTetrimino.OnChangePosition = ChangePosition;
		
		currentTetrimino.OnChangeRotation += Draw;
        
		mBlockColor = (isPreview) ? new Color(1,1,1,0.5f) : currentTetrimino.color;

		ChangePosition();
		Draw();
	}

    public void DestroyLine(int y)
	{
		int nullAmount = 0;
		for (int i = 0; i < mPieces.Count; i++)
		{
			if (mPieces[i] == null){
				nullAmount++;
				continue;	
			}

			if (Mathf.Approximately(mPieces[i].transform.position.y, -y))
			{
				Destroy(mPieces[i]);
				nullAmount++;
			}

			if (mPieces[i].transform.position.y > -y)
				mPieces[i].transform.position = new Vector3(mPieces[i].transform.position.x, mPieces[i].transform.position.y - 1, 0);
		}

		if (nullAmount == mPieces.Count)
			OnDestroyTetrimoView.Invoke(this);
	}

	private void ChangePosition()
	{
		transform.position = new Vector3(currentTetrimino.currentPosition.x, -currentTetrimino.currentPosition.y, 0);
	}

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
					piece = Instantiate(prefabReference, transform);
					piece.GetComponent<SpriteRenderer>().color = mBlockColor;
					mPieces.Add(piece);
				}

				currentIndex++;
				piece.transform.localPosition = new Vector3(j, -i);
            }
        }
	}

	private void OnDestroy()
	{
		currentTetrimino.OnChangeRotation -= Draw;
	}
}
