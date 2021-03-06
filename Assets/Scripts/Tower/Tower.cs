using UnityEngine;
using System.Collections.Generic;
using System;

public partial class Tower : MonoBehaviour
{
	#region Inspector variables

	[Header("Hierarchy")]
	[SerializeField] Transform selectorLeft = null;
	[SerializeField] Transform selectorRight = null;
	[SerializeField] AudioSource audioSourceXposZpos = null;
	[SerializeField] AudioSource audioSourceXnegZpos = null;
	[SerializeField] AudioSource audioSourceXposZneg = null;
	[SerializeField] AudioSource audioSourceXnegZneg = null;

	[Header("Prefabs")]
	[SerializeField] Block.BlockDefinition[] blockDefs = null;
	public GameObject blockDisappearPrefab = null;

	[Header("Tuning")]
	[SerializeField] GameTuning tuning = null;

	[Header("Audio")]
	[SerializeField] AudioClip[] blockDisappearAudio = null;
	[SerializeField] AudioClip[] selectorMoveAudio = null;
	[SerializeField] AudioClip[] selectorSwitchAudio = null;

	#endregion	// Inspector variables

	Transform myTrans;
	internal int columns = 12;
	internal int rows = 10;
	int currentBlockTypes = 6;
	internal Vector3 basePosition;
	float blockScale;
	float halfSelectorAngle;
	int blockStyle;
	Stack<Block>[] blockPools;	// Array of stacks per block type
	Block[] blocks;
	int selectorLeftCol, selectorRow;
	float newBlockTimer;
	float fallSpeed;
	float newBlockAppearRate;
	bool joystickDirectionHeld;
	float selectorSwapAnimOffset;
	AudioSource selectorAudioSource;
	List<int> blocksToDelete = new List<int>();
	Vector3 localEulerAngles, targetEulerAngles;
	internal TowerControl controller;
	ControlData controlData = new ControlData();

	// Quick helper functions
	int		BlockIdx(int _col, int _row) { return (_row * columns) + _col; }
	Block	GetBlock(int _col, int _row) { return blocks[BlockIdx(_col, _row)]; }
	bool	IsBlockAboutToShiftDown(Block _block) { return ((_block.row != 0) && (GetBlock(_block.col, _block.row - 1) == null)); }

	#region Block pool
	
	/// <summary> Gets a block off the stack, or creates a new one </summary>
	/// <param name='_blockID'> Block's ID, used for matching with other blocks</param>
	/// <param name='_col'> Column in the tower </param>
	/// <param name='_row'> Row (0 = bottom row) in the tower </param>
	/// <returns> The new/recycled block </returns>
	public Block GetNewBlock(int _blockID, int _col, int _row)
	{
		Stack<Block> thisBlocksStack = blockPools[_blockID];

		// Have a pooled one to recycle?
		if (thisBlocksStack.Count > 0)
		{
			Block block = thisBlocksStack.Pop();
			block.Setup(blockDefs[_blockID], _blockID, transform, columns, tuning.towerRadius, blockScale, _col, _row);
			return block;
		}
		else
		{
			// Create new block from prefab
			Block.BlockDefinition blockDef = blockDefs[_blockID];
			GameObject prefab = (blockStyle == 0) ? blockDef.prefabSolid : blockDef.prefabWithInnerShape;
			Block block = Instantiate(prefab).GetComponent<Block>();
			block.Setup(blockDefs[_blockID], _blockID, transform, columns, tuning.towerRadius, blockScale, _col, _row);
			return block;
		}
	}

	/// <summary> Adds a block to the stack for recycling </summary>
	/// <param name="block"> Block to add </param>
	public void RecycleBlock(Block block)
	{
		blockPools[block.blockID].Push(block);
		block.gameObject.SetActive(false);
	}

	#endregion // Block pool

	/// <summary> Called when object/script initiates </summary>
	void Awake()
	{
		selectorAudioSource = selectorLeft.GetComponent<AudioSource>();
		blockStyle = PlayerPrefs.GetInt(Constants.PPKeys.BlockStyle.ToString(), 1);

		PrepareObjectPools();

		myTrans = transform;
		localEulerAngles = targetEulerAngles = myTrans.localEulerAngles;
	}

