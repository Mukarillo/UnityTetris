using UnityEngine;
using TetrisEngine.TetriminosPiece;
using System.Collections.Generic;
using pooling;
using Photon.Pun;
using TMPro;
using System;

namespace TetrisEngine
{   

	//This class is responsable for conecting the engine to the view
    //It is also responsable for calling Playfield.Step
	public class GameLogic : MonoBehaviour 
    {
		[Tooltip("File path from the Resources folder to the json settings file.")]
		[SerializeField] string JSON_PATH = @"SupportFiles/GameSettings";
		
		[Header("Multiplayer")]
		[SerializeField] bool multiplayer = false;

		[Header("UI")]
		[SerializeField] GameObject gameOverObject;
		[SerializeField] TextMeshProUGUI scoreDisplay;
		[SerializeField] TextMeshProUGUI gameOverScoreDisplay;

		[Header("Game Logic")]
		[SerializeField] public GameObject tetriminoBlockPrefab;
		[SerializeField] public GameObject tetriminoHolderPrefab;
		[SerializeField] Playfield mPlayfield;
		[SerializeField] public Transform tetriminoParent;
		[SerializeField] public Transform nextTetriminoParent;
		[SerializeField] public Transform tetriminoNext;
		[SerializeField] PhotonView pv;

		[Tooltip("UI")]
		[SerializeField] TextMeshProUGUI controlsDisplay = null;
		[SerializeField] TextMeshProUGUI timerDisplay = null;

		float time = 0;

		[Header("This property will be overriten by GameSettings.json file.")] 
		[Space(-10)]
		[Header("You can play with it while the game is in Play-Mode.")] 
		public float timeToStep = 2f;

		private bool running = false;

		private GameSettings mGameSettings;
		private List<TetriminoView> mTetriminos = new List<TetriminoView>();
		private float mTimer = 0f;
        
		private Pooling<TetriminoBlock> mBlockPool = new Pooling<TetriminoBlock>();    
		private Pooling<TetriminoView> mTetriminoPool = new Pooling<TetriminoView>();

		private int currentPoints = 0;
		private bool newGame = true;
		public bool gameIsPaused = false;
		public bool hasStashed = true;

		// create a one time use bool that gets turned to false on first creation and is reset to false once a new game has started
		private bool firstPiece = true;

		private Tetrimino mCurrentTetrimino
		{
			get
			{
				return (mTetriminos.Count > 0 && !mTetriminos[mTetriminos.Count - 1].isLocked) ? mTetriminos[mTetriminos.Count - 1].currentTetrimino : null;
			}

		}

		private TetriminoView mPreview;
		private TetriminoView mNextView;
		private bool mRefreshPreview;
		private bool mGameIsOver;

        //Regular Unity Start method
        //Responsable for initiating all the pooling systems and the playfield
		public void Start()
		{
		transform.parent = GameObject.FindGameObjectWithTag("Room").transform;
		StartGame();
		}

		[PunRPC]
		public void StartGame() {
			running = true;			

			mBlockPool.createMoreIfNeeded = true;
			mBlockPool.Initialize(tetriminoBlockPrefab, tetriminoParent);

			mTetriminoPool.createMoreIfNeeded = true;
			mTetriminoPool.Initialize(tetriminoHolderPrefab, tetriminoParent);
			mTetriminoPool.OnObjectCreationCallBack += x =>
			{
				x.OnDestroyTetrimoView = DestroyTetrimino;
				x.blockPool = mBlockPool;
			};

			//Checks for the json file
			var settingsFile = Resources.Load<TextAsset>(JSON_PATH);
			if (settingsFile == null)
				throw new System.Exception(string.Format("GameSettings.json could not be found inside {0}. Create one in Window>GameSettings Creator.", JSON_PATH));

			//Loads the GameSettings Json
			var json = settingsFile.text;
			mGameSettings = JsonUtility.FromJson<GameSettings>(json);
			mGameSettings.CheckValidSettings();
			timeToStep = mGameSettings.timeToStep;

			mPlayfield.setUpPlayfield(mGameSettings);
			mPlayfield.OnCurrentPieceReachBottom = CreateTetrimino;
			mPlayfield.OnGameOver = SetGameOver;
			mPlayfield.OnDestroyLine = DestroyLine;

			if (multiplayer)
			{
				gameOverObject.SetActive(false);
				//GameOver.instance.HideScreen();
				//Score.instance.HideScreen();
			}
			else
			{
				GameOver.instance.HideScreen(0f);
				Score.instance.HideScreen();
			}

			//Display player controls from json file
			string controlsText = $"Rotate right: {mGameSettings.rotateRightKey}\n" +
				$"Rotate left: {mGameSettings.rotateLeftKey}\n" +
				$"Move: {mGameSettings.moveLeftKey}/{mGameSettings.moveRightKey}\n" +
				$"Lower: {mGameSettings.moveDownKey}\n" +
				$"Discard block: {mGameSettings.discardPieceKey}";
			controlsDisplay.text = controlsText;

			RestartGame();
			newGame = false;
		}

