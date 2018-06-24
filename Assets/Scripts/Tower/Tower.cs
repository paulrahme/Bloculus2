using UnityEngine;
using System.Collections.Generic;
using System;

public partial class Tower : MonoBehaviour
{
	// Enums & constants
	public enum GameModes { Original, Arcade, TimeChallenge, SpeedChallenge, ScoreChallenge };

	#region Inspector variables

	[Header("Hierarchy")]
	[SerializeField] GameObject			selectorLeft = null;
	[SerializeField] GameObject			selectorRight = null;
	[SerializeField] AudioSource		audioSourceXposZpos = null;
	[SerializeField] AudioSource		audioSourceXnegZpos = null;
	[SerializeField] AudioSource		audioSourceXposZneg = null;
	[SerializeField] AudioSource		audioSourceXnegZneg = null;

	[Header("Prefabs")]
	[SerializeField] BlockDefinition[]	blockDefs = null;
	[SerializeField] GameObject			fallingRingPrefab = null;
	public GameObject					rippleRingPrefab = null;
	public GameObject					blockDisappearPrefab = null;

	[Header("Audio")]
	[SerializeField] AudioClip[]		blockDisappearAudio = null;
	[SerializeField] AudioClip[]		selectorMoveAudio = null;
	[SerializeField] AudioClip[]		selectorSwitchAudio = null;

	[Header("Gameplay Tuning & Balancing")]
	[SerializeField] float				towerRadius = 4.0f;
	[SerializeField] float				minCameraDistance = 6.0f;
	[SerializeField] int				levelMin = 1;
	[SerializeField] int				levelMax = 33;
	[SerializeField] float				fallSpeedSlowest = 2.0f;
	[SerializeField] float				fallSpeedFastest = 7.0f;
	[SerializeField] float				levelIncreaseRateFull = 0.00666f;
	[SerializeField] float				levelIncreaseRateShorter = 0.01666f;
	[SerializeField] float				levelIncreaseRateArcade = 0.02333f;
	[SerializeField] float				newBlockAppearRateSlowest = 4.5f;
	[SerializeField] float				newBlockAppearRateFastest = 2.0f;
	[SerializeField] int				columnsMin = 10;
	[SerializeField] int				columnsMax = 30;
	[SerializeField] int				rowsMin = 5;
	[SerializeField] int				rowsMax = 16;
	[SerializeField] int				blockTypesMin = 4;
	[SerializeField] int				ringFillCapacityMin = 45;
	[SerializeField] int				ringFilleCapacityMax = 150;
	[SerializeField] float				selectorSwapAnimSpeed = 9.0f;

	#endregion	// Inspector variables

	internal int columns = 12;
	internal int rows = 10;
	int currentBlockTypes = 6;
	float blockScale;
	public System.Random randomGen = new System.Random();
	float level;
	int levelInt;
	int blockStyle;
	Stack<Block>[] blockPool;	// Array of stacks per block type
	float startingLevel;
	public GameModes gameMode;
	Block[] blocks;
	int selectorLeftCol, selectorRow;
	float newBlockTimer;
	int score;
	float fallSpeed;
	float newBlockAppearRate;
	float scoreDifficultyMult;
	bool joystickDirectionHeld;
	float selectorSwapAnimOffset;
	string highScoreName;
	float levelIncreaseRate;
	int playerBarCapacity;
	int playerBarValue;
	AudioSource selectorAudioSource;
	List<int> blocksToDelete = new List<int>();

