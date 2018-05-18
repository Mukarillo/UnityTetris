using System;
using UnityEngine;

namespace TetrisEngine.Tetriminos
{
	public abstract class TetriminoBase
    {
		public Action OnChangePosition;
		public Action OnChangeRotation;

		public bool isLocked;
		public abstract TetriminoType type { get; }
		public abstract Color color { get; }
		public abstract int[][][] blockPositions { get; }
		protected abstract Vector2Int[] initialPosition { get; }

		private Vector2Int mCurrentPosition;
		public Vector2Int currentPosition
		{
			set
			{
				mCurrentPosition = value;
				if(OnChangePosition != null && !isLocked)
				    OnChangePosition.Invoke();
			}
            get
			{
				return mCurrentPosition;
			}
		}

		private int mCurrentRotation;
		public int currentRotation
		{
			set
			{
				mCurrentRotation = value;
				if (OnChangeRotation != null)
					OnChangeRotation.Invoke();
			}
            get
			{
				return mCurrentRotation;
			}
		}
        
        public int NextRotation{ get { return currentRotation + 1 > 3 ? 0 : currentRotation + 1; } }
		public int PreviousRotation { get { return currentRotation - 1 < 0 ? 3 : currentRotation - 1; } }
        
        public int GetBlockType(int rotation, int x, int y)
		{
			return blockPositions[rotation][x][y];
		}

		public Vector2Int GetInitialPosition(int rotation)
		{
			return initialPosition[rotation];
		}
        
		public bool ValidBlock(int rotation, int x, int y)
		{
			return blockPositions[rotation][x][y] != 0;
		}
    }
}
