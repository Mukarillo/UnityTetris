using System;
using UnityEngine;
using TetrisEngine.TetriminosPiece;
using Photon.Pun;

namespace TetrisEngine
{
	//This class is a representation of the field in the engine
	//Stores the field spots as a bidimensional array of 0s and 1s
	//Where 0 means empty slot and 1 means filled spot
	public class Playfield : MonoBehaviour
	{
		internal enum SpotState { EMPTY_SPOT = 0, FILLED_SPOT = 1 }
		public TetriminoSpawner mSpawner;

		public const int WIDTH = 10;
		public const int HEIGHT = 22;

		public Action OnCurrentPieceReachBottom;
		public Action OnGameOver;
		public Action<int> OnDestroyLine;

		private int[][] mPlayfield = new int[WIDTH][];
		public Tetrimino mNextTetrimino;
		private Tetrimino mCurrentTetrimino;
		private GameSettings mGameSettings;

		private bool firstPiece = true;

		//Constructor of the class.
		//Setting the playfield bidimensional array and creating a reference to piece spawner
		public void setUpPlayfield(GameSettings gameSettings)
		{
			if (!PhotonNetwork.LocalPlayer.IsLocal) return;
			mGameSettings = gameSettings;

			for (int i = 0; i < WIDTH; i++)
			{
				mPlayfield[i] = new int[HEIGHT];
			}

			ResetGame();

			mSpawner.createTetriminoSpawner(mGameSettings.controledRandomMode, mGameSettings.pieces);
		}

		//Resets the array to make all the slots empty
		public void ResetGame()
		{
			firstPiece = true;

			mCurrentTetrimino = null;
			mNextTetrimino = null;
			firstPiece = true;
			for (int i = 0; i < WIDTH; i++)
			{
				for (int j = 0; j < HEIGHT; j++)
				{
					mPlayfield[i][j] = (int)SpotState.EMPTY_SPOT;
				}
			}

			if (mGameSettings.debugMode)
				Debug.Log("RESETING GAME");
		}

		//Create a random piece in the engine and returns it
		public Tetrimino CreateTetrimo()
		{
			//mCurrentTetrimino = mSpawner.GetRandomTetrimino();
			//int rotation = RandomGenerator.random.Next(0, mCurrentTetrimino.blockPositions.GetLength(0));
			//Vector2Int position = mCurrentTetrimino.GetInitialPosition(rotation);
			//position.x += WIDTH / 2;

			//mCurrentTetrimino.currentPosition = position;
			//mCurrentTetrimino.currentRotation = rotation;

			//if (mGameSettings.debugMode)
			//	Debug.Log("CREATING TETRIMINO: " + mCurrentTetrimino.name);
			//return mCurrentTetrimino;

			CreateTetrimo(firstPiece);
			if (firstPiece) firstPiece = false;
			return mCurrentTetrimino;
		}

		public void CreateTetrimo(bool firstPiece)
		{
			Tetrimino block = mSpawner.GetRandomTetrimino();
			int rotation = RandomGenerator.random.Next(0, block.blockPositions.GetLength(0));
			Vector2Int position = block.GetInitialPosition(rotation);
			position.x += WIDTH / 2;

			block.currentPosition = position;
			block.currentRotation = rotation;

			if (mGameSettings.debugMode)
				Debug.Log("CREATING TETRIMINO: " + block.name);
			if (firstPiece)
			{
				//Set first block to random block
				mCurrentTetrimino = block;

				//Generate new block for next block
				block = mSpawner.GetRandomTetrimino();
				rotation = RandomGenerator.random.Next(0, block.blockPositions.GetLength(0));
				position = block.GetInitialPosition(rotation);
				position.x += WIDTH / 2;

				block.currentPosition = position;
				block.currentRotation = rotation;

				//Set next block to random block
				mNextTetrimino = block;
			}
			else
			{
				//Set next current block to next block, then generate new block
				mCurrentTetrimino = mNextTetrimino;
				mNextTetrimino = block;
			}
		}