	// Quick helper functions
	int		BlockIdx(int _col, int _row) { return (_row * columns) + _col; }
	Block	GetBlock(int _col, int _row) { return blocks[BlockIdx(_col, _row)]; }
	bool	IsBlockAboutToShiftDown(Block _block) { return ((_block.row != 0) && (GetBlock(_block.col, _block.row - 1) == null)); }
	void	DeleteTemporaryObjects() { foreach (GameObject tempObject in GameObject.FindGameObjectsWithTag("TemporaryObject")) { GameObject.Destroy(tempObject); }	}
	float	GetLevelPercent() { return ((level - Convert.ToSingle(levelMin)) / Convert.ToSingle(levelMax - levelMin)); }
	float	GetLevelPercentCapped() { return Mathf.Min(GetLevelPercent(), 1.0f); }
	bool	IsPlayerBarFull() { return (playerBarValue >= playerBarCapacity); }
	void	SetScore(int _score) { score = _score; }
	bool	DoesGameModeSupportSaving() { return (gameMode == GameModes.Original); }

	/// <summary> Singleton instance </summary>
	public static Tower instance;

	#region Block pool
	
	/// <summary> Gets a block off the stack, or creates a new one </summary>
	/// <param name='_blockID'> Block's ID, used for matching with other blocks</param>
	/// <param name='_col'> Column in the tower </param>
	/// <param name='_row'> Row (0 = bottom row) in the tower </param>
	/// <returns> The new/recycled block </returns>
	public Block GetNewBlock(int _blockID, int _col, int _row)
	{
		Stack<Block> thisBlocksStack = blockPool[_blockID];

		// Have a pooled one to recycle?
		if (thisBlocksStack.Count > 0)
		{
			Block block = thisBlocksStack.Pop();
			block.Setup(_blockID, transform, columns, towerRadius, blockScale, _col, _row);
			return block;
		}
		else
		{
			// Create new block from prefab
			BlockDefinition blockDef = blockDefs[_blockID];
			GameObject prefab = (blockStyle == 0) ? blockDef.prefabSolid : blockDef.prefabWithInnerShape;
			GameObject gameObj = Instantiate(prefab) as GameObject;
			Block block = new Block(gameObj, blockDefs[_blockID]);
			block.Setup(_blockID, transform, columns, towerRadius, blockScale, _col, _row);
			return block;
		}
	}

	/// <summary> Adds a block to the stack for recycling </summary>
	/// <param name="block"> Block to add </param>
	public void RecycleBlock(Block block)
	{
		blockPool[block.blockID].Push(block);
		block.gameObj.transform.parent = null;//gDisabledGameObjectPool;
	}

	public void EmptyRecyclePool()
	{
		foreach (Stack<Block> blockStack in blockPool)
		{
			while (blockStack.Count > 0)
				Destroy(blockStack.Pop().gameObj);
		}
	}
	
	#endregion	// Block pool

	/// <summary> Called when object/script initiates </summary>
	void Awake()
	{
		if (instance != null)
			throw new UnityException("Singleton instance already exists");
		instance = this;

		selectorAudioSource = selectorLeft.GetComponent<AudioSource>();
		blockStyle = PlayerPrefs.GetInt(Constants.ppBlockStyle, 1);
//		gProgressBarMaxScaleY = gProgressBarPlayerBar.localScale.y;

		PrepareObjectPools();
	}

	/// <summary> Called before the first Update() </summary>
	void Start()
	{
		SetNewLevel(0.0f, true);
		SetGameMode(GameModes.Original);
		ResetTower();
	}

	void ResetTower()
	{
		ClearBlocks(true);
		DeleteTemporaryObjects();
		RefreshTower(false);
		ResetScore();
		RestoreSpeeds();
	}

	/// <summary> Creates & prepares the stacks for recycling objects </summary>
	void PrepareObjectPools()
	{
		int blockPoolSize = blockDefs.Length;
		blockPool = new Stack<Block>[blockPoolSize];
		for (int i = 0; i < blockPoolSize; ++i)
		{
			blockPool[i] = new Stack<Block>();
		}
	}

