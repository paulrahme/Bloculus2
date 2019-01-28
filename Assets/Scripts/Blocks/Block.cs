using UnityEngine;

public class Block : MonoBehaviour
{
	[System.Serializable]
	public class BlockDefinition
	{
		public GameObject prefabWithInnerShape;
		public GameObject prefabSolid;
		public AudioClip landingAudio;
	}

	#region Inspector variables

	[SerializeField] Renderer mainColorRenderer = null;
	[SerializeField] GameObject fallingComboHierarchy = null;

	#endregion // Inspector variables

	internal BlockDefinition blockDef;
	internal int blockID;
	internal int col, row;
	internal float fallingOffset;
	internal bool isInFallingCombo { get; private set; }
	internal Transform trans;

	/// <summary> Sets up the block's column, row, 3D position, etc </summary>
	/// <param name='_blockID'> Block's ID, used for matching with other blocks</param>
	/// <param name='_towerTransform'> Pointer to the tower's main transform </param>
	/// <param name='_totalCols'> Total columns in the tower </param>
	/// <param name='_radius'> Radius of the tower </param>
	/// <param name='_scale'> Scale for the transform </param>
	/// <param name='_col'> Column in the tower </param>
	/// <param name='_row'> Row (0 = bottom row) in the tower </param>
	public void Setup(BlockDefinition _blockDef, int _blockID, Transform _towerTransform, int _totalCols, float _radius, float _scale, int _col, int _row)
	{
		float angleDeg = CalcAngleDeg(_col, _totalCols);

		blockDef = _blockDef;
		blockID = _blockID;
		col = _col;
		row = _row;
		fallingOffset = 0f;
		SetFallingCombo(false);

		if (trans == null)
			trans = transform;
		trans.parent = _towerTransform;
		trans.localEulerAngles = new Vector3(0.0f, angleDeg, 0.0f);
		trans.localPosition = CalcPosition(_col, _row, angleDeg, _radius, _scale);
		trans.localScale = new Vector3(_scale, _scale, _scale);
		gameObject.SetActive(true);
	}

	/// <summary> Sets "falling combo" mode and enables/disables the sub-hierarchy </summary>
	/// <param name="_isInCombo"> True when in combo mode, false if not </param>
	public void SetFallingCombo(bool _isInCombo)
	{
		isInFallingCombo = _isInCombo;
		fallingComboHierarchy.SetActive(isInFallingCombo);
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
		return ((fallingOffset == 0f) && (blockID == _blockID));
	}

	/// <summary> Returns the colour of either the solid block or of the inside shape </summary>
	/// <returns> Colour </returns>
	public Color GetMainColor()
	{
		return mainColorRenderer.material.color;
	}
}
