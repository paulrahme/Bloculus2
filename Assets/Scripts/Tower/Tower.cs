using UnityEngine;
using System.Collections.Generic;
using System;

public partial class Tower : MonoBehaviour
{
	// Enums & constants
	public enum GameModes { Original, Arcade, TimeChallenge, SpeedChallenge, ScoreChallenge };
	enum LevelStates { None, Popup, JarCount, JarPause, FullJarCount, FinalPause };

	#region Inspector variables

	[Header("Hierarchy")]
	[SerializeField] GameObject			selectorLeft = null;
	[SerializeField] GameObject			selectorRight = null;
	[SerializeField] GameObject			audioSourceXposZpos = null;
	[SerializeField] GameObject			audioSourceXnegZpos = null;
	[SerializeField] GameObject			audioSourceXposZneg = null;
	[SerializeField] GameObject			audioSourceXnegZneg = null;

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
	[SerializeField] int				levelCompleteBonusPerTick = 100;
	[SerializeField] int				levelCompleteConusPerLevelPercent = 500;
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
	Stack<Block>[] blockPool;
	float startingLevel;
	public GameModes gameMode;
//	int highScore;

	private Block[]							gBlocks;														// Array of blocks
	private int								gSelectorLeftCol, gSelectorRow;									// Position of the left half of the selector
	private float							gNewBlockTimer;													// Countdown until new blocks appear
	private int								gScore;															// Player's current score
	private float							gFallSpeed;														// Falling speed in units/sec relative to block scale
	private float							gNewBlockAppearRate;											// How often (in seconds) new blocks appear
	private float							gScoreDifficultyMult;											// Helper for calculating score depending on difficulty
	private bool							gJoystickDirectionHeld;											// When true, a direction is being pressed
	private LevelStates						gLCState;														// Current state of the "level complete" sequence
	private float							gLCStateTime;													// Fixed time when the current state began
	private float							gLCPrevTimeOffset;												// State's time offset last update/frame
	private bool							gLCJarFull;														// True if jar was filled, false if level ended first
	private int								gLCDropBonus;													// Total bonus accumulated for drops
	private int								gLCFullJarBonus;												// Total bonus accumulated for filling the jar early
	private int								gLCLevelJustCompleted;											// Level that was just completed
	private float							gSelectorSwapAnimOffset;										// Where the selector's
	private string							gHighScoreName;													// Record holder's name (for current starting level)
	private float							gRepeatingSoundTimer;											// When to re-trigger a repeating sound effect
	private float							gLevelIncreaseRate;												// How quickly the level increases during gameplay
	private float							gTimeChallengeStartTime;										// Time at which the time challenge began
	private int								gPlayerBarCapacity;												// How many combos the player needs to fill the progress bar
	private int								gPlayerBarAmount;												// Value of the player's progress bar
	private Vector3							gPlayerBarColorTotal;											// All the colours dropped in, added together

	// Helper/inline functions
	private int								BlockIdx(int col, int row) { return (row * columns) + col; }
	private Block							GetBlock(int col, int row) { return gBlocks[BlockIdx(col, row)]; }
	private bool							IsBlockAboutToShiftDown(Block block) { return ((block.row != 0) && (GetBlock(block.col, block.row - 1) == null)); }
	private void							DeleteTemporaryObjects() { foreach (GameObject tempObject in GameObject.FindGameObjectsWithTag("TemporaryObject")) { GameObject.Destroy(tempObject); }	}
	private float							GetLevelPercent() { return ((level - Convert.ToSingle(levelMin)) / Convert.ToSingle(levelMax - levelMin)); }
	private float							GetLevelPercentCapped() { return Mathf.Min(GetLevelPercent(), 1.0f); }
	private bool							IsPlayerBarFull() { return (gPlayerBarAmount >= gPlayerBarCapacity); }
	private void							SetScore(int score) { gScore = score; }
	private bool							DoesGameModeSupportSaving() { return (gameMode == GameModes.Original); }

	/// <summary> Singleton instance </summary>
	public static Tower instance;

	#region Block pool
	
	/// <summary> Gets a block off the stack, or creates a new one </summary>
	/// <param name='blockID'> Block's ID, used for matching with other blocks</param>
	/// <param name='towerTransform'> Pointer to the tower's main transform </param>
	/// <param name='totalCols'> Total columns in the tower </param>
	/// <param name='radius'> Radius of the tower </param>
	/// <param name='scale'> Scale for the transform </param>
	/// <param name='col'> Column in the tower </param>
	/// <param name='row'> Row (0 = bottom row) in the tower </param>
	/// <returns> The new/recycled block </returns>
	public Block GetNewBlock(int blockID, Transform towerTransform, int totalCols, float radius, float scale, int col, int row)
	{
		Stack<Block> thisBlocksStack = blockPool[blockID];
		if (thisBlocksStack.Count > 0)
		{
			// Recycle block form the pool
			Block block = thisBlocksStack.Pop();
			block.Setup(blockID, transform, columns, towerRadius, blockScale, col, row);
			return block;
		}
		else
		{
			// Create new block from prefab
			BlockDefinition blockDef = blockDefs[blockID];
			GameObject prefab = (blockStyle == 0) ? blockDef.prefabSolid : blockDef.prefabWithInnerShape;
			GameObject gameObj = Instantiate(prefab) as GameObject;
			Block block = new Block(gameObj, blockDefs[blockID]);
			block.Setup(blockID, transform, columns, towerRadius, blockScale, col, row);
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
			{
				GameObject.Destroy(blockStack.Pop().gameObj);
			}
		}
	}
	