	/// <summary> Sets a new game mode & performs any appropriate actions </summary>
	/// <param name='_gameMode'> eGameMode.... name </param>
	public void SetGameMode(GameModes _gameMode)
	{
		gameMode = _gameMode;
		switch (gameMode)
		{
			case GameModes.Original:
				SetStartingLevel(PlayerPrefs.GetInt(Constants.ppkPPStartingLevel, 1));
				levelIncreaseRate = levelIncreaseRateFull;
				break;
		
			case GameModes.Arcade:
				SetStartingLevel(1);
				levelIncreaseRate = levelIncreaseRateArcade;
				break;
		
			case GameModes.TimeChallenge:
			case GameModes.SpeedChallenge:
				SetStartingLevel(PlayerPrefs.GetInt(Constants.ppkPPStartingLevel, 1));
				levelIncreaseRate = 0;
				break;
		
			case GameModes.ScoreChallenge:
				SetStartingLevel(PlayerPrefs.GetInt(Constants.ppkPPStartingLevel, 1));
				levelIncreaseRate = levelIncreaseRateShorter;
				break;
		
			default:
				throw new Exception("Unhandled GameMode '"+gameMode+"'");
		}
	}

	/// <summary> Sets the starting level and adjusts game speeds accordingly </summary>
	/// <param name='level'> New level to start game on </param>
	public void SetStartingLevel(int level)
	{
		startingLevel = level;
		RestoreSpeeds();
		UpdateBackground(false);
	}

	/// <summary> Updates the background colour, texture etc. </summary>
	/// <param name="_changeMusic"> When true, change background music as necessary </param>
	void UpdateBackground(bool _changeMusic)
	{
		int musicIdx = Environment.instance.SetBackground(level, levelMax);
		if (_changeMusic)
			Environment.instance.musicController.StartGameMusic(musicIdx);

	}

	/// <summary> Resets the current speed values to their starting values </summary>
	void RestoreSpeeds()
	{
		level = startingLevel;
		levelInt = Mathf.FloorToInt(startingLevel);
		SetNewLevel(GetLevelPercent(), false);
		newBlockTimer = newBlockAppearRate;
	}
	
	/// <summary> Prepares the tower from the current settings </summary>
	/// <param name='_createRandomBlocks'> When true, creates a new bunch of random blocks </param>
	public void RefreshTower(bool _createRandomBlocks)
	{
		// Calculate scale for block transforms
		blockScale = towerRadius * 6.0f / columns;
		
		// Set up starting blocks
		blocks = new Block[columns * (rows + 1)];	// 1 extra row for block generators
		if (_createRandomBlocks)
			CreateRandomBlocks();
		TowerCamera.instance.StartBlendingPos(rows * blockScale / 2.0f, minCameraDistance - (blockScale * rows));
		newBlockTimer = newBlockAppearRate;
		
		// Initialise selector boxes
		if (_createRandomBlocks)
			SetSelectorPos(columns - 1, 2);
		selectorLeft.transform.localScale = selectorRight.transform.localScale = new Vector3(blockScale, blockScale, blockScale);
	}		

	/// <summary> Starts the disappear anim & recycles the block </summary>
	/// <param name="_block"> Block to remove </param>
	/// <param name="_disappearAnim"> When true, play the disappearing animation </param>
	public void ClearBlock(Block _block, bool _disappearAnim)
	{
		if (_disappearAnim)
			BlockDisappear.StartDisappearing(_block);

		blocks[BlockIdx(_block.col, _block.row)] = null;
		RecycleBlock(_block);
	}
	
	/// <summary> Clears all blocks from the tower </summary>
	/// <param name="_disappearAnim"> When true, play the disappearing animation </param>
	public void ClearBlocks(bool _disappearAnim)
	{
		if (blocks == null) { return; }

		for (int row = 0; row <= rows; ++row)	// Note rows + 1, for new block generators
		{
			for (int col = 0; col < columns; ++col)
			{
				Block block = GetBlock(col, row);
				if (block != null)
					ClearBlock (block, _disappearAnim);
			}
		}
	}
	