	/// <summary> Creates & prepares the stacks for recycling objects </summary>
	void PrepareObjectPools()
	{
		int blockPoolSize = blockDefs.Length;
		blockPools = new Stack<Block>[blockPoolSize];
		for (int i = 0; i < blockPoolSize; ++i)
		{
			blockPools[i] = new Stack<Block>();
		}
	}

	/// <summary> Resets the current speed values to their starting values </summary>
	public void RestoreSpeeds(int _level, bool _resetTower)
	{
		float progressThroughAllLevels = ((_level - 1f) / (GameMaster.Tuning.levelMax - 1f));
		if (progressThroughAllLevels > 1f)
			progressThroughAllLevels = 1f;

		SetNewLevel(progressThroughAllLevels, _resetTower);
		newBlockTimer = newBlockAppearRate;
		RefreshSelectorPos();
	}
	
	/// <summary> Prepares the tower from the current settings </summary>
	/// <param name='_createRandomBlocks'> When true, creates a new bunch of random blocks </param>
	public void RefreshTower(bool _createRandomBlocks)
	{
		// Set distance away from the camera
		transform.localPosition = new Vector3(basePosition.x, basePosition.y, basePosition.z + (columns * 0.5f));
		
		// Calculate scale for block transforms
		blockScale = tuning.towerRadius * 6.0f / columns;
		halfSelectorAngle = 360.0f / (float)columns * 0.5f;
		
		// Set up starting blocks
		blocks = new Block[columns * (rows + 1)];	// 1 extra row for block generators
		if (_createRandomBlocks)
			CreateRandomBlocks();
		TowerCamera.instance.StartBlending(rows * blockScale * 0.5f, tuning.minCameraDistance + rows);
		newBlockTimer = newBlockAppearRate;
		
		// Initialise selector boxes
		if (_createRandomBlocks)
			SetSelectorPos(columns - 1, 2);
		selectorLeft.localScale = selectorRight.localScale = new Vector3(blockScale, blockScale, blockScale);
	}		

	/// <summary> Starts the disappear anim & recycles the block </summary>
	/// <param name="_block"> Block to remove </param>
	/// <param name="_disappearAnim"> When true, play the disappearing animation </param>
	public void ClearBlock(Block _block, bool _disappearAnim)
	{
		if (_disappearAnim)
			BlockDisappear.StartDisappearing(_block, blockDisappearPrefab);

		blocks[BlockIdx(_block.col, _block.row)] = null;
		RecycleBlock(_block);
	}

	/// <summary> Clears all blocks from the tower </summary>
	public void ClearAllBlocks()
	{
		if (blocks == null)
			return;

		for (int row = 0; row <= rows; ++row)   // Note rows + 1, for new block generators
		{
			for (int col = 0; col < columns; ++col)
			{
				Block block = GetBlock(col, row);
				if (block != null)
					ClearBlock(block, false);
			}
		}
	}

	/// <summary> Sets up some random blocks in the tower </summary>
	void CreateRandomBlocks()
	{
		for (int row = 0; row < rows; ++row)
		{
			for (int col = 0; col < columns; ++col)
			{
				// Create a block randomly
				if ((GameMaster.randomGen.Next() & 4) != 0)
				{
					int blockIdx = GameMaster.randomGen.Next() % currentBlockTypes;
					blocks[BlockIdx(col, row)] = GetNewBlock(blockIdx, col, row);
					GetBlock(col, row).fallingOffset = (float)GameMaster.randomGen.NextDouble();
				}
				else
					blocks[BlockIdx(col, row)] = null;
			}
		}
	}

	/// <summary> Finds the topmost block in the specified column </summary>s>
	/// <param name="_col"> Column number </param>
	/// <returns> The topmost block, or null if the column is empty </return>
	private Block FindTopmostBlock(int _col)
	{
		int rowNo = rows - 1;
		Block block;
		do
		{
			block = GetBlock(_col, rowNo--);
		}
		while ((block == null) && (rowNo >= 0));

		return block;
	}
	