	#endregion	// Block pool

	/// <summary> Called when object/script initiates </summary>
	void Awake()
	{
		instance = this;

		blockStyle = PlayerPrefs.GetInt(Constants.ppBlockStyle, 1);
//		gProgressBarMaxScaleY = gProgressBarPlayerBar.localScale.y;

		PrepareObjectPools();
	}

	/// <summary> Called before the first Update() </summary>
	void Start()
	{
		SetNewLevel(0.0f, true);
		SetGameMode(GameModes.Original);
		ShowFrontendMenu();
	}

	/// <summary> Creates & prepares the stacks for recycling objects </summary>
	private void PrepareObjectPools()
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
				gLevelIncreaseRate = levelIncreaseRateFull;
				break;
		
			case GameModes.Arcade:
				SetStartingLevel(1);
				gLevelIncreaseRate = levelIncreaseRateArcade;
				break;
		
			case GameModes.TimeChallenge:
			case GameModes.SpeedChallenge:
				SetStartingLevel(PlayerPrefs.GetInt(Constants.ppkPPStartingLevel, 1));
				gLevelIncreaseRate = 0;
				break;
		
			case GameModes.ScoreChallenge:
				SetStartingLevel(PlayerPrefs.GetInt(Constants.ppkPPStartingLevel, 1));
				gLevelIncreaseRate = levelIncreaseRateShorter;
				break;
		
			default:
				throw new Exception("Unhandled GameMode '"+gameMode+"'");
		}

		// Prepare pause menu to only allow saving in Original mode
