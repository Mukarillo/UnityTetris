using System;
using System.Collections.Generic;
using TetrisEngine.Tetriminos;

namespace TetrisEngine
{
    public class TetriminoSpawner
    {

		private List<TetriminoBase> mAvailableTetriminos = new List<TetriminoBase>();

		public TetriminoBase GetRandomTetrimino()
		{
			if (mAvailableTetriminos.Count == 0)
				mAvailableTetriminos = GetFullTetriminoBaseList();
			
			var tetrimino = mAvailableTetriminos[RandomGenerator.random.Next(0, mAvailableTetriminos.Count)];
			mAvailableTetriminos.Remove(tetrimino);
			return tetrimino;
		}

		public TetriminoBase GetSpecificTetrimino(TetriminoType type){
			var allTypes = GetFullTetriminoBaseList();

			return allTypes.Find(x => x.type == type);
		}

		private List<TetriminoBase> GetFullTetriminoBaseList()
		{
			return new List<TetriminoBase>
			{
				new TetriminoI(),
				new TetriminoO(),
				new TetriminoT(),
				new TetriminoS(),
				new TetriminoZ(),
				new TetriminoJ(),
				new TetriminoL()
			};
		}

    }
}