	/// <summary> Called from GameMaster's Update() </summary>
	public void UpdateTower(float _dTime, out int _scoreThisFrame)
	{
		// Read & react to controls
		controller.UpdateControls(_dTime, ref controlData);
		if (controlData.horizontalDir == ControlData.HorizontalDirs.Left)
			MoveLeft();
		else if (controlData.horizontalDir == ControlData.HorizontalDirs.Right)
			MoveRight();
		if (controlData.verticalDir == ControlData.VerticalDirs.Up)
			MoveUp();
		else if (controlData.verticalDir == ControlData.VerticalDirs.Down)
			MoveDown();
		if (controlData.switchBlocks)
			SwitchBlocks();

		UpdateSelectorSwapAnim(_dTime);
		UpdateNewBlocks(_dTime);
		_scoreThisFrame = UpdateBlocks(_dTime);
		UpdateRotation(_dTime);
	}
	
	/// <summary> Restarts with the previous settings </summary>
	public void ReplayGame(int _level)
	{
		RestoreSpeeds(_level, true);
	}

	/// <summary> Sets the speeds & tower layout </summary>
	/// <param name='_progressThroughAllLevels'> Speed scale: 0.0f = slowest, 1.0f = fastest </param>
	public void SetNewLevel(float _progressThroughAllLevels, bool _resetTower)
	{
		// Update speeds
		fallSpeed = tuning.fallSpeedSlowest + ((tuning.fallSpeedFastest - tuning.fallSpeedSlowest) * _progressThroughAllLevels);
		newBlockAppearRate = tuning.newBlockAppearRateSlowest + ((tuning.newBlockAppearRateFastest - tuning.newBlockAppearRateSlowest) * _progressThroughAllLevels);

		// Update block types
		currentBlockTypes = tuning.blockTypesMin + Convert.ToInt32(Convert.ToSingle(blockDefs.Length - tuning.blockTypesMin) * _progressThroughAllLevels);

		// Calculate columns & rows for new level
		int newColumns = tuning.columnsMin + Convert.ToInt32(Convert.ToSingle(tuning.columnsMax - tuning.columnsMin) * _progressThroughAllLevels);
		int newRows = tuning.rowsMin + Convert.ToInt32(Convert.ToSingle(tuning.rowsMax - tuning.rowsMin) * _progressThroughAllLevels);

		// Menu selection: always recreate tower with new set of blocks
		if (_resetTower)
		{
			ClearAllBlocks();
			columns = newColumns;
			rows = newRows;
			RefreshTower(true);
		}
		// Regular gameplay - if the columns or rows have changed, update the tower preserving the previous blocks
		else
		{
			// Columns/rows changed?
			if((columns != newColumns) || (rows != newRows))
			{
				// Backup blocks
				List<SavedBlockInfo> oldBlocks = BackupBlocks();
				
				// Create (bigger) tower
				ClearAllBlocks();
				columns = newColumns;
				rows = newRows;
				RefreshTower(false);
	
				// Restore old blocks
				RestoreBlocks(oldBlocks);
	
				// Safely release all info structures now
				oldBlocks.Clear();
			}
		}
	}

	/// <summary> Updates the tower rotating so that the selectors are in front of the camera </summary>
	/// <param name="_dTime"> Time passed since the last Update() </param>
	void UpdateRotation(float _dTime)
	{
		targetEulerAngles.y = 180.0f - (selectorLeft.localEulerAngles.y + halfSelectorAngle);
		while (targetEulerAngles.y > localEulerAngles.y + 180.0f)
			targetEulerAngles.y -= 360.0f;
		while (targetEulerAngles.y < localEulerAngles.y - 180.0f)
			targetEulerAngles.y += 360.0f;
		localEulerAngles += (targetEulerAngles - localEulerAngles) * tuning.rotateSpeed * _dTime;
		myTrans.localEulerAngles = localEulerAngles;
	}

	#region Movement

	/// <summary> Moves the selector left </summary>
	public void MoveLeft()
	{
		if (Environment.instance.musicController.SfxEnabled)
			selectorAudioSource.PlayOneShot(selectorMoveAudio[0]);
		SetSelectorPos(WrapCol(selectorLeftCol + 1), selectorRow);
		selectorSwapAnimOffset = 0.0f;
	}
	