//		gPauseSaveAndExitButton.SetActive(gGameMode == GameModes.Original);
//		gPauseExitNoSaveButton.SetActive(gGameMode == GameModes.Original);
//		gPauseQuitButton.SetActive(gGameMode != GameModes.Original);
	}

	/// <summary> Sets the starting level and adjusts game speeds accordingly </summary>
	/// <param name='level'> New level to start game on </param>
	public void SetStartingLevel(int level)
	{
		startingLevel = level;
//		highScore = PlayerPrefs.GetInt(gameMode+Constants.ppHiScore+level, 0);
		gHighScoreName = PlayerPrefs.GetString(gameMode+Constants.ppHiScoreName+level, string.Empty);
		RefreshHiScoreGUIString();
		RestoreSpeeds();
		UpdateBackground(false);
	}

	/// <summary> Updates the background colour, texture etc. </summary>
	/// <param name="changeMusic"> When true, change background music as necessary </param>
	void UpdateBackground(bool changeMusic)
	{
		if (level < 8.0f)
			Environment.instance.SetBackground(0, level / 8.0f, changeMusic ? 0 : -1);
		else if (level < 16.0f)
			Environment.instance.SetBackground(1, (level - 8.0f) / 8.0f, changeMusic ? 1 : -1);
		else if (level < 24.0f)
			Environment.instance.SetBackground(2, (level - 16.0f) / 8.0f, changeMusic ? 2 : -1);
		else if (level < levelMax)
			Environment.instance.SetBackground(3, (level - 24.0f) / (levelMax - 24.0f), changeMusic ? 3 : -1);
		else
			Environment.instance.SetBackground(4, 0, changeMusic ? 4 : -1);
	}


	/// <summary> Creates the high score + name (if set) for displaying on the GUI </summary>
	void RefreshHiScoreGUIString()
	{
/*		// Score
		if ((gGameMode == GameModes.TimeChallenge) || (gGameMode == GameModes.SpeedChallenge))
		{
			int milliseconds = gHighScore % 1000;
			int seconds = (gHighScore / 1000) % 60;
			int minutes = ((gHighScore / 1000) /60);
			gGUITextHiScore.text = string.Format("Best Time:\n{0,00}:{1:00}.{2:000}", minutes, seconds, milliseconds);
		}
		else
		{
			gGUITextHiScore.text = "High Score:\n"+gHighScore;
		}
		
		// Record holder's name
		if (gHighScore > 0)
		{
			gGUITextHiScore.text += "\n("+gHighScoreName+")";
		}

		// Also update the one in the GUI
		gGUITextHiScoreFrontend.text = gGUITextHiScore.text.Replace("\n", " ");
*/	}
	
	
	/// <summary> Saves the high score + record holder's name </summary>
	/// <param name="name"> Record holder's name </param>
	public void SaveHiScoreEntry(string name)
	{
		gHighScoreName = ((name == string.Empty) ? "Anonymous" : name);
//		highScore = gScore;
		PlayerPrefs.SetInt(gameMode+Constants.ppHiScore+Convert.ToInt32(startingLevel), gScore);
		PlayerPrefs.SetString(gameMode+Constants.ppHiScoreName+Convert.ToInt32(startingLevel), gHighScoreName);
		PlayerPrefs.Save();
//		gGameOverObjectHiScoreNameEntry.SetActive(false);
//		gGameOverObjectHiScoreShare.SetActive(true);
		RefreshHiScoreGUIString();
	}
	
	
	/// <summary> Resets the current speed values to their starting values </summary>
	void RestoreSpeeds()
	{
		level = startingLevel;
		levelInt = Mathf.FloorToInt(startingLevel);
//		gGUITextLevel.text = "Level\n"+gLevelInt;
		SetNewLevel(GetLevelPercent(), false);
		gNewBlockTimer = gNewBlockAppearRate;
	}
	
	
	/// <summary> Prepares the tower from the current settings </summary>
	/// <param name='createNewBlocks'> When true, creates a new bunch of random blocks </param>
	public void RefreshTower(bool createNewBlocks)
	{
		// Calculate scale for block transforms
		blockScale = towerRadius * 6.0f / columns;
		
		// Set up starting blocks
		gBlocks = new Block[columns * (rows + 1)];	// 1 extra row for block generators
		if (createNewBlocks)
		{
			CreateRandomBlocks();
		}
		TowerCamera.instance.StartBlendingPos(rows * blockScale / 2.0f, minCameraDistance - (blockScale * rows));
		gNewBlockTimer = gNewBlockAppearRate;
		
		// Initialise selector boxes
		if (createNewBlocks)
		{
			SetSelectorPos(columns - 1, 2);
		}
		selectorLeft.transform.localScale = selectorRight.transform.localScale = new Vector3(blockScale, blockScale, blockScale);
	}
		

	/// <summary> Starts the disappear anim & recycles the block </summary>
	/// <param name="block"> Block to remove </param>
	/// <param name="disappearAnim"> When true, play the disappearing animation </param>
	public void ClearBlock(Block block, bool disappearAnim)
	{
		if (disappearAnim) { BlockDisappear.StartDisappearing(block); }
		gBlocks[BlockIdx(block.col, block.row)] = null;
		RecycleBlock(block);
	}
	
	
	/// <summary> Clears all blocks from the tower </summary>
	/// <param name="disappearAnim"> When true, play the disappearing animation </param>
	public void ClearBlocks(bool disappearAnim)
	{
		if (gBlocks == null) { return; }

		for (int row = 0; row <= rows; ++row)	// Note rows + 1, for new block generators
		{
			for (int col = 0; col < columns; ++col)
			{
				Block block = GetBlock(col, row);
				if (block != null)
				{
					ClearBlock (block, disappearAnim);
				}
			}
		}
	}

	
	/// <summary> Sets the 3d falling drops to pause/unpause </summary>
	/// <param name='enabled'> True to pause, false to unpause </param>
	private void PauseDropsAndShockwaves(bool paused)
	{
		foreach (GameObject tempObject in GameObject.FindGameObjectsWithTag("TemporaryObject"))
		{
			FallAndDisappear fallScript = tempObject.GetComponent<FallAndDisappear>();
			if (fallScript != null)
			{
				fallScript.enabled = !paused;
			}
			else
			{
				RippleGrowAndFade growScript = tempObject.GetComponent<RippleGrowAndFade>();
				if (growScript != null)
				{
					growScript.enabled = !paused;
				}
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
					gBlocks[BlockIdx(col, row)] = GetNewBlock(blockIdx, transform, columns, towerRadius, blockScale, col, row);
					GetBlock(col, row).fallingOffset = (float)randomGen.NextDouble();
				}
				else
				{
					gBlocks[BlockIdx(col, row)] = null;
				}
			}
		}
	}

	/// <summary> Finds the topmost block in the specified column </summary>s>
	/// <param name="col"> Column number </param>
	/// <returns> The topmost block, or null if the column is empty </return>
	private Block FindTopmostBlock(int col)
	{
		int rowNo = rows - 1;
		Block block;
		do
		{
			block = GetBlock(col, rowNo--);
		}
		while ((block == null) && (rowNo >= 0));

		return block;
	}
	
	
	/// <summary> Triggered as soon as the frontend disappears and the game begins </summary>
	public void GameHasBegun()
	{
//		gGUITextLevel.gameObject.SetActive(true);
//		gGUITextHiScore.gameObject.SetActive(true);
		switch (gameMode)
		{
			case GameModes.Original:
			case GameModes.Arcade:
//				gGUITextScore.gameObject.SetActive(true);
//				gGUITextTime.gameObject.SetActive(false);
//				gProgressBarLevel.SetActive(true);
//				gProgressBarPlayer.SetActive(true);
				break;
				
			case GameModes.TimeChallenge:
			case GameModes.SpeedChallenge:
//				gGUITextScore.gameObject.SetActive(false);
//				gGUITextTime.gameObject.SetActive(true);
//				gProgressBarLevel.SetActive(false);
//				gProgressBarPlayer.SetActive(true);
				gTimeChallengeStartTime = Time.fixedTime;
				break;
				
			case GameModes.ScoreChallenge:
//				gGUITextScore.gameObject.SetActive(true);
//				gGUITextTime.gameObject.SetActive(false);
//				gProgressBarLevel.SetActive(true);
//				gProgressBarPlayer.SetActive(false);
				gTimeChallengeStartTime = Time.fixedTime;
				break;

			default:
				throw new Exception("Unhandled GameMode "+gameMode);
		}
//		UpdateGameplayProgressBar(gProgressBarPlayerBar, 0.0f, Vector3.zero);
//		UpdateGameplayProgressBar(gProgressBarLevelBar, 0.0f, Vector3.zero);
		UpdateBackground(true);
//		gInGameGUI.SetActive(true);
	}
	
	/// <summary> Called once per frame </summary>
	void Update()
	{
		float dTime = Time.deltaTime;
		
		UpdateKeyboard();
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
			gPlayerBarAmount = gPlayerBarCapacity;
//			UpdateGameplayProgressBar(gProgressBarPlayerBar, 1.0f, gPlayerBarColorTotal / (float)(gPlayerBarAmount));

			// Trigger level complete sequence
			LevelComplete();
		}