	/// <summary> Sets the 3d falling drops to pause/unpause </summary>
	/// <param name='_paused'> True to pause, false to unpause </param>
	void PauseDropsAndShockwaves(bool _paused)
	{
		foreach (GameObject tempObject in GameObject.FindGameObjectsWithTag("TemporaryObject"))
		{
			FallAndDisappear fallScript = tempObject.GetComponent<FallAndDisappear>();
			if (fallScript != null)
				fallScript.enabled = !_paused;
			else
			{
				RippleGrowAndFade growScript = tempObject.GetComponent<RippleGrowAndFade>();
				if (growScript != null)
					growScript.enabled = !_paused;
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
				if ((randomGen.Next() & 4) != 0)
				{
					int blockIdx = randomGen.Next() % currentBlockTypes;
					blocks[BlockIdx(col, row)] = GetNewBlock(blockIdx, col, row);
					GetBlock(col, row).fallingOffset = (float)randomGen.NextDouble();
				}
				else
				{
					blocks[BlockIdx(col, row)] = null;
				}
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
	
	/// <summary> Triggered as soon as the frontend disappears and the game begins </summary>
	public void GameHasBegun()
	{
		switch (gameMode)
		{
			case GameModes.Original:
			case GameModes.Arcade:
				break;
				
			case GameModes.TimeChallenge:
			case GameModes.SpeedChallenge:
//				timeChallengeStartTime = Time.fixedTime;
				break;
				
			case GameModes.ScoreChallenge:
//				timeChallengeStartTime = Time.fixedTime;
				break;

			default:
				throw new Exception("Unhandled GameMode "+gameMode);
		}
		UpdateBackground(true);
	}
	
	/// <summary> Called once per frame </summary>
	void Update()
	{
		float dTime = Time.deltaTime;

#if UNITY_STANDALONE
		UpdateKeyboard();
#endif
		UpdateJoystick();
		UpdateSelectorSwapAnim(dTime);
		UpdateNewBlocks(dTime);
		UpdateBlocks(dTime);
		UpdateLevelProgress(dTime);
		GroundController.Instance.UpdateEffect();

		// Filled bar?
		if (IsPlayerBarFull())
		{
			// Ensure player bar has not overflowed
			playerBarValue = playerBarCapacity;

			// Trigger level complete sequence
//			LevelComplete();
		}

#if UNITY_EDITOR
		HandleScreenshotKey();
#endif
	}
	
#if UNITY_EDITOR
	/// <summary> Handles debug key/s for saving screenshots </summary>
	void HandleScreenshotKey()
	{
		// Save screenshot
		if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftShift))
		{
			string fileName = "Screenshots/"+Screen.width+"x"+Screen.height+"_"+System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm_ss")+".png";
			ScreenCapture.CaptureScreenshot(fileName);
			Debug.Log("Saved screenshot '"+fileName+"'");
		}
	}
#endif

	/// <summary> Restarts with the previous settings </summary>
	public void ReplayGame()
	{
		ClearBlocks(true);
		DeleteTemporaryObjects();
		RestoreSpeeds();
		ResetScore();
		CreateRandomBlocks();
		GameHasBegun();
	}

	/// <summary> Closes the popup window & continues gameplay </summary>
	public void UnpauseGame()
	{
		PauseDropsAndShockwaves(false);
		Environment.instance.musicController.UnpauseGameMusic();
	}

	/// <summary> Resets the score & the jar </summary>
	void ResetScore()
	{
		SetScore(0);
		ResetPlayerBar();
	}
	
	/// <summary> Starts the Game Over sequence </summary>
	void GameOver()
	{
		Environment.instance.GameOver();
	}
	
	/// <summary> Resets the player's progress bar </summary>
	void ResetPlayerBar()
	{
		playerBarValue = 0;
	}

/*
	/// <summary> Starts a new state in the "level complete" sequence </summary>
	/// <param name='state'> eLevelCompleteStates.... value </param>
	void LevelCompleteSetState(LevelStates state)
	{
		gLCStateTime = gLCPrevTimeOffset = Time.fixedTime;
		gLCState = state;
	}	

	/// <summary> Prepares & pops up the "level complete" sequence </summary>
	public void LevelComplete()
	{
		DeleteTemporaryObjects();
		gLCDropBonus = 0;
		gLCFullJarBonus = 0;
		gLCLevelJustCompleted = levelInt;
//		gLevelCompleteBonusTextMesh.text = string.Empty;
		gLCJarFull = IsPlayerBarFull();
		switch (gameMode)
		{
			case GameModes.Original:
				if (gLCJarFull)
					Environment.instance.LevelComplete();
				else
					Environment.instance.LevelEnded();
				LevelCompleteSetState(LevelStates.Popup);
//				PopupWindow(gLevelCompleteObject);
//				gLevelCompleteButtonsObject.SetActive(false);
//				gLevelCompleteTitleTextMesh.text = "LEVEL "+gLCLevelJustCompleted+ (gLCJarFull ? "\nCOMPLETED!" : "\nHAS ENDED.");
				break;
			
			case GameModes.Arcade:
				if (gLCJarFull)
				{
					Environment.instance.musicController.PauseGameMusic();
					LevelCompleteSetState(LevelStates.JarCount);
//					PopupWindow(gLevelCompleteObject);
//					gLevelCompleteButtonsObject.SetActive(false);
//					gLevelCompleteTitleTextMesh.text = jarFullTitleString;
				}
				else
				{
					QuickLevelUp();
				}
				break;
			
			case GameModes.TimeChallenge:
			case GameModes.SpeedChallenge:
				level = levelInt + 1.0f;
				if ((gameMode == GameModes.SpeedChallenge) || (Mathf.FloorToInt(level) >= startingLevel + 5))
				{
					SetScore(Mathf.FloorToInt((Time.fixedTime - gTimeChallengeStartTime) * 1000.0f));	// *1000 to preserve milliseconds
					Environment.instance.LevelComplete();
				}
				else
				{
					Environment.instance.LevelComplete(true);
					QuickLevelUp(false);
					ResetPlayerBar();
				}
				break;
			
			case GameModes.ScoreChallenge:
				if (Mathf.FloorToInt(level) >= startingLevel + 5)
					Environment.instance.LevelEnded();
				else
					QuickLevelUp();
				break;
		}
//		gLevelCompleteBonusTextMesh.text = string.Empty;
	}
*/	
	/// <summary> Triggers the "Level up" fading text </summary>
	/// <param name='_playSound'> True to play the "level up" sound </param>
	void QuickLevelUp(bool _playSound = true)
	{
		if (_playSound)
			Environment.instance.LevelUp();

		UpdateBackground(true);
	}


	/// <summary> Updates the gradual level increase, reacting if the level has been completed </summary>
	/// <param name='_dTime'> Time elapsed since last Update() </param>
	void UpdateLevelProgress(float _dTime)
	{
		level += levelIncreaseRate * _dTime;
		
		// Has level just changed?
		if (Mathf.FloorToInt(level) != levelInt)
		{
			// Update speeds & tower layout for next level
			float levelPercent = GetLevelPercentCapped();
			SetNewLevel(levelPercent, false);
			
			// If it's in gameplay, trigger the "level complete" sequence
//			if (!gFrontendMenuObject.activeSelf && !IsGameFrozen())
//				LevelComplete();

			levelInt = Mathf.FloorToInt(level);
		}
	}
	

	/// <summary> Sets the speeds & tower layout </summary>
	/// <param name='_progressThroughAllLevels'> Speed scale: 0.0f = slowest, 1.0f = fastest </param>
	void SetNewLevel(float _progressThroughAllLevels, bool _resetTower)
	{
		// Update speeds
		fallSpeed = fallSpeedSlowest + ((fallSpeedFastest - fallSpeedSlowest) * _progressThroughAllLevels);
		newBlockAppearRate = newBlockAppearRateSlowest + ((newBlockAppearRateFastest - newBlockAppearRateSlowest) * _progressThroughAllLevels);

		// Update block types & jar capacity
		currentBlockTypes = blockTypesMin + Convert.ToInt32(Convert.ToSingle(blockDefs.Length - blockTypesMin) * _progressThroughAllLevels);
		playerBarCapacity = ringFillCapacityMin + Convert.ToInt32(Convert.ToSingle(ringFilleCapacityMax - ringFillCapacityMin) * _progressThroughAllLevels);

		// Calculate columns & rows for new level
		int newColumns = columnsMin + Convert.ToInt32(Convert.ToSingle(columnsMax - columnsMin) * _progressThroughAllLevels);
		int newRows = rowsMin + Convert.ToInt32(Convert.ToSingle(rowsMax - rowsMin) * _progressThroughAllLevels);

		// Update background effects
		Environment.instance.flowerOfLife.SetMaxActiveMaterials(Mathf.FloorToInt(level));
		Environment.instance.groundController.SetScrollSpeed(_progressThroughAllLevels);

		// Menu selection: always recreate tower with new set of blocks
		if (_resetTower)
		{
			ClearBlocks(true);
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
				ClearBlocks(false);
				columns = newColumns;
				rows = newRows;
				RefreshTower(false);
	
				// Restore old blocks
				RestoreBlocks(oldBlocks);
	
				// Safely release all info structures now
				oldBlocks.Clear();
			}
		}

		// Update score multiplier
		scoreDifficultyMult = _progressThroughAllLevels;
	}