	/// <summary> Moves the selector right </summary>
	public void MoveRight()
	{
		if (Environment.instance.musicController.SfxEnabled)
			selectorAudioSource.PlayOneShot(selectorMoveAudio[1]);

		SetSelectorPos(WrapCol(selectorLeftCol - 1), selectorRow);
		selectorSwapAnimOffset = 0.0f;
	}
	
	/// <summary> Moves the selector up </summary>
	public void MoveUp()
	{
		if (Environment.instance.musicController.SfxEnabled)
			selectorAudioSource.PlayOneShot(selectorMoveAudio[2]);
	
		SetSelectorPos(selectorLeftCol, Mathf.Min(rows - 1, selectorRow + 1));
		selectorSwapAnimOffset = 0.0f;
	}
	
	/// <summary> Moves the selector down </summary>
	public void MoveDown()
	{
		if (Environment.instance.musicController.SfxEnabled)
			selectorAudioSource.PlayOneShot(selectorMoveAudio[3]);
	
		SetSelectorPos(selectorLeftCol, Mathf.Max(0, selectorRow - 1));
		selectorSwapAnimOffset = 0.0f;
	}
	
	/// <summary> Swaps the currently selected blocks </summary>
	public void SwitchBlocks()
	{
		// Get left & right blocks
		Block oldLeft = GetBlock(selectorLeftCol, selectorRow);
		int rightCol = WrapCol(selectorLeftCol + 1);
		Block oldRight = GetBlock(rightCol, selectorRow);
		
		// Ensure neither of them, or the ones below them, are busy falling
		Block belowLeft = ((selectorRow == 0) ? null : GetBlock(selectorLeftCol, selectorRow - 1));
		Block belowRight = ((selectorRow == 0) ? null : GetBlock(rightCol, selectorRow - 1));
		if (((oldLeft == null) || (oldLeft.fallingOffset == 0.0f)) &&
			((oldRight == null) || (oldRight.fallingOffset == 0.0f)) &&
			((belowLeft == null) || (belowLeft.fallingOffset == 0.0f)) &&
			((belowRight == null) || (belowRight.fallingOffset == 0.0f)))
		{
			// Play switch sound
			if (Environment.instance.musicController.SfxEnabled)
				selectorAudioSource.PlayOneShot(selectorSwitchAudio[(selectorLeftCol & 1)]);

			// Swap the blocks
			blocks[(selectorRow * columns) + selectorLeftCol] = oldRight;
			blocks[(selectorRow * columns) + rightCol] = oldLeft;
			
			// Set the new columns, positions & rotations
			if (oldLeft != null)
			{
				oldLeft.col = rightCol;
				oldLeft.trans.localEulerAngles = new Vector3(0.0f, Block.CalcAngleDeg(oldLeft.col, columns), 0.0f);
				oldLeft.trans.localPosition = Block.CalcPosition(oldLeft.col, oldLeft.row, oldLeft.trans.localEulerAngles.y, tuning.towerRadius, blockScale);
			}
			if (oldRight != null)
			{
				oldRight.col = selectorLeftCol;
				oldRight.trans.localEulerAngles = new Vector3(0.0f, Block.CalcAngleDeg(oldRight.col, columns), 0.0f);
				oldRight.trans.localPosition = Block.CalcPosition(oldRight.col, oldRight.row, oldRight.trans.localEulerAngles.y, tuning.towerRadius, blockScale);
			}
		}
		
		// Start the selector's swap anim
		selectorSwapAnimOffset = 1.0f;
	}

	#endregion	// Movement

	#region Block logic