#if UNITY_EDITOR
		HandleScreenshotKey();
#endif
	}
	
	
#if UNITY_EDITOR
	/// <summary> Handles debug key/s for saving screenshots </summary>
	private void HandleScreenshotKey()
	{
		// Save screenshot
		if (Input.GetKeyDown(KeyCode.G))
		{
			string fileName = "Screenshots/"+Screen.width+"x"+Screen.height+"_"+System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm_ss")+".png";
			ScreenCapture.CaptureScreenshot(fileName);
			Debug.Log("Saved screenshot '"+fileName+"'");
		}
	}
#endif

	/// <summary> Exit to the frontend menu without saving the game </summary>
	public void ExitNoSave()
	{
		ShowFrontendMenu();
	}

	/// <summary> Update when 'Game Over' is active </summary>
	void UpdateGameOver()
	{
//		if (gGameOverObjectHiScoreShare.activeSelf)
		{
			if (Input.GetKeyDown(KeyCode.R))
			{
				ReplayGame();
			}
			else if (Input.GetKeyDown(KeyCode.Escape))
			{
				ExitNoSave();
			}
		}
	}
	

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
	
	
	/// <summary> Update when 'Pause' is active </summary>
	void UpdatePause()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			UnpauseGame();
		}
		else if (Input.GetKeyDown(KeyCode.Escape))
		{
			ShowFrontendMenu();
		}
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

//		PopupWindow(gGameOverObject);
		
		// Check if highest level reached has increased
		if (levelInt > PlayerPrefs.GetInt(Constants.ppHighestLevel))
		{
			PlayerPrefs.SetInt(Constants.ppHighestLevel, levelInt);
		}

