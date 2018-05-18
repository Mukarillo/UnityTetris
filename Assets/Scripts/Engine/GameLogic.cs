using UnityEngine;
using TetrisEngine.Tetriminos;
using System;
using System.Collections.Generic;

namespace TetrisEngine
{   
	public class GameLogic : MonoBehaviour 
    {      
		public GameObject tetriminoViewPrefab;
		public float timeToStep = 2f;

		private Playfield mPlayfield;
		private List<TetriminoView> mTetriminos = new List<TetriminoView>();
		private float mTimer = 0f;

		private TetriminoBase mCurrentTetrimino
		{
			get
			{
				return (mTetriminos.Count > 0 && !mTetriminos[mTetriminos.Count - 1].isLocked) ? mTetriminos[mTetriminos.Count - 1].currentTetrimino : null;
			}
		}

		private TetriminoView mPreview;
		private bool mRefreshPreview;      

        public void Start()
		{
			mPlayfield = new Playfield();
			mPlayfield.OnCurrentPieceReachBottom = CreateTetrimino;
			mPlayfield.OnGameOver = GameOver;
			mPlayfield.OnDestroyLine = DestroyLine;
			CreateTetrimino();
		}

		private void DestroyLine(int y)
		{
			mTetriminos.ForEach(x => x.DestroyLine(y));
			mTetriminos.RemoveAll(x => x == null);
		}

		private void GameOver()
		{
			Debug.Log("GAME OVER");
		}

        private void CreateTetrimino()
		{
			if (mCurrentTetrimino != null)
				mCurrentTetrimino.isLocked = true;
			
			var tetrimino = mPlayfield.CreateTetrimo();
			var tetriminoView = Instantiate(tetriminoViewPrefab).GetComponent<TetriminoView>();
			tetriminoView.OnDestroyTetrimoView = DestroyTetrimino;
			tetriminoView.InitiateTetrimino(tetrimino);
			mTetriminos.Add(tetriminoView);

			if (mPreview != null)
				Destroy(mPreview.gameObject);
			
			mPreview = Instantiate(tetriminoViewPrefab).GetComponent<TetriminoView>();
			mPreview.InitiateTetrimino(tetrimino, true);
			mRefreshPreview = true;
		}

		private void DestroyTetrimino(TetriminoView obj)
		{
			var index = mTetriminos.FindIndex(x => x == obj);
			Destroy(obj);
			mTetriminos[index] = null;         
		}

		public void Update()
		{
			mTimer += Time.deltaTime;
			if(mTimer > timeToStep)
			{
				mTimer = 0;
				mPlayfield.Step();
			}
			if (mCurrentTetrimino == null) return;

			if(Input.GetKeyDown(KeyCode.DownArrow))
			{
				if(mPlayfield.IsPossibleMovement(mCurrentTetrimino.currentPosition.x,
    											  mCurrentTetrimino.currentPosition.y,
    											  mCurrentTetrimino,
    			                                  mCurrentTetrimino.NextRotation))
				{
					mCurrentTetrimino.currentRotation = mCurrentTetrimino.NextRotation;
					mRefreshPreview = true;
				}
			}

			if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (mPlayfield.IsPossibleMovement(mCurrentTetrimino.currentPosition.x,
                                                  mCurrentTetrimino.currentPosition.y,
                                                  mCurrentTetrimino,
    			                                  mCurrentTetrimino.PreviousRotation))
                {
					mCurrentTetrimino.currentRotation = mCurrentTetrimino.PreviousRotation;
					mRefreshPreview = true;
                }
            }

			if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (mPlayfield.IsPossibleMovement(mCurrentTetrimino.currentPosition.x - 1,
                                                  mCurrentTetrimino.currentPosition.y,
                                                  mCurrentTetrimino,
                                                  mCurrentTetrimino.currentRotation))
                {
                    mCurrentTetrimino.currentPosition = new Vector2Int(mCurrentTetrimino.currentPosition.x - 1, mCurrentTetrimino.currentPosition.y);
					mRefreshPreview = true;
                }
            }

			if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (mPlayfield.IsPossibleMovement(mCurrentTetrimino.currentPosition.x + 1,
                                                  mCurrentTetrimino.currentPosition.y,
                                                  mCurrentTetrimino,
                                                  mCurrentTetrimino.currentRotation))
                {
                    mCurrentTetrimino.currentPosition = new Vector2Int(mCurrentTetrimino.currentPosition.x + 1, mCurrentTetrimino.currentPosition.y);
					mRefreshPreview = true;
                }
            }

			if (Input.GetKey(KeyCode.Space))
            {
                if (mPlayfield.IsPossibleMovement(mCurrentTetrimino.currentPosition.x,
                                                  mCurrentTetrimino.currentPosition.y + 1,
                                                  mCurrentTetrimino,
				                                  mCurrentTetrimino.currentRotation))
                {
					mCurrentTetrimino.currentPosition = new Vector2Int(mCurrentTetrimino.currentPosition.x, mCurrentTetrimino.currentPosition.y + 1);               
                }
            }

			if(mRefreshPreview)
			{
				var y = mCurrentTetrimino.currentPosition.y;
				while(mPlayfield.IsPossibleMovement(mCurrentTetrimino.currentPosition.x,
                                                  y,
                                                  mCurrentTetrimino,
                                                  mCurrentTetrimino.currentRotation))
				{
					y++;
				}

				mPreview.transform.position = new Vector3(mCurrentTetrimino.currentPosition.x, -y + 1, 0);
				mRefreshPreview = false;
			}
		}
	}
}