		//If possible, akes the current piece fall, else locks the piece in the playfield and check for full lines
		//Also checks for GameOver
		public void Step()
		{
			if (IsPossibleMovement(mCurrentTetrimino.currentPosition.x, mCurrentTetrimino.currentPosition.y + 1, mCurrentTetrimino, mCurrentTetrimino.currentRotation))
			{
				mCurrentTetrimino.currentPosition = new Vector2Int(mCurrentTetrimino.currentPosition.x, mCurrentTetrimino.currentPosition.y + 1);
			}
			else
			{
				PlaceTetrimino(mCurrentTetrimino);
				DeletePossibleLines();

				if (IsGameOver())
				{
					if (mGameSettings.debugMode)
						Debug.Log("GAME OVER");

					OnGameOver.Invoke();
					return;
				}

				if (mGameSettings.debugMode)
					Dump();

				OnCurrentPieceReachBottom.Invoke();
			}
		}

		//Places 1s wherever needed whena piece either reaches bottom, either collides in a way that is not possible to go lower anymore
		private void PlaceTetrimino(Tetrimino tetrimino)
		{
			for (int i1 = tetrimino.currentPosition.x, i2 = 0; i1 < tetrimino.currentPosition.x + Tetrimino.BLOCK_AREA; i1++, i2++)
			{
				for (int j1 = tetrimino.currentPosition.y, j2 = 0; j1 < tetrimino.currentPosition.y + Tetrimino.BLOCK_AREA; j1++, j2++)
				{
					if (tetrimino.ValidBlock(tetrimino.currentRotation, j2, i2) && InBounds(i1, j1))
					{
						mPlayfield[i1][j1] = (int)SpotState.FILLED_SPOT;
					}
				}
			}
		}

		//Checks the first line for 1s, if any, Game Over is true
		public bool IsGameOver()
		{
			for (int i = 0; i < WIDTH; i++)
			{
				if (mPlayfield[i][0] == (int)SpotState.FILLED_SPOT)
					return true;
			}

			return false;
		}

		//Deletes a line in the playfield
		//Also makes the pieces below that line to move 1 spot down
		private void DeleteLine(int y)
		{
			if (mGameSettings.debugMode)
				Debug.Log("DESTROYING LINE: " + y);
			for (int j = y; j > 0; j--)
			{
				for (int i = 0; i < WIDTH; i++)
				{
					mPlayfield[i][j] = mPlayfield[i][j - 1];
				}
			}
			OnDestroyLine.Invoke(y);
		}

		//Checks for full lines, if any, deletes it
		private void DeletePossibleLines()
		{
			for (int j = 0; j < HEIGHT; j++)
			{
				int i = 0;
				while (i < WIDTH)
				{
					if (mPlayfield[i][j] != (int)SpotState.FILLED_SPOT) break;
					i++;
				}

				if (i == WIDTH) DeleteLine(j);
			}
		}

		//If the spot is 0, returns true
		private bool IsFreeBlock(int pX, int pY)
		{
			return mPlayfield[pX][pY] == (int)SpotState.EMPTY_SPOT;
		}

		//Check if the movemente is valid before it occours.
		//It takes Tetrimino as a parameter to check its position and rotation;
		public bool IsPossibleMovement(int x, int y, Tetrimino tetrimino, int rotation)
		{
			for (int i1 = x, i2 = 0; i1 < x + Tetrimino.BLOCK_AREA; i1++, i2++)
			{
				for (int j1 = y, j2 = 0; j1 < y + Tetrimino.BLOCK_AREA; j1++, j2++)
				{
					if (i1 < 0 ||
						i1 > WIDTH - 1 ||
						j1 > HEIGHT - 1)
					{
						if (tetrimino.ValidBlock(rotation, j2, i2))
							return false;
					}

					if (j1 >= 0)
					{
						if ((tetrimino.ValidBlock(rotation, j2, i2)) &&
							(!IsFreeBlock(i1, j1)))
							return false;
					}
				}
			}

			return true;
		}

		//Check if (x, y) is inside the playfield
		private bool InBounds(int x, int y)
		{
			return x >= 0 && x < WIDTH && y >= 0 && y < HEIGHT;
		}

		//Method used to debug the field, it logs the spots
		public void Dump()
		{
			string playfield = "";
			for (int i = 0; i < HEIGHT; i++)
			{
				for (int j = 0; j < WIDTH; j++)
				{
					playfield += string.Format("[{0}]", mPlayfield[j][i]);
				}
				playfield += Environment.NewLine;
			}

			Debug.Log(playfield);
		}
	}
}