/*		// Check if high score has been broken
		bool beatHiScore;
		if ((gGameMode == GameModes.TimeChallenge) || (gGameMode == GameModes.SpeedChallenge))
			beatHiScore = ((gameOverType != GameOverTypes.PlayerDied) && ((gHighScore == 0) || (gScore < gHighScore)));
		else
			beatHiScore = (gScore > gHighScore);

		if (beatHiScore)
			gGameOverObjectHiScoreNameEntry.SetActive(true);
	else
			gGameOverObjectHiScoreNameEntry.SetActive(false);

		gGameOverObjectHiScoreShare.SetActive(false);
*/	}
	
	
	/// <summary> Resets the player's progress bar </summary>
	private void ResetPlayerBar()
	{
		gPlayerBarAmount = 0;
		gPlayerBarColorTotal = Vector3.zero;
//		UpdateGameplayProgressBar(gProgressBarPlayerBar, 0.0f, Vector3.zero);
	}
	

	/// <summary> Updates the size & colour of the on-screen progress bar </summary>
	/// <param name='progressBarTransform'> Transform of the progress bar to update </param>
	/// <param name='percent'> Value from 0.0f to 1.0f representing progress </param>
	/// <param name='colorRGB'> Colour to set the renderer's material </param>
	private void UpdateGameplayProgressBar(Transform progressBarTransform, float percent, Vector3 colorRGB)
	{
/*		float scaleY = percent * gProgressBarMaxScaleY;
		progressBarTransform.localPosition = new Vector3(progressBarTransform.localPosition.x, scaleY / 2.0f, progressBarTransform.localPosition.z);
		progressBarTransform.localScale = new Vector3(progressBarTransform.localScale.x, scaleY, progressBarTransform.localScale.z);
		progressBarTransform.GetComponent<Renderer>().material.color = new Color(colorRGB.x, colorRGB.y, colorRGB.z, progressBarTransform.GetComponent<Renderer>().material.color.a);
*/	}
	

	/// <summary> Starts a new state in the "level complete" sequence </summary>
	/// <param name='state'> eLevelCompleteStates.... value </param>
	private void LevelCompleteSetState(LevelStates state)
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
	
	/// <summary> Triggers the "Level up" fading text </summary>
	/// <param name='playSound'> True to play the "level up" sound </param>
	void QuickLevelUp(bool playSound = true)
	{
		if (playSound)
			Environment.instance.LevelUp();
//		gLevelUp3DTextScript.ResetAnim();
//		gLevelUp3DTextScript.gameObject.SetActive(true);
		UpdateBackground(true);
	}
	
	/// <summary> Updates the "level complete/ended" popup sequence between levels </summary>
	void UpdateLevelComplete()
	{
		// Quit?
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			ResetPlayerBar();
			ShowFrontendMenu();
			return;
		}

		float fixedTime = Time.fixedTime;
		float timeOffset = fixedTime - gLCStateTime;
		switch (gLCState)
		{
			case LevelStates.Popup:
				if (timeOffset > 1.75f)
				{
					gRepeatingSoundTimer = 0.0f;
					LevelCompleteSetState(LevelStates.JarCount);
				}
				else if ((timeOffset >= 1.0f) && (gLCPrevTimeOffset < 1.0f))
				{
//					gLevelCompleteBonusTextMesh.text = jarBonusString;
//					if (gLCJarFull) { gLevelCompleteBonusTextMesh.text += "\n"; }
				}
				break;

			case LevelStates.JarCount:
				if (gPlayerBarAmount > 0)
				{
					if (fixedTime > gRepeatingSoundTimer)
					{
//						GetComponent<AudioSource>().PlayOneShot(gAudioJarEmptying);
						gRepeatingSoundTimer = fixedTime + 0.2f;
					}
					if (timeOffset > 0.03f)
					{
						gLCStateTime = Time.fixedTime;
						--gPlayerBarAmount;
//						UpdateGameplayProgressBar(gProgressBarPlayerBar, (float)(gPlayerBarAmount) / (float)(gPlayerBarCapacity), gPlayerBarColorTotal / (float)(gPlayerBarAmount));
						gLCDropBonus += levelCompleteBonusPerTick;
						SetScore(gScore + levelCompleteBonusPerTick);
					}
				}
				else
				{
					if (gameMode == GameModes.Arcade)
					{
						LevelCompleteSetState(LevelStates.FinalPause);
					}
					else
					{	
						LevelCompleteSetState(LevelStates.JarPause);
					}
				}
				// Bonus string, with new line if jar full
//				gLevelCompleteBonusTextMesh.text = jarBonusString+gLCDropBonus;
//				if (gLCJarFull) { gLevelCompleteBonusTextMesh.text += "\n"; }
				break;
			
			case LevelStates.JarPause:
				if (timeOffset > 1.0f)
				{
					if (gLCJarFull)
					{
						gRepeatingSoundTimer = 0.0f;
						LevelCompleteSetState(LevelStates.FullJarCount);
					}
					else
					{
//						gLevelCompleteButtonsObject.SetActive(true);
						LevelCompleteSetState(LevelStates.FinalPause);
					}
				}
				if ((gLCJarFull) && (timeOffset >= 0.5f) && (gLCPrevTimeOffset < 0.5f))
				{
					Environment.instance.LevelUp();
//					gLevelCompleteBonusTextMesh.text = jarBonusString+gLCDropBonus+jarFullString;
				}
				break;
			
			case LevelStates.FullJarCount:
				if (Mathf.FloorToInt(level) == gLCLevelJustCompleted)
				{
					if (fixedTime > gRepeatingSoundTimer)
					{
//						GetComponent<AudioSource>().PlayOneShot(gAudioBarFilling);
						gRepeatingSoundTimer = fixedTime + 0.15f;
					}
					gLCFullJarBonus += levelCompleteConusPerLevelPercent / 4;
					SetScore(gScore + (levelCompleteConusPerLevelPercent / 4));
					UpdateLevelProgress(0.25f);
				}
				else
				{
//					gLevelCompleteButtonsObject.SetActive(true);
					LevelCompleteSetState(LevelStates.FinalPause);
				}
//				gLevelCompleteBonusTextMesh.text = jarBonusString+gLCDropBonus+jarFullStringPlus+gLCFullJarBonus;
				break;

			case LevelStates.FinalPause:
				if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || (gameMode == GameModes.Arcade))
				{
					StartNextLevel();
				}
				break;
			
			default:
				throw new Exception("Unhandled eLevelCompleteState '"+gLCState+"'");
		}
		
		// Save time offset for next frame
		gLCPrevTimeOffset = timeOffset;
	}
	

	/// <summary> Closes the popup window and continues gameplay </summary>
	public void StartNextLevel()
	{
		ResetPlayerBar();
		UpdateBackground(true);
	}
	
	
	/// <summary> Info needed to recreate blocks when adding a column </summary>
	class BlockInfo
	{
		public BlockInfo(int col, int row, int blockID, float fallingOffset)
		{
			mCol = col;
			mRow = row;
			mBlockID = blockID;
			mFallingOffset = fallingOffset;
		}
		
		public int mCol, mRow;
		public int mBlockID;
		public float mFallingOffset;
	};
	

	/// <summary> Updates the gradual level increase, reacting if the level has been completed </summary>
	/// <param name='dTime'> Time elapsed since last Update() </param>
	void UpdateLevelProgress(float dTime)
	{
		level += gLevelIncreaseRate * dTime;
		
		// Has level just changed?
		if (Mathf.FloorToInt(level) != levelInt)
		{
			// Update speeds & tower layout for next level
			float levelPercent = GetLevelPercentCapped();
			SetNewLevel(levelPercent, false);
			
			// If it's in gameplay, trigger the "level complete" sequence
			// if (!gFrontendMenuObject.activeSelf && !IsGameFrozen())
			{
				LevelComplete();
			}

			levelInt = Mathf.FloorToInt(level);
//			gGUITextLevel.text = "Level\n"+gLevelInt;
		}
		
		// GameMode specific updates
		switch (gameMode)
		{
			case GameModes.TimeChallenge:
			case GameModes.SpeedChallenge:
//				TimeSpan timeSpan = TimeSpan.FromSeconds(Time.fixedTime - gTimeChallengeStartTime);
//				gGUITextTime.text = string.Format("Time:\n{0:00}:{1:00}.{2:000}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
				break;
			
			default:
				float levelPercent = level - (float)(levelInt);
				if ((levelPercent < 0.9f) /*|| IsGameFrozen()*/ || ((Mathf.FloorToInt(Time.fixedTime * 3.0f) & 1) == 0))	// Flash when almost full
				{
//					UpdateGameplayProgressBar(gProgressBarLevelBar, 1.0f - levelPercent, (levelPercent > 0.8f) ? levelProgressBarVecRed : levelProgressBarVecGreen);
//					gProgressBarLevelBar.GetComponent<Renderer>().enabled = true;
				}
				else
				{
//					gProgressBarLevelBar.GetComponent<Renderer>().enabled = false;
				}
				break;
		}
	}
	

	/// <summary> Sets the speeds & tower layout </summary>
	/// <param name='_progressThroughAllLevels'> Speed scale: 0.0f = slowest, 1.0f = fastest </param>
	void SetNewLevel(float _progressThroughAllLevels, bool _resetTower)
	{
		// Update speeds
		gFallSpeed = fallSpeedSlowest + ((fallSpeedFastest - fallSpeedSlowest) * _progressThroughAllLevels);
		gNewBlockAppearRate = newBlockAppearRateSlowest + ((newBlockAppearRateFastest - newBlockAppearRateSlowest) * _progressThroughAllLevels);

		// Update block types & jar capacity
		currentBlockTypes = blockTypesMin + Convert.ToInt32(Convert.ToSingle(blockDefs.Length - blockTypesMin) * _progressThroughAllLevels);
		gPlayerBarCapacity = ringFillCapacityMin + Convert.ToInt32(Convert.ToSingle(ringFilleCapacityMax - ringFillCapacityMin) * _progressThroughAllLevels);

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
				List<BlockInfo> oldBlocks = BackupBlocks();
				
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
		gScoreDifficultyMult = _progressThroughAllLevels;
	}
	
	
	/// <summary> Backs up the blocks into an info list </summary>
	/// <returns> List of BlockInfo classes describing the blocks </returns>
	private List<BlockInfo> BackupBlocks()
	{
		List<BlockInfo> blockInfoList = new List<BlockInfo>();
		blockInfoList.Clear();
		for (int i = 0; i < gBlocks.Length; ++i)
		{
			if (gBlocks[i] != null)
			{
				Block block = gBlocks[i];
				blockInfoList.Add(new BlockInfo(block.col, block.row, block.blockID, block.fallingOffset));
			}
		}
		
		return blockInfoList;
	}
	
	
	/// <summary> Restores the blocks from an info list </summary>
	/// <param name='blockInfoList'> List of BlockInfo classes to restore from </param>
	private void RestoreBlocks(List<BlockInfo> blockInfoList)
	{
		foreach (BlockInfo info in blockInfoList)
		{
			gBlocks[BlockIdx(info.mCol, info.mRow)] = GetNewBlock(info.mBlockID, transform, columns, towerRadius, blockScale, info.mCol, info.mRow);
			GetBlock(info.mCol, info.mRow).fallingOffset = info.mFallingOffset;
		}
	}
	
	
	/// <summary> Brings up the main menu </summary>
	public void ShowFrontendMenu()
	{
		// TODO: Close menu, EOR popup, etc here?

		// Recreate tower
		ClearBlocks(true);
		DeleteTemporaryObjects();
		RefreshTower(false);
		ResetScore();

		// Restore starting speeds (will also recreate tower by calling LevelChanged())
		RestoreSpeeds();
	}


	/// <summary> Moves the selector left </summary>
	public void MoveLeft()
	{
		selectorLeft.GetComponent<AudioSource>().PlayOneShot(selectorMoveAudio[0]);
		SetSelectorPos(WrapCol(gSelectorLeftCol + 1), gSelectorRow);
		gSelectorSwapAnimOffset = 0.0f;
	}

	
	/// <summary> Moves the selector right </summary>
	public void MoveRight()
	{
		selectorLeft.GetComponent<AudioSource>().PlayOneShot(selectorMoveAudio[1]);
		SetSelectorPos(WrapCol(gSelectorLeftCol - 1), gSelectorRow);
		gSelectorSwapAnimOffset = 0.0f;
	}

	
	/// <summary> Moves the selector up </summary>
	public void MoveUp()
	{
		selectorLeft.GetComponent<AudioSource>().PlayOneShot(selectorMoveAudio[2]);
		SetSelectorPos(gSelectorLeftCol, Mathf.Min(rows - 1, gSelectorRow + 1));
		gSelectorSwapAnimOffset = 0.0f;
	}

	
	/// <summary> Moves the selector down </summary>
	public void MoveDown()
	{
		selectorLeft.GetComponent<AudioSource>().PlayOneShot(selectorMoveAudio[3]);
		SetSelectorPos(gSelectorLeftCol, Mathf.Max(0, gSelectorRow - 1));
		gSelectorSwapAnimOffset = 0.0f;
	}

	
	/// <summary> Swaps the currently selected blocks </summary>
	public void SwitchBlocks()
	{
		// Get left & right blocks
		Block oldLeft = GetBlock(gSelectorLeftCol, gSelectorRow);
		int rightCol = WrapCol(gSelectorLeftCol + 1);
		Block oldRight = GetBlock(rightCol, gSelectorRow);
		
		// Ensure neither of them, or the ones below them, are busy falling
		Block belowLeft = ((gSelectorRow == 0) ? null : GetBlock(gSelectorLeftCol, gSelectorRow - 1));
		Block belowRight = ((gSelectorRow == 0) ? null : GetBlock(rightCol, gSelectorRow - 1));
		if (((oldLeft == null) || (oldLeft.fallingOffset == 0.0f)) &&
			((oldRight == null) || (oldRight.fallingOffset == 0.0f)) &&
			((belowLeft == null) || (belowLeft.fallingOffset == 0.0f)) &&
			((belowRight == null) || (belowRight.fallingOffset == 0.0f)))
		{
			// Play switch sound
			selectorLeft.GetComponent<AudioSource>().PlayOneShot(selectorSwitchAudio[(gSelectorLeftCol & 1)]);

			// Swap the blocks
			gBlocks[(gSelectorRow * columns) + gSelectorLeftCol] = oldRight;
			gBlocks[(gSelectorRow * columns) + rightCol] = oldLeft;
			
			// Set the new columns, positions & rotations
			if (oldLeft != null)
			{
				oldLeft.col = rightCol;
				oldLeft.gameObj.transform.localEulerAngles = new Vector3(0.0f, Block.CalcAngleDeg(oldLeft.col, columns), 0.0f);
				oldLeft.gameObj.transform.localPosition = Block.CalcPosition(oldLeft.col, oldLeft.row, oldLeft.gameObj.transform.localEulerAngles.y, towerRadius, blockScale);
			}
			if (oldRight != null)
			{
				oldRight.col = gSelectorLeftCol;
				oldRight.gameObj.transform.localEulerAngles = new Vector3(0.0f, Block.CalcAngleDeg(oldRight.col, columns), 0.0f);
				oldRight.gameObj.transform.localPosition = Block.CalcPosition(oldRight.col, oldRight.row, oldRight.gameObj.transform.localEulerAngles.y, towerRadius, blockScale);
			}
		}
		
		// Start the selector's swap anim
		gSelectorSwapAnimOffset = 1.0f;
	}
	
	
	/// <summary> Updates keyboard controls </summary>
	private void UpdateKeyboard()
	{
		if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.A))
		{
			MoveLeft();
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.D))
		{
			MoveRight();
		}
		else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.W))
		{
			MoveUp();
		}
		else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.S))
		{
			MoveDown();
		}
		else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
		{
			SwitchBlocks();
		}
	}
	
	/// <summary> Updates joystick / gamepad controls </summary>
	private void UpdateJoystick()
	{
		// Already pressing in a direction?
		if (gJoystickDirectionHeld)
		{
			if ((Input.GetAxis("Horizontal") > -0.1f) && (Input.GetAxis("Horizontal") < 0.1f) &&
				(Input.GetAxis("Vertical") > -0.1f) && (Input.GetAxis("Vertical") < 0.1f))
			{
				gJoystickDirectionHeld = false;
			}
		}
		else
		{
			if (Input.GetAxis("Horizontal") < -0.1f)
			{
				gJoystickDirectionHeld = true;
				MoveLeft();
			}
			else if (Input.GetAxis("Horizontal") > 0.1f)
			{
				gJoystickDirectionHeld = true;
				MoveRight();
			}
			else if (Input.GetAxis("Vertical") < -0.1f)
			{
				gJoystickDirectionHeld = true;
				MoveUp();
			}
			else if (Input.GetAxis("Vertical") > 0.1f)
			{
				gJoystickDirectionHeld = true;
				MoveDown();
			}
		}

		if (Input.GetButtonDown("Switch Blocks"))
		{
			SwitchBlocks();
		}
	}


	/// <summary> Updates newly appearing blocks </summary>
	/// <param name='dTime'> Time elapsed since last Update() </param>
	public void UpdateNewBlocks(float dTime)
	{
		gNewBlockTimer -= dTime;
		if (gNewBlockTimer <= 0.0f)
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
				gNewBlockTimer = gNewBlockAppearRate;
				
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
					gBlocks[BlockIdx(wrappedCol, rows)] = GetNewBlock(blockIdx, transform, columns, towerRadius, blockScale, wrappedCol, rows);
					
					prevBlockIdx = blockIdx;
				}
			}
		}
		
		// Update growing
		float growScale = 1.0f - (gNewBlockTimer / gNewBlockAppearRate);
		for (int col = 0; col < columns; ++col)
		{
			Block block = GetBlock(col, rows);
			if (block != null)
			{
				block.gameObj.transform.localScale = new Vector3(blockScale, blockScale * growScale, blockScale);
			}
		}
	}


	/// <summary> Shifts a block into the next position down </summary>
	/// <param name='block'> Block class to shift </param>
	private void ShiftBlockDown(Block block)
	{
		// Shift it into lower position
		gBlocks[BlockIdx(block.col, block.row - 1)] = block;
		gBlocks[BlockIdx(block.col, block.row)] = null;
		block.row--;
		block.fallingOffset += 1.0f;
	}


	/// <summary> Plays a block's sound effect using the closest audio source </summary>
	/// <param name="localPosition"> Block's local position (relative to the tower) </param>
	/// <param name="audioClip"> Audio to play </param>
	/// <param name="stopCurrentAudio"> When <c>true</c>, stops (interrupts) any currently playing audio </param>
	private void PlayBlockAudio(Vector3 localPosition, AudioClip audioClip, bool stopCurrentAudio)
	{
		AudioSource audioSourceToUse;

		// Find closest AudioSource to use
		if (localPosition.x > 0)
		{
			audioSourceToUse = (localPosition.z > 0) ? audioSourceXposZpos.GetComponent<AudioSource>() : audioSourceXposZneg.GetComponent<AudioSource>();
		}
		else
		{
			audioSourceToUse = (localPosition.z > 0) ? audioSourceXnegZpos.GetComponent<AudioSource>() : audioSourceXnegZneg.GetComponent<AudioSource>();
		}

		// Play the audio, stopping current one if necessary
		if (audioSourceToUse.isPlaying)
		{
			if (stopCurrentAudio)
			{
				audioSourceToUse.Stop();
				audioSourceToUse.PlayOneShot(audioClip);
			}
		}
		else
		{
			audioSourceToUse.PlayOneShot(audioClip);
		}
	}


	/// <summary> Updates the falling & disappearing blocks </summary>
	/// <param name='dTime'> Time elapsed since last Update() </param>
	private void UpdateBlocks(float dTime)
	{
		List<int> blocksToDelete = new List<int>();
		
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
						{
							ShiftBlockDown(block);
						}
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
						block.fallingOffset -= gFallSpeed * dTime;
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

						// Mark for deletion if there was a math
						if (delete)
						{
							blocksToDelete.Add(BlockIdx(col, row));
						}
					}
				}
			}
		}

		// Delete blocks & handle scoring
		int scoreChain = 0;
		Vector3 scorePopupPos = Vector3.zero;
		foreach (int blockIdx in blocksToDelete)
		{
			Block blockToDelete = gBlocks[blockIdx];

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
				GameObject drop = GameObject.Instantiate(fallingRingPrefab, blockTrans.position, blockTrans.rotation) as GameObject;
				drop.transform.localScale = new Vector3(blockScale * 0.275f, blockScale * 0.275f, blockScale * 0.275f);
				drop.GetComponent<Renderer>().material.color = new Color(blockColor.r * 0.5f, blockColor.g * 0.5f, blockColor.b * 0.5f);
				
				// Add to the size & colour of the player's progress bar
				if (gameMode != GameModes.ScoreChallenge)
				{
					++gPlayerBarAmount;
					gPlayerBarColorTotal += new Vector3(blockColor.r, blockColor.g, blockColor.b);
//					UpdateGameplayProgressBar(gProgressBarPlayerBar, (float)(gPlayerBarAmount) / (float)(gPlayerBarCapacity), gPlayerBarColorTotal / (float)(gPlayerBarAmount));
				}
			}

			// Add background pulse
			Environment.instance.flowerOfLife.StartPulse(blockColor);
		}
		if (blocksToDelete.Count > 0)
		{
			scorePopupPos /= blocksToDelete.Count;
		}
		
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
			scoreThisFrame += Convert.ToInt32(Convert.ToSingle(scoreThisFrame) * gScoreDifficultyMult);
			scoreThisFrame += Convert.ToInt32(level * 3.0f / Convert.ToSingle(levelMax - levelMin));
			scoreThisFrame *= 10;
			SetScore(gScore + scoreThisFrame);

