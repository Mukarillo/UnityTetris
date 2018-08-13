using UnityEngine;
using System;
using System.Collections.Generic;

namespace TetrisEngine.TetriminosPiece
{
	//Struct to hold informationa about the tetrimino
	[Serializable]
	public struct TetriminoSpecs
	{
		public string name;
		public Color color;
		public List<int> serializedBlockPositions;
		public Vector2Int[] initialPosition;      
	}
}