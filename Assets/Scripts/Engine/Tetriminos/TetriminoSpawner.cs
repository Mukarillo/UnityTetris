using System.Collections.Generic;

namespace TetrisEngine.TetriminosPiece
{
	//Class responsable for generating random pieces
	//if mControledRandom is true, it makes sure no piece is choosen twice befere all other types are choosen
    //if not, a random type is choosen
    public class TetriminoSpawner
    {
		private List<TetriminoSpecs> mAllTetriminos = new List<TetriminoSpecs>();
		private List<TetriminoSpecs> mAvailableTetriminos = new List<TetriminoSpecs>();

		private bool mControledRandom;

		public TetriminoSpawner(bool controledRandom, List<TetriminoSpecs> allTetriminos)      
		{
			mAllTetriminos = allTetriminos;
			mControledRandom = controledRandom;
		}

		public Tetrimino GetRandomTetrimino()
		{
			if (mControledRandom)
			{
				//if the list is empty, it creates a new one with all the tetriminos inside the project and chooses one to return
				if (mAvailableTetriminos.Count == 0)
					mAvailableTetriminos = GetFullTetriminoBaseList();

				var tetriminoSpecs = mAvailableTetriminos[RandomGenerator.random.Next(0, mAvailableTetriminos.Count)];
				mAvailableTetriminos.Remove(tetriminoSpecs);
				return new Tetrimino(tetriminoSpecs);
			}
                    
            //creates an instance a random object
			return new Tetrimino(mAllTetriminos[RandomGenerator.random.Next(0, mAllTetriminos.Count)]);
		}

		private List<TetriminoSpecs> GetFullTetriminoBaseList()
		{
			var allTetriminos = new List<TetriminoSpecs>(mAllTetriminos);
			return allTetriminos;
		}
    }
}