//			TextGrowAndFade.StartPopupText(scorePopupPos, TowerCamera.Instance.transform.rotation, gGUITextLevel.color, scoreThisFrame.ToString() + ((scoreChain > 2) ? "!" : ""));
		}
	}
	

	/// <summary> Returns the number wrapped into the range [0..gColums) </summary>
	/// <param name='col'> Unwrapped column </param>
	/// <returns> Wrapped column from 0 to (gColumns - 1) </returns>
	private int WrapCol(int col)
	{
		while (col < 0) { col += columns; }
		while (col >= columns) { col -= columns; }
		return col;
	}
	
	
	/// <summary>Positions the selectors at the specified col (and col+1) and row. </summary>
	/// <param name='colLeft'> Column for left half of selector (right half will add 1) </param>
	/// <param name='row'> Row (for both halves) </param>
	private void SetSelectorPos(int colLeft, int row)
	{
		gSelectorLeftCol = colLeft;
		gSelectorRow = row;

		selectorLeft.transform.localEulerAngles = new Vector3(0.0f, Block.CalcAngleDeg(colLeft, columns), 0.0f);
		selectorLeft.transform.localPosition = Block.CalcPosition(WrapCol(colLeft), row, selectorLeft.transform.localEulerAngles.y, towerRadius, blockScale);

		selectorRight.transform.localEulerAngles = new Vector3(0.0f, Block.CalcAngleDeg(WrapCol(colLeft + 1), columns), 0.0f);
		selectorRight.transform.localPosition = Block.CalcPosition(WrapCol(colLeft + 1), row, selectorRight.transform.localEulerAngles.y, towerRadius, blockScale);
	}


	/// <summary> Updates the selector's swapping animation </summary>
	/// <param name='dTime'> Time elapsed since last Update() </param>
	private void UpdateSelectorSwapAnim(float dTime)
	{
		gSelectorSwapAnimOffset -= selectorSwapAnimSpeed * dTime;
		if (gSelectorSwapAnimOffset < 0.0f) { gSelectorSwapAnimOffset = 0.0f; }
	
		Vector3 leftPos = Block.CalcPosition(WrapCol(gSelectorLeftCol), gSelectorRow, selectorLeft.transform.localEulerAngles.y, towerRadius, blockScale);
		Vector3 rightPos = Block.CalcPosition(WrapCol(gSelectorLeftCol + 1), gSelectorRow, selectorRight.transform.localEulerAngles.y, towerRadius, blockScale);

		selectorRight.transform.localPosition = new Vector3(Mathf.Lerp(rightPos.x, leftPos.x, gSelectorSwapAnimOffset), rightPos.y, Mathf.Lerp(rightPos.z, leftPos.z, gSelectorSwapAnimOffset));
		selectorLeft.transform.localPosition = new Vector3(Mathf.Lerp(leftPos.x, rightPos.x, gSelectorSwapAnimOffset), leftPos.y, Mathf.Lerp(leftPos.z, rightPos.z, gSelectorSwapAnimOffset));
	}
}