	/// <summary> Updates newly appearing blocks </summary>
	/// <param name='dTime'> Time elapsed since last Update() </param>
	public void UpdateNewBlocks(float dTime)
	{
		newBlockTimer -= dTime;
		if (newBlockTimer <= 0.0f)
		{
			// Blocks finished growing, shift them down onto the actual tower
			for (int col = 0; col < columns; ++col)
			{
				Block block = GetBlock(col, rows);
				if (block != null)
				{
					block.trans.localScale = new Vector3(blockScale, blockScale, blockScale);
					if (GetBlock(block.col, block.row - 1) == null)
						ShiftBlockDown(block);
					else if (GetBlock(block.col, block.row - 1).blockID == block.blockID)
					{
						// Special case: landed on a matching block
						ClearBlock(block, true);
						ClearBlock(GetBlock(block.col, block.row - 1), true);
					}
					else
						GameMaster.instance.GameOver(this);
				}
			}

			// Game still in play?
			if (!GameMaster.instance.IsGameOver)
			{
				// Reset timer
				newBlockTimer = newBlockAppearRate;

				// Create next batch of blocks
				int firstBlockIdxToCreate = GameMaster.randomGen.Next() % columns;
				int numBlocksToCreate = GameMaster.randomGen.Next() % columns;
				int prevBlockIdx = -1;
				for (int col = firstBlockIdxToCreate; col < firstBlockIdxToCreate + numBlocksToCreate; ++col)
				{
					int wrappedCol = WrapCol(col);
					int blockIdx = GameMaster.randomGen.Next() % currentBlockTypes;

					// Avoid matching the block it's falling onto
					Block topMostBlock = FindTopmostBlock(wrappedCol);
					if ((topMostBlock != null) && (blockIdx == topMostBlock.blockID))
					{
						blockIdx = (blockIdx + 1) % currentBlockTypes;
						// Debug.Log("Changed col "+wrappedCol+"'s blockID from "+gBlockPrefabsSolid[topMostBlock.mBlockID]+" to "+gBlockPrefabsSolid[blockIdx]+" (matched topmost block)");
					}
					// Avoid 2 matching blocks next to each other
					else if (blockIdx == prevBlockIdx)
					{
						blockIdx = (blockIdx + 1) % currentBlockTypes;
						// Debug.Log("Changed col "+wrappedCol+"'s blockID from "+gBlockPrefabsSolid[prevBlockIdx]+" to "+gBlockPrefabsSolid[blockIdx]+" (matched block next to it)");
					}

					// Create block
					blocks[BlockIdx(wrappedCol, rows)] = GetNewBlock(blockIdx, wrappedCol, rows);

					prevBlockIdx = blockIdx;
				}
			}
		}
		
		// Update growing
		float growScale = 1.0f - (newBlockTimer / newBlockAppearRate);
		for (int col = 0; col < columns; ++col)
		{
			Block block = GetBlock(col, rows);
			if (block != null)
				block.trans.localScale = new Vector3(blockScale, blockScale * growScale, blockScale);
		}
	}

	/// <summary> Shifts a block into the next position down </summary>
	/// <param name='_block'> Block class to shift </param>
	void ShiftBlockDown(Block _block)
	{
		// Shift it into lower position
		blocks[BlockIdx(_block.col, _block.row - 1)] = _block;
		blocks[BlockIdx(_block.col, _block.row)] = null;
		_block.row--;
		_block.fallingOffset += 1.0f;
	}

	/// <summary> Plays a block's sound effect using the closest audio source </summary>
	/// <param name="_localPosition"> Block's local position (relative to the tower) </param>
	/// <param name="_audioClip"> Audio to play </param>
	/// <param name="_stopCurrentAudio"> When <c>true</c>, stops (interrupts) any currently playing audio </param>
	void PlayBlockAudio(Vector3 _localPosition, AudioClip _audioClip, bool _stopCurrentAudio)
	{
		if (!Environment.instance.musicController.SfxEnabled)
			return;

		AudioSource audioSourceToUse;

		// Find closest AudioSource to use
		if (_localPosition.x > 0)
			audioSourceToUse = (_localPosition.z > 0) ? audioSourceXposZpos : audioSourceXposZneg;
		else
			audioSourceToUse = (_localPosition.z > 0) ? audioSourceXnegZpos : audioSourceXnegZneg;

		// Play the audio, stopping current one if necessary
		if (audioSourceToUse.isPlaying)
		{
			if (_stopCurrentAudio)
			{
				audioSourceToUse.Stop();
				audioSourceToUse.PlayOneShot(_audioClip);
			}
		}
		else
			audioSourceToUse.PlayOneShot(_audioClip);
	}