	#region Movement

	/// <summary> Moves the selector left </summary>
	public void MoveLeft()
	{
		selectorAudioSource.PlayOneShot(selectorMoveAudio[0]);
		SetSelectorPos(WrapCol(selectorLeftCol + 1), selectorRow);
		selectorSwapAnimOffset = 0.0f;
	}
	
	/// <summary> Moves the selector right </summary>
	public void MoveRight()
	{
		selectorAudioSource.PlayOneShot(selectorMoveAudio[1]);
		SetSelectorPos(WrapCol(selectorLeftCol - 1), selectorRow);
		selectorSwapAnimOffset = 0.0f;
	}
	
	/// <summary> Moves the selector up </summary>
	public void MoveUp()
	{
		selectorAudioSource.PlayOneShot(selectorMoveAudio[2]);
		SetSelectorPos(selectorLeftCol, Mathf.Min(rows - 1, selectorRow + 1));
		selectorSwapAnimOffset = 0.0f;
	}
	
	/// <summary> Moves the selector down </summary>
	public void MoveDown()
	{
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
			selectorAudioSource.PlayOneShot(selectorSwitchAudio[(selectorLeftCol & 1)]);

			// Swap the blocks
			blocks[(selectorRow * columns) + selectorLeftCol] = oldRight;
			blocks[(selectorRow * columns) + rightCol] = oldLeft;
			
			// Set the new columns, positions & rotations
			if (oldLeft != null)
			{
				oldLeft.col = rightCol;
				oldLeft.gameObj.transform.localEulerAngles = new Vector3(0.0f, Block.CalcAngleDeg(oldLeft.col, columns), 0.0f);
				oldLeft.gameObj.transform.localPosition = Block.CalcPosition(oldLeft.col, oldLeft.row, oldLeft.gameObj.transform.localEulerAngles.y, towerRadius, blockScale);
			}
			if (oldRight != null)
			{
				oldRight.col = selectorLeftCol;
				oldRight.gameObj.transform.localEulerAngles = new Vector3(0.0f, Block.CalcAngleDeg(oldRight.col, columns), 0.0f);
				oldRight.gameObj.transform.localPosition = Block.CalcPosition(oldRight.col, oldRight.row, oldRight.gameObj.transform.localEulerAngles.y, towerRadius, blockScale);
			}
		}
		
