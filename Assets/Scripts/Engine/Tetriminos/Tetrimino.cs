using System;
using UnityEngine;

namespace TetrisEngine.TetriminosPiece
{
	//Class that represents a Tetrimino in the engine
    //It requires a TetriminoSpecs as constructor parameter to fill it's properties
	public class Tetrimino
    {
		public const int BLOCK_AREA = 5;
		public const int BLOCK_ROTATIONS = 4;

		public Action OnChangePosition;
		public Action OnChangeRotation;
        
		public bool isLocked;

		public string name { get; private set; }
		public Color color { get; private set; }
		public int[][][] blockPositions { get; private set; }

		private Vector2Int[] initialPosition;      
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
        
		public Tetrimino(TetriminoSpecs specs)
		{
			name = specs.name;         
			color = specs.color;
			initialPosition = specs.initialPosition;

			if(specs.serializedBlockPositions.Count != BLOCK_ROTATIONS * BLOCK_AREA * BLOCK_AREA)
				throw new Exception(
                    string.Format(
                        "The layout of piece {0} is wrong in Json file. It must have {1} rotations of {2}x{3} grid.",
                        name, BLOCK_ROTATIONS, BLOCK_AREA, BLOCK_AREA));

			int position = 0;

			blockPositions = new int[BLOCK_ROTATIONS][][];
            for (int i = 0; i < blockPositions.Length; i++)
            {
                blockPositions[i] = new int[BLOCK_AREA][];
                for (int j = 0; j < blockPositions[i].Length; j++)
                {
                    blockPositions[i][j] = new int[BLOCK_AREA];
                    for (int k = 0; k < blockPositions[i][j].Length; k++)
                    {
						if (specs.serializedBlockPositions[position] != (int)Playfield.SpotState.EMPTY_SPOT &&
						    specs.serializedBlockPositions[position] != (int)Playfield.SpotState.FILLED_SPOT)
                            throw new Exception(
                                string.Format(
									"The layout of piece {0} is wrong in Json file. It contains '{1}' when only {2}s and {3}s are supported.",
                                    name, 
									specs.serializedBlockPositions[position],
									(int)Playfield.SpotState.EMPTY_SPOT,
									(int)Playfield.SpotState.FILLED_SPOT));
						blockPositions[i][j][k] = specs.serializedBlockPositions[position++];
                    }
                }
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