	/// <summary> Updates the falling & disappearing blocks </summary>
	/// <param name='_dTime'> Time elapsed since last Update() </param>
	/// <returns> Score chain earned this frame </returns>
	int UpdateBlocks(float _dTime)
	{
		blocksToDelete.Clear();

		// Update falling
		for (int row = 0; row < rows; ++row)
		{
			for (int col = 0; col < columns; ++col)
			{
				Block block = GetBlock(col, row);
				if (block != null)
				{
					// Update shifting down / landing
					if (block.fallingOffset <= 0.0f)
					{
						// Empty space below it?
						if ((block.row > 0) && (GetBlock(block.col, block.row - 1) == null))
							ShiftBlockDown(block);
						// Should land?
						else if (block.fallingOffset < 0.0)
						{
							PlayBlockAudio(block.trans.localPosition, block.blockDef.landingAudio, false);

							block.fallingOffset = 0.0f;
							Vector3 pos = block.trans.localPosition;
							pos.y = block.row * blockScale;
							block.trans.localPosition = pos;
						}
					}
					
					// Check again in case it just landed / shifted
					if (block.fallingOffset > 0.0f)
					{
						// Falling normally
						block.fallingOffset -= fallSpeed * _dTime;
						Vector3 pos = block.trans.localPosition;
						pos.y = (block.row * blockScale) + (block.fallingOffset * blockScale);
						block.trans.localPosition = pos;
					}
				}
			}
		}
		
		// Check for matches
		for (int row = 0; row < rows; ++row)
		{
			for (int col = 0; col < columns; ++col)
			{
				Block block = GetBlock(col, row);
				if (block != null)
				{
					// At rest?
					if (block.fallingOffset == 0.0f)
					{
						bool delete = false;

						// Check below
						if (row > 0)
						{
							Block otherBlock = GetBlock(col, row - 1);
							delete = ((otherBlock != null) && otherBlock.CheckForMatch(block.blockID) && !IsBlockAboutToShiftDown(otherBlock));
						}
						// Check above
						if (!delete && (row < rows - 1))
						{
							Block otherBlock = GetBlock(col, row + 1);
							delete = ((otherBlock != null) && otherBlock.CheckForMatch(block.blockID) && !IsBlockAboutToShiftDown(otherBlock));
						}
						// Check to the left
						if (!delete)
						{
							Block otherBlock = GetBlock(WrapCol(col - 1), row);
							delete = ((otherBlock != null) && otherBlock.CheckForMatch(block.blockID) && !IsBlockAboutToShiftDown(otherBlock));
						}
						// Check to the right
						if (!delete)
						{
							Block otherBlock = GetBlock(WrapCol(col + 1), row);
							delete = ((otherBlock != null) && otherBlock.CheckForMatch(block.blockID) && !IsBlockAboutToShiftDown(otherBlock));
						}

						// Mark for deletion if there was a match
						if (delete)
							blocksToDelete.Add(BlockIdx(col, row));
						else
							block.SetFallingCombo(false);
					}
				}
			}
		}

		// Delete blocks & handle scoring
		int scoreChain = 0, comboChain = 0;
		Vector3 scorePopupPos = Helpers.vec3Zero; // TODO: use a persistent popupPos via UIMaster/UI_HUD instead
		int length = blocksToDelete.Count;
		for (int i = 0; i < length; ++i)
		{
			int blockIdx = blocksToDelete[i];
			Block blockToDelete = blocks[blockIdx];

			// Accumulate score 
			++scoreChain;
			scorePopupPos += blockToDelete.trans.position;

			// Was block already falling from a previous match?
			if (blockToDelete.isInFallingCombo)
			{
				++comboChain;
				blockToDelete.SetFallingCombo(false);
			}
			
			// Start block disappearing
			Color blockColor = blockToDelete.GetMainColor();
			ClearBlock(blockToDelete, true);

			// If multiple matches this frame, add falling ring
			if (scoreChain > 2)
			{
				// Add 3D object to fall out of the bottom of the tower
				Transform blockTrans = blockToDelete.trans;
				Environment.instance.SpawnFallingRing(blockTrans.position, blockScale * tuning.fallingRingScaleMult, blockColor);
			}

			// Find any blocks resting on top of it, to set into falling combo state
			Block blockAbove = blockToDelete;
			while ((blockAbove != null) && (blockAbove.row < rows - 1))
			{
				blockAbove = GetBlock(blockAbove.col, blockAbove.row + 1);
				if ((blockAbove != null) && (blockAbove.fallingOffset == 0f))
					blockAbove.SetFallingCombo(true);
			}


			// If it's a combo from a previous match, show FX
			if (comboChain > 0)
			{
				// TODO: Spawn combo PFX on block
			}

			// Add background pulse
			Environment.instance.flowerOfLife.StartPulse(blockColor);
		}
		if (blocksToDelete.Count > 0)
			scorePopupPos /= blocksToDelete.Count;
		
		// Play audio & give score
		if (scoreChain > 0)
		{
			switch (scoreChain)
			{
				case 1:
				case 2:
					PlayBlockAudio(scorePopupPos, blockDisappearAudio[0], true);
					break;
				
				case 3:
					PlayBlockAudio(scorePopupPos, blockDisappearAudio[1], true);
					break;

				case 4:
					PlayBlockAudio(scorePopupPos, blockDisappearAudio[2], true);
					break;
				
				default:
					PlayBlockAudio(scorePopupPos, blockDisappearAudio[3], true);
					break;
			}
		}

		return scoreChain * comboChain;
	}

