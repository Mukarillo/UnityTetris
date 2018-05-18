using UnityEngine;

namespace TetrisEngine.Tetriminos
{
	public class TetriminoT : TetriminoBase
    {
		public override TetriminoType type { get { return TetriminoType.T; } }
		public override Color color { get { return Color.magenta; } }

		public override int[][][] blockPositions
		{
			get
			{
				return new int[][][]
                {
                    new int[][]{
                        new int[]{0, 0, 0, 0, 0},
                        new int[]{0, 0, 1, 0, 0},
                        new int[]{0, 0, 1, 1, 0},
                        new int[]{0, 0, 1, 0, 0},
                        new int[]{0, 0, 0, 0, 0}
                    },
                    new int[][]{
                        new int[]{0, 0, 0, 0, 0},
                        new int[]{0, 0, 0, 0, 0},
                        new int[]{0, 1, 1, 1, 0},
                        new int[]{0, 0, 1, 0, 0},
                        new int[]{0, 0, 0, 0, 0}
                    },
                    new int[][]{
                        new int[]{0, 0, 0, 0, 0},
                        new int[]{0, 0, 1, 0, 0},
                        new int[]{0, 1, 1, 0, 0},
                        new int[]{0, 0, 1, 0, 0},
                        new int[]{0, 0, 0, 0, 0}
                    },
                    new int[][]{
                        new int[]{0, 0, 0, 0, 0},
                        new int[]{0, 0, 1, 0, 0},
                        new int[]{0, 1, 1, 1, 0},
                        new int[]{0, 0, 0, 0, 0},
                        new int[]{0, 0, 0, 0, 0}
                    }
                };
			}
		}

		protected override Vector2Int[] initialPosition
		{
			get
			{
				return new Vector2Int[]
                {
                    new Vector2Int(-2, -3),
                    new Vector2Int(-2, -3),
                    new Vector2Int(-2, -3),
                    new Vector2Int(-2, -2)
                };            
			}
		}
	}
}
