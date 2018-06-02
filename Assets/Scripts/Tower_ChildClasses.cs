using UnityEngine;

public partial class Tower : MonoBehaviour
{
	[System.Serializable]
	public class BlockDefinition
	{
		public GameObject prefabWithInnerShape;
		public GameObject prefabSolid;
		public AudioClip landingAudio;
	}

	/// <summary> Main class for a block in the 3D tower </summary>
	public class Block
	{
		public GameObject gameObj;			// GameObject, or null if empty
		public BlockDefinition blockDef;	// Block data
		public int blockID;					// ID used for matching with other blocks
		public int col, row;				// Which column + row (0 = bottom) it's in
		public float fallingOffset;			// Amount to fall before resting in this position (1.0f = 1 r)

		/// <summary> Constructor </summary>
		/// <param name='_gameObj'> GameObject to use </param>
		public Block(GameObject _gameObj, BlockDefinition _blockDef)
		{
			gameObj = _gameObj;
			blockDef = _blockDef;
		}

		/// <summary> Sets up the block's column, row, 3D position, etc </summary>
		/// <param name='_blockID'> Block's ID, used for matching with other blocks</param>
		/// <param name='_towerTransform'> Pointer to the tower's main transform </param>
		/// <param name='_totalCols'> Total columns in the tower </param>
		/// <param name='_radius'> Radius of the tower </param>
		/// <param name='_scale'> Scale for the transform </param>
		/// <param name='_col'> Column in the tower </param>
		/// <param name='_row'> Row (0 = bottom row) in the tower </param>
		public void Setup(int _blockID, Transform _towerTransform, int _totalCols, float _radius, float _scale, int _col, int _row)
		{
			float angleDeg = CalcAngleDeg(_col, _totalCols);

			this.blockID = _blockID;
			this.col = _col;
			this.row = _row;
			gameObj.transform.parent = _towerTransform;
			gameObj.transform.localEulerAngles = new Vector3(0.0f, angleDeg, 0.0f);
			gameObj.transform.localPosition = CalcPosition(_col, _row, angleDeg, _radius, _scale);
			gameObj.transform.localScale = new Vector3(_scale, _scale, _scale);
		}

		/// <summary> Calculates the position of a block. </summary>
		/// <param name='_col'> Column in the tower </param>
		/// <param name='_row'> Row (0 = bottom row) in the tower </param>
		/// <param name='_angleDeg'> Angle in degrees </param>
		/// <param name='_radius'> Radius of the tower </param>
		/// <param name='_scale'> Scale for the transform </param>
		/// <returns> The (relative) position of the block in 3D space </returns>
		public static Vector3 CalcPosition(int _col, int _row, float _angleDeg, float _radius, float _scale)
		{
			float angleRad = _angleDeg * (2.0f * Mathf.PI) / 360.0f;
			return new Vector3(Mathf.Sin(angleRad) * _radius, _row * _scale, Mathf.Cos(angleRad) * _radius);
		}

		/// <summary> Calculates the block's y rotation angle in degrees </summary>
		/// <param name='_col'> Column in the tower </param>
		/// <param name='_totalCols'> Total columns in the tower </param>
		/// <returns> The y rotation angle (in degrees) </returns>
		public static float CalcAngleDeg(int _col, int _totalCols)
		{
			return (_col * 360 / _totalCols);
		}

		/// <summary> Checks for colour match and valid react state </summary>
		/// <param name='_blockID'> ID to check against </param>
		/// <returns> True if the colour matches and it's in a state to react </returns>
		public bool CheckForMatch(int _blockID)
		{
			return ((fallingOffset == 0.0f) && (blockID == _blockID));
		}

		/// <summary> Returns the colour of either the solid block or of the inside shape </summary>
		/// <returns> Colour </returns>
		public Color GetMainColor()
		{
			Transform insideShapeTrans = gameObj.transform.Find("InsideShape");
			if (insideShapeTrans != null)
				return insideShapeTrans.GetComponent<Renderer>().material.color;
			else
				return gameObj.GetComponent<Renderer>().material.color;
		}
	}
}
