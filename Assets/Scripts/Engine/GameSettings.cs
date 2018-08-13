using System.Collections.Generic;
using TetrisEngine.TetriminosPiece;
using UnityEngine;

namespace TetrisEngine
{
	//Class responsable for holding the settings of the game
	public class GameSettings
	{
		private const float MIN_TIME_TO_STEP = 0.01f;

		[SerializeField]
		public float timeToStep;

		[SerializeField]
        public int pointsByBreakingLine;

		[SerializeField]
        public bool controledRandomMode;      
		[SerializeField]
        public bool debugMode;

		[SerializeField]
		public KeyCode moveRightKey;
		[SerializeField]
        public KeyCode moveLeftKey;
		[SerializeField]
        public KeyCode moveDownKey;
		[SerializeField]
        public KeyCode rotateRightKey;
		[SerializeField]
        public KeyCode rotateLeftKey;
              
		[SerializeField]
		public List<TetriminoSpecs> pieces;

        public void CheckValidSettings()
		{
			if (timeToStep < MIN_TIME_TO_STEP)
				throw new System.Exception(string.Format("timeToStep inside GameSettings.json must be higher than {0}", MIN_TIME_TO_STEP));
			
			if(pointsByBreakingLine < 0)
				throw new System.Exception("pointsByBreakingLine inside GameSettings.json must be higher or equal 0");
			
			if(moveRightKey == KeyCode.None)
				throw new System.Exception("moveRightKey inside GameSettings.json must different than None");
			if (moveLeftKey == KeyCode.None)
				throw new System.Exception("moveLeftKey inside GameSettings.json must different than None");
			if (moveDownKey == KeyCode.None)
				throw new System.Exception("moveDownKey inside GameSettings.json must different than None");
			if (rotateRightKey == KeyCode.None)
				throw new System.Exception("rotateRightKey inside GameSettings.json must different than None");
			if (rotateLeftKey == KeyCode.None)
				throw new System.Exception("rotateLeftKey inside GameSettings.json must different than None");         
		}
	}
}