        //Called when the game starts and when user click Restart Game on GameOver screen
        //Responsable for restaring all necessary components
        public void RestartGame()
		{
			time = 0;
			if (!newGame) { 
			GameObject[] blocks = GameObject.FindGameObjectsWithTag("Blocks");
			foreach(GameObject block in blocks) {
			if (block.GetComponent<PhotonView>().OwnerActorNr == PhotonNetwork.LocalPlayer.ActorNumber) {
				PhotonNetwork.Destroy(block);
			}
			}
			}

			currentPoints = 0;
			pv.RPC("setDisplayText", RpcTarget.All, currentPoints, 0);
			//setDisplayText(0);
			if (!pv.IsMine) return;
			if (multiplayer) { 
			

			}
			else { 
			GameOver.instance.HideScreen();
			Score.instance.ResetScore();
			}

			hasStashed = true;
			firstPiece = false;

            mGameIsOver = false;
			mTimer = 0f;
            
			mPlayfield.ResetGame();
			mTetriminoPool.ReleaseAll();
			mTetriminos.Clear();

			//pv.RPC("CreateTetrimino", RpcTarget.All);
            CreateTetrimino();         
		}
	
        //Callback from Playfield to destroy a line in view
		private void DestroyLine(int y)
		{
			//if (!pv.IsMine) return;
			if (multiplayer) { 

			int value = (mGameSettings.pointsByBreakingLine + currentPoints > 0 && mGameSettings.pointsByBreakingLine + currentPoints < int.MaxValue) ? mGameSettings.pointsByBreakingLine + currentPoints : int.MaxValue;
			currentPoints = value;
			pv.RPC("setDisplayText", RpcTarget.All, currentPoints, 0);
			//setDisplayText(value);

			}
			else { 
			Score.instance.AddPoints(mGameSettings.pointsByBreakingLine);
			}
            
			mTetriminos.ForEach(x => x.DestroyLine(y));
            mTetriminos.RemoveAll(x => x.destroyed == true);
		}

        //Callback from Playfield to show game over in view
		private void SetGameOver()
		{
			if (!pv.IsMine) return;
			mGameIsOver = true;
			if (multiplayer) { 
			gameOverObject.SetActive(true);
			//gameOverScoreDisplay.text = $"Score:\n{currentPoints}";
			pv.RPC("setDisplayText", RpcTarget.All, currentPoints, 1);
			}
			else { 
			GameOver.instance.ShowScreen();
			}
		}

		[PunRPC]
		private void setDisplayText(int value, int ui = 0) {

			string text = $"Score: {value}";
			TextMeshProUGUI	display = (ui == 0) ? scoreDisplay.GetComponent<TextMeshProUGUI>() : gameOverScoreDisplay.GetComponent<TextMeshProUGUI>();
			display.text = text;

		}

		//[PunRPC]
        //Call to the engine to create a new piece and create a representation of the random piece in view
        private void CreateTetrimino()
		{
			hasStashed = false;

			if (mCurrentTetrimino != null)
				mCurrentTetrimino.isLocked = true;

			var tetrimino = mPlayfield.CreateTetrimo();
			var tetriminoView = mTetriminoPool.Collect();
			tetriminoView.InitiateTetrimino(tetrimino);
			mTetriminos.Add(tetriminoView);

			if (mPreview != null)
				mTetriminoPool.Release(mPreview);
			
			mPreview = mTetriminoPool.Collect();
			mPreview.InitiateTetrimino(tetrimino, true);

			//Try to create a nextBlock view
			if (mNextView != null)
				mTetriminoPool.Release(mNextView);

			mNextView = mTetriminoPool.Collect(nextTetriminoParent);
			mNextView.InitiateTetrimino(mPlayfield.mNextTetrimino);
			mRefreshPreview = true;

			if(firstPiece)
			{
				firstPiece = false;
			}
		}


		//When all the blocks of a piece is destroyed, we must release ("destroy") it.
		private void DestroyTetrimino(TetriminoView obj)
		{
			if (!pv.IsMine) return;
			var index = mTetriminos.FindIndex(x => x == obj);
			mTetriminoPool.Release(obj);
			mTetriminos[index].destroyed = true;
		}