	#endregion // Block logic

	/// <summary> Returns the number wrapped into the range [0..gColums) </summary>
	/// <param name='_col'> Unwrapped column </param>
	/// <returns> Wrapped column from 0 to (gColumns - 1) </returns>
	int WrapCol(int _col)
	{
		while (_col < 0)
			_col += columns;
		while (_col >= columns)
			_col -= columns;
		return _col;
	}
	
	/// <summary> Positions the selectors at the specified col (and col+1) and row. </summary>
	/// <param name='_colLeft'> Column for left half of selector (right half will add 1) </param>
	/// <param name='_row'> Row (for both halves) </param>
	void SetSelectorPos(int _colLeft, int _row)
	{
		selectorLeftCol = _colLeft;
		selectorRow = _row;

		selectorLeft.localEulerAngles = new Vector3(0.0f, Block.CalcAngleDeg(_colLeft, columns), 0.0f);
		selectorLeft.localPosition = Block.CalcPosition(WrapCol(_colLeft), _row, selectorLeft.localEulerAngles.y, tuning.towerRadius, blockScale);

		selectorRight.transform.localEulerAngles = new Vector3(0.0f, Block.CalcAngleDeg(WrapCol(_colLeft + 1), columns), 0.0f);
		selectorRight.transform.localPosition = Block.CalcPosition(WrapCol(_colLeft + 1), _row, selectorRight.localEulerAngles.y, tuning.towerRadius, blockScale);
	}

	/// <summary> Refreshes the current selectors position, eg. when tower size has changed </summary>
	void RefreshSelectorPos()
	{
		SetSelectorPos(selectorLeftCol, selectorRow);
	}

	/// <summary> Updates the selector's swapping animation </summary>
	/// <param name='_dTime'> Time elapsed since last Update() </param>
	void UpdateSelectorSwapAnim(float _dTime)
	{
		selectorSwapAnimOffset -= tuning.selectorSwapAnimSpeed * _dTime;
		if (selectorSwapAnimOffset < 0.0f) { selectorSwapAnimOffset = 0.0f; }
	
		Vector3 leftPos = Block.CalcPosition(WrapCol(selectorLeftCol), selectorRow, selectorLeft.localEulerAngles.y, tuning.towerRadius, blockScale);
		Vector3 rightPos = Block.CalcPosition(WrapCol(selectorLeftCol + 1), selectorRow, selectorRight.localEulerAngles.y, tuning.towerRadius, blockScale);

		selectorRight.localPosition = new Vector3(Mathf.Lerp(rightPos.x, leftPos.x, selectorSwapAnimOffset), rightPos.y, Mathf.Lerp(rightPos.z, leftPos.z, selectorSwapAnimOffset));
		selectorLeft.localPosition = new Vector3(Mathf.Lerp(leftPos.x, rightPos.x, selectorSwapAnimOffset), leftPos.y, Mathf.Lerp(leftPos.z, rightPos.z, selectorSwapAnimOffset));
	}
}