		// Start the selector's swap anim
		selectorSwapAnimOffset = 1.0f;
	}

#if UNITY_STANDALONE
	/// <summary> Updates keyboard controls </summary>
	void UpdateKeyboard()
	{
		if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.A))
			MoveLeft();
		else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.D))
			MoveRight();
		else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.W))
			MoveUp();
		else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.S))
			MoveDown();
		else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
			SwitchBlocks();
	}
#endif

	/// <summary> Updates joystick / gamepad controls </summary>
	void UpdateJoystick()
	{
		// Already pressing in a direction?
		if (joystickDirectionHeld)
		{
			joystickDirectionHeld &= ((Input.GetAxis("Horizontal") > -0.1f) && (Input.GetAxis("Horizontal") < 0.1f) &&
									  (Input.GetAxis("Vertical") > -0.1f) && (Input.GetAxis("Vertical") < 0.1f));
		}
		else
		{
			if (Input.GetAxis("Horizontal") < -0.1f)
			{
				joystickDirectionHeld = true;
				MoveLeft();
			}
			else if (Input.GetAxis("Horizontal") > 0.1f)
			{
				joystickDirectionHeld = true;
				MoveRight();
			}
			else if (Input.GetAxis("Vertical") < -0.1f)
			{
				joystickDirectionHeld = true;
				MoveUp();
			}
			else if (Input.GetAxis("Vertical") > 0.1f)
			{
				joystickDirectionHeld = true;
				MoveDown();
			}
		}

		if (Input.GetButtonDown("Switch Blocks"))
			SwitchBlocks();
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
					block.gameObj.transform.localScale = new Vector3(blockScale, blockScale, blockScale);
					if (GetBlock(block.col, block.row - 1) == null)
					{
						ShiftBlockDown(block);
					}
					else if (GetBlock(block.col, block.row - 1).blockID == block.blockID)
					{
						// Special case: landed on a matching block
						ClearBlock(block, true);
						ClearBlock(GetBlock(block.col, block.row - 1), true);
					}
					else
					{
						GameOver();
					}
				}
			}