		private void checkToStart() {
			if (!running && PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
			{
				running = true;
				pv.RPC("StartGame", RpcTarget.All);
			}
		}

		//Regular Unity Update method
        //Responsable for counting down and calling Step
        //Also responsable for gathering users input
		public void Update()
		{
			//if (!running) checkToStart();
			if (mGameIsOver || mCurrentTetrimino == null) return;
			mTimer += Time.deltaTime;
			time += Time.deltaTime;
			timerDisplay.text = Math.Round(time, 2).ToString();

			if (!pv.IsMine) mTimer = mTimer / PhotonNetwork.PlayerList.Length;

			if(mTimer > timeToStep && mPlayfield != null)
			{
				mTimer = 0;
				mPlayfield.Step();
			}
			if (!pv.IsMine) return;


			/*
			 * Originally the code just did a bunch of if checks and the piece operations were isolated here
			 * I split it up into individual methods so they can be called from external sources if need be and so it is not running
			 * a multi-line if check
			 */

			if (!gameIsPaused)
			{
				//Rotate Right
				if (Input.GetKeyDown(mGameSettings.rotateRightKey))
				{
					rightRotate();
				}

				//Rotate Left
				if (Input.GetKeyDown(mGameSettings.rotateLeftKey))
				{
					leftRotate();
				}

				//Move piece to the left
				if (Input.GetKeyDown(mGameSettings.moveLeftKey))
				{
					moveLeft();
				}

				//Move piece to the right
				if (Input.GetKeyDown(mGameSettings.moveRightKey))
				{
					moveRight();
				}

				//Make the piece fall faster
				//this is the only input with GetKey instead of GetKeyDown, because most of the time, users want to keep this button pressed and make the piece fall
				if (Input.GetKeyDown(mGameSettings.moveDownKey))
				{
					moveDown();
				}

				if (Input.GetKeyDown(mGameSettings.discardPieceKey))
				{
					discardPiece();
				}
			}

			if (Input.GetKeyDown(mGameSettings.pauseKey) && PhotonNetwork.CurrentRoom.MaxPlayers == 1)
			{
				pauseGame();
			}

			//This part is responsable for rendering the preview piece in the right position
			if (mRefreshPreview)
			{
				var y = mCurrentTetrimino.currentPosition.y;
				while(mPlayfield.IsPossibleMovement(mCurrentTetrimino.currentPosition.x,
                                                  y,
                                                  mCurrentTetrimino,
                                                  mCurrentTetrimino.currentRotation))
				{
					y++;
				}

				mPreview.ForcePosition(mCurrentTetrimino.currentPosition.x, y - 1);
				mRefreshPreview = false;
			}
		}

		//individual methods for moving of the pieces
		public void rightRotate()
		{
			if (mPlayfield.IsPossibleMovement(mCurrentTetrimino.currentPosition.x,
												  mCurrentTetrimino.currentPosition.y,
												  mCurrentTetrimino,
												  mCurrentTetrimino.NextRotation))
			{
				mCurrentTetrimino.currentRotation = mCurrentTetrimino.NextRotation;
				mRefreshPreview = true;
			}
		}

		public void leftRotate()
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

		public void moveLeft()
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

		public void moveRight()
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

		public void moveDown()
		{
			if (mPlayfield.IsPossibleMovement(mCurrentTetrimino.currentPosition.x,
												  mCurrentTetrimino.currentPosition.y + 1,
												  mCurrentTetrimino,
												  mCurrentTetrimino.currentRotation))
			{
				mCurrentTetrimino.currentPosition = new Vector2Int(mCurrentTetrimino.currentPosition.x, mCurrentTetrimino.currentPosition.y + 1);
			}
		}

		public void discardPiece()
		{
			if(!firstPiece)
			{
				if (!hasStashed)
				{
					//Debug.Log(mTetriminoPool.Count);

					int index = (mTetriminoPool.Count == 2) ? 0 : mTetriminoPool.Count - 1;
					TetriminoView TV = mTetriminoPool[index];
					DestroyTetrimino(TV);
					mTetriminos.Remove(TV);
					//mTetriminoPool.Release(mTetriminoPool[index]);


					CreateTetrimino();

					hasStashed = true;
				}
			}
		}

		public void pauseGame()
		{
			if(gameIsPaused)
			{
				Time.timeScale = 1;
				gameIsPaused = false;
				//dissable the pause screen here
				Debug.Log("Game has been unpaused");
			}
			else if(!gameIsPaused)
			{
				Time.timeScale = 0;
				gameIsPaused = true;
				//enable the pause screen here
				Debug.Log("Game has been paused");
			}
		}

	}
}