//			if (!IsGameFrozen())
			{
				// Reset timer
				newBlockTimer = newBlockAppearRate;
				
				// Create next batch of blocks
				int firstBlockIdxToCreate = randomGen.Next() % columns;
				int numBlocksToCreate = randomGen.Next() % columns;
				int prevBlockIdx = -1;
				for (int col = firstBlockIdxToCreate; col < firstBlockIdxToCreate + numBlocksToCreate; ++col)
				{
					int wrappedCol = WrapCol(col);
					int blockIdx = randomGen.Next() % currentBlockTypes;

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
				block.gameObj.transform.localScale = new Vector3(blockScale, blockScale * growScale, blockScale);
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
	void UpdateBlocks(float _dTime)
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
							PlayBlockAudio(block.gameObj.transform.localPosition, block.blockDef.landingAudio, false);

							block.fallingOffset = 0.0f;
							block.gameObj.transform.localPosition = new Vector3(block.gameObj.transform.localPosition.x, (block.row * blockScale), block.gameObj.transform.localPosition.z);
						}
					}
					
					// Check again in case it just landed / shifted
					if (block.fallingOffset > 0.0f)
					{
						// Falling normally
						block.fallingOffset -= fallSpeed * _dTime;
						Vector3 newPos = block.gameObj.transform.localPosition;
						newPos.y = (block.row * blockScale) + (block.fallingOffset * blockScale);
						block.gameObj.transform.localPosition = newPos;
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
					}
				}
			}
		}

		// Delete blocks & handle scoring
		int scoreChain = 0;
		Vector3 scorePopupPos = Vector3.zero;
		foreach (int blockIdx in blocksToDelete)
		{
			Block blockToDelete = blocks[blockIdx];

			// Accumulate score 
			++scoreChain;
			scorePopupPos += blockToDelete.gameObj.transform.position;
			
			// Start block disappearing
			Color blockColor = blockToDelete.GetMainColor();
			ClearBlock(blockToDelete, true);
			
			// If it's a combo, add falling drop
			if (scoreChain > 2)
			{
				// Add 3D object to fall out of the bottom of the tower
				Transform blockTrans = blockToDelete.gameObj.transform;
				GameObject drop = Instantiate(fallingRingPrefab, blockTrans.position, blockTrans.rotation);
				drop.transform.localScale = new Vector3(blockScale * 0.275f, blockScale * 0.275f, blockScale * 0.275f);
				drop.GetComponent<Renderer>().material.color = new Color(blockColor.r * 0.5f, blockColor.g * 0.5f, blockColor.b * 0.5f);
				
				// Add to the player's progress bar
				if (gameMode != GameModes.ScoreChallenge)
					++playerBarValue;
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

			// Give extra score for harder difficulty
			int scoreThisFrame = 1 << scoreChain;
			scoreThisFrame += Convert.ToInt32(Convert.ToSingle(scoreThisFrame) * scoreDifficultyMult);
			scoreThisFrame += Convert.ToInt32(level * 3.0f / Convert.ToSingle(levelMax - levelMin));
			scoreThisFrame *= 10;
			SetScore(score + scoreThisFrame);
		}
	}
	
	#endregion	// Block logic

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
	
	/// <summary>Positions the selectors at the specified col (and col+1) and row. </summary>
	/// <param name='_colLeft'> Column for left half of selector (right half will add 1) </param>
	/// <param name='_row'> Row (for both halves) </param>
	void SetSelectorPos(int _colLeft, int _row)
	{
		selectorLeftCol = _colLeft;
		selectorRow = _row;

		selectorLeft.transform.localEulerAngles = new Vector3(0.0f, Block.CalcAngleDeg(_colLeft, columns), 0.0f);
		selectorLeft.transform.localPosition = Block.CalcPosition(WrapCol(_colLeft), _row, selectorLeft.transform.localEulerAngles.y, towerRadius, blockScale);

		selectorRight.transform.localEulerAngles = new Vector3(0.0f, Block.CalcAngleDeg(WrapCol(_colLeft + 1), columns), 0.0f);
		selectorRight.transform.localPosition = Block.CalcPosition(WrapCol(_colLeft + 1), _row, selectorRight.transform.localEulerAngles.y, towerRadius, blockScale);
	}

	/// <summary> Updates the selector's swapping animation </summary>
	/// <param name='_dTime'> Time elapsed since last Update() </param>
	void UpdateSelectorSwapAnim(float _dTime)
	{
		selectorSwapAnimOffset -= selectorSwapAnimSpeed * _dTime;
		if (selectorSwapAnimOffset < 0.0f) { selectorSwapAnimOffset = 0.0f; }
	
		Vector3 leftPos = Block.CalcPosition(WrapCol(selectorLeftCol), selectorRow, selectorLeft.transform.localEulerAngles.y, towerRadius, blockScale);
		Vector3 rightPos = Block.CalcPosition(WrapCol(selectorLeftCol + 1), selectorRow, selectorRight.transform.localEulerAngles.y, towerRadius, blockScale);

		selectorRight.transform.localPosition = new Vector3(Mathf.Lerp(rightPos.x, leftPos.x, selectorSwapAnimOffset), rightPos.y, Mathf.Lerp(rightPos.z, leftPos.z, selectorSwapAnimOffset));
		selectorLeft.transform.localPosition = new Vector3(Mathf.Lerp(leftPos.x, rightPos.x, selectorSwapAnimOffset), leftPos.y, Mathf.Lerp(leftPos.z, rightPos.z, selectorSwapAnimOffset));
	}
}
