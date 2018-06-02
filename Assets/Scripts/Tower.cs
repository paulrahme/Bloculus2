using UnityEngine;
using System.Collections.Generic;
using System;

public partial class Tower : MonoBehaviour
{
	// Enums & constants
	public enum								GameModes { Original, Arcade, TimeChallenge, SpeedChallenge, ScoreChallenge };
	public enum								ControlMethods { TouchButtons, SwipeTower, SwipeSelector };
	private enum							LevelStates { None, Popup, JarCount, JarPause, FullJarCount, FinalPause };
	private enum							GameOverTypes { PlayerDied, TimeOrSpeedChallengeComplete, ScoreChallengeComplete };
	private static readonly Vector3			levelProgressBarVecGreen = new Vector3(0.0f, 0.8f, 0.0f);
	private static readonly Vector3			levelProgressBarVecRed = new Vector3(1.0f, 0.0f, 0.0f);
	private const string					jarBonusString ="Ring Bonus: ";
	private const string					jarFullString ="\nAll Rings! ";
	private const string					jarFullStringPlus ="\nAll Rings! +";
	private const string					jarFullTitleString ="ALL RINGS!";

	// Public variables
	public BlockDefinition[]				blockDefs;								// Data for block types
	public float							gTowerRadius = 4.0f;											// Radius of tower
	public int								gLevelMin = 1;													// Which level is considered lowest level
	public int								gLevelMax = 33;													// Which level is considered top level
	public float							gFallSpeedSlowest = 2.0f;										// Slowest falling speed in units/sec relative to block scale
	public float							gFallSpeedFastest = 7.0f;										// Fastest falling speed in units/sec relative to block scale
	public float							gLevelIncreaseRateFull = 0.00666f;								// How much the level increases per second (full length)
	public float							gLevelIncreaseRateShorter = 0.01666f;							// How much the level increases per second (shorter levels)
	public float							gLevelIncreaseRateArcade = 0.02333f;							// How much the level increases per second (arcade levels)
	public float							gNewBlockAppearRateSlowest = 3.5f;								// Slowest rate (in seconds) at which new blocks appear
	public float							gNewBlockAppearRateFastest = 2f;								// Fastest rate (in seconds) at which new blocks appear
	public int								gColumnsMin = 10;												// Number of columns considered the least (easiest)
	public int								gColumnsMax = 30;												// Number of columns considered the most (hardest)
	public int								gRowsMin = 5;													// Number of rows considered the least (hardest)
	public int								gRowsMax = 16;													// Number of rows considered the most (easiest)
	public int								gBlockTypesMin = 4;												// Number of block types considered the least (easiest)
	public int								gJarCapacityMin = 60;											// Drops to fill the jar on lowest level
	public int								gJarCapacityMax = 150;											// Drops to fill the jar on highest level
	public int								gLCBonusPerJarDrop = 100;										// Bonus for each drop in the jar when the level is complete
	public int								gLCBonusPerLevelPercent = 500;									// Bonus for filling the jar early, per remaining level percentage
	public float							gSelectorSwapAnimSpeed = 9.0f;									// Speed of the selector's swap anim, in loops per second
	public GameObject						gSelectorLeft;													// Half of the selector for swapping blocks
	public GameObject						gSelectorRight;													// Half of the selector for swapping blocks
	public GameObject						gReleaseNotesObject;											// Menu GUI for selecting starting level, speed etc
	public GameObject						gPopupWindowObject;												// Popup window for Game Over, Pause, Level Complete, etc
	public GameObject						gGameOverObject;												// Appears when the game's over
	public TextMesh							gGameOverObjectTitleTextMesh;									// Appears when the game's over
	public GameObject						gGameOverObjectHiScoreNameEntry;								// Sub-message saying you got a high score
	public GameObject						gGameOverObjectHiScoreShare;									// Sub-message saying to press button / tap screen to continue
	public GameObject						gPauseObject;													// Appears when the game is paused
	public GameObject						gPauseSaveAndExitButton;										// "Save and Exit" button in Pause menu
	public GameObject						gPauseExitNoSaveButton;											// "Quit Without Saving" button in Pause menu
	public GameObject						gPauseQuitButton;												// "Quit" button in Pause menu
	public GameObject						gLevelCompleteObject;											// Appears when the player completes a level
	public TextMesh							gLevelCompleteTitleTextMesh;									// Text showing "Level Complete" title
	public TextMesh							gLevelCompleteBonusTextMesh;									// Text showing bonus count up
	public GameObject						gLevelCompleteButtonsObject;									// Continue/quit buttons
	public TextGrowAndFade					gLevelUp3DTextScript;											// Appears when the level increases in continuous mode
	public GameObject						gInGameGUI;														// In-game GUI hierarchy
	public TextMesh							gGUITextLevel;													// 3D TextMesh on GUI
	public TextMesh							gGUITextHiScore, gGUITextHiScoreFrontend;						// 3D TextMeshes on GUI
	public TextMesh							gGUITextScore;													// 3D TextMesh on GUI
	public TextMesh							gGUITextTime;													// 3D TextMesh on GUI
	public TextMesh							gGUITextRings;													// 3D TextMesh on GUI
	public TextMesh							gGUIPauseButtonText;											// 3D TextMesh on TouchButton on GUI
	public GameObject						gProgressBarLevel;												// Progress bar that increases with the player's combos
	public Transform						gProgressBarLevelBar;											// Actual child bar transform which scales
	public GameObject						gProgressBarPlayer;												// Progress bar that increases with the player's combos
	public Transform						gProgressBarPlayerBar;											// Actual child bar transform which scales
	public GameObject						gScoreTextPrefab;												// Prefab from which to instantiate fading score popups
	public Transform						gDisabledGameObjectPool;										// Parent transform of disabled GameObjects in scene's hierarchy
	public GameObject						gFallingRingPrefab;												// Prefab from which to instantiate 3D falling rings
	public GameObject						gRippleRingPrefab;												// Prefab from which to instantiate pulsing ring effect
	public Texture							gLevelProgressBarTexture;										// GUI texture for level progress bar
	public Texture							gLevelProgressBarRedTexture;									// GUI texture for level progress bar
	public Texture							gLevelProgressBarBgTexture;										// GUI texture for level progress bar's background
	public GameObject						gAudioSourceXposZpos;											// Audio source for 1/4 of the blocks in the tower
	public GameObject						gAudioSourceXnegZpos;											// Audio source for 1/4 of the blocks in the tower
	public GameObject						gAudioSourceXposZneg;											// Audio source for 1/4 of the blocks in the tower
	public GameObject						gAudioSourceXnegZneg;											// Audio source for 1/4 of the blocks in the tower
	public GameObject						gBlockDisappearPrefab;											// Prefab from which to instantiate disappearing blocks
	public AudioClip[]						gBlockDisappearAudio;											// Audio for blocks disappearing (incl combos)
	public AudioClip[]						gSelectorMoveAudio;												// Audio for selector moving around
	public AudioClip[]						gSelectorSwitchAudio;											// Audio for selector switching
	public GameObject[]						gEnableOnlyForSwipeControls, gEnableOnlyForButtonControls;		// Objects to activate/deactivate depending on control method
	public AudioClip						gLevelEndedAudio;												// Audio for level ending (before player filled jar)
	public AudioClip						gLevelCompleteAudio;											// Audio for player filling the jar & ending the level
	public AudioClip						gLevelUpAudio;													// Audio for level automatically increasing
	public AudioClip						gAudioJarEmptying;												// Sound for when the jar is emptying during the end of a level
	public AudioClip						gAudioBarFilling;												// Sound for when the bar fills up during the end of a level
	public Color							gColor0Start, gColor0End, gColor0Text;							// Background + text colours for group of levels
	public Color							gColor1Start, gColor1End, gColor1Text;							// Background + text colours for group of levels
	public Color							gColor2Start, gColor2End, gColor2Text;							// Background + text colours for group of levels
	public Color							gColor3Start, gColor3End, gColor3Text;							// Background + text colours for group of levels
	public Color							gColor4, gColor4Text;											// Background + text colours for group of levels
	public TowerCamera						gTowerCameraScript;												// Camera script
	public BackgroundMusic					gBGMScript;														// Background Music script
	public FlowerOfLife						gFlowerOfLifeScript;											// Flower Of Life Script
	public Ripple							gRippleScript;													// Surface script to ripple when drop 'lands'
	public TouchGestures					gTouchGestureScript;											// Touch gesture/swipe recognition script

	// Public (non-editor) variables & properties
	[HideInInspector]
	public int								gColumns = 12;													// How many columns of blocks in 1 revolution
	[HideInInspector]
	public int								gRows = 10;														// How many rows of blocks going up the tower
	[HideInInspector]
	public int								gCurrentBlockTypes = 6;												// How many different coloured block types there are
	[HideInInspector]
	public float							gBlockScale;													// Scale of the blocks' transform
	[HideInInspector]
	public System.Random					gRandom = new System.Random();									// Random nunmber generator
	[HideInInspector]
	public float							gLevel;															// Current level
	[HideInInspector]
	public int								gLevelInt;														// Current level's integer value (not rounded up)
	[HideInInspector]
	public int								blockStyle;													// Appearance of blocks
	[HideInInspector]
	private Stack<Block>[]					gBlockPool;														// Used for recycling blocks, to avoid regular destroying & creating
	public float							gStartingLevel { get; private set; }							// Speed / rate to start at
	public GameModes						gGameMode { get; private set; }									// Current type of gameplay
	public ControlMethods					gControlMethod { get; private set; }							// Current control method (swipe, buttons, etc)
	public int								gHighScore { get; private set; }								// Record for the current starting level

	// Private variables
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
	private float							gProgressBarMaxScaleY;											// Scale of a full player/level progress bar

	// Helper/inline functions
	private int								BlockIdx(int col, int row) { return (row * gColumns) + col; }
	private Block							GetBlock(int col, int row) { return gBlocks[BlockIdx(col, row)]; }
	private bool							IsBlockAboutToShiftDown(Block block) { return ((block.row != 0) && (GetBlock(block.col, block.row - 1) == null)); }
	private float							GetSelectorAngle() { return Block.CalcAngleDeg(gSelectorLeftCol + (gSelectorLeftCol + 1), gColumns * 2); }
	private void							DeleteTemporaryObjects() { foreach (GameObject tempObject in GameObject.FindGameObjectsWithTag("TemporaryObject")) { GameObject.Destroy(tempObject); }	}
	private float							GetLevelPercent() { return ((gLevel - Convert.ToSingle(gLevelMin)) / Convert.ToSingle(gLevelMax - gLevelMin)); }
	private float							GetLevelPercentCapped() { return Mathf.Min(GetLevelPercent(), 1.0f); }
	public bool								IsGameFrozen() { return gPopupWindowObject.activeSelf; }
	public bool								IsGamePaused() { return (IsGameFrozen() && (gPopupWindowObject == gPauseObject)); }
	private bool							IsPlayerBarFull() { return (gPlayerBarAmount >= gPlayerBarCapacity); }
	private void							SetScore(int score) { gScore = score; gGUITextScore.text = "Score:\n"+gScore; }
	private bool							DoesGameModeSupportSaving() { return (gGameMode == GameModes.Original); }
	public bool								ShowReleaseNotes() { return (PlayerPrefs.GetInt(Constants.kPPShowReleaseNotes + CurrentBundleVersion.Version, 1) == 1); }
	public void								ShowReleaseNotesInFuture(bool show) { PlayerPrefs.SetInt(Constants.kPPShowReleaseNotes + CurrentBundleVersion.Version, show ? 1 : 0); }

	// Instance
    public static Tower						gInstance { get; private set; }

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
		Stack<Block> thisBlocksStack = gBlockPool[blockID];
		if (thisBlocksStack.Count > 0)
		{
			// Recycle block form the pool
			Block block = thisBlocksStack.Pop();
			block.Setup(blockID, transform, gColumns, gTowerRadius, gBlockScale, col, row);
			return block;
		}
		else
		{
			// Create new block from prefab
			BlockDefinition blockDef = blockDefs[blockID];
			GameObject prefab = (blockStyle == 0) ? blockDef.prefabSolid : blockDef.prefabWithInnerShape;
			GameObject gameObj = Instantiate(prefab) as GameObject;
			Block block = new Block(gameObj, blockDefs[blockID]);
			block.Setup(blockID, transform, gColumns, gTowerRadius, gBlockScale, col, row);
			return block;
		}
	}
	

	/// <summary> Adds a block to the stack for recycling </summary>
	/// <param name="block"> Block to add </param>
	public void RecycleBlock(Block block)
	{
		gBlockPool[block.blockID].Push(block);
		block.gameObj.transform.parent = gDisabledGameObjectPool;
	}

	public void EmptyRecyclePool()
	{
		foreach (Stack<Block> blockStack in gBlockPool)
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
		gInstance = this;
		blockStyle = PlayerPrefs.GetInt(Constants.kPPBlockStyle, 1);
		gProgressBarMaxScaleY = gProgressBarPlayerBar.localScale.y;

		PrepareObjectPools();
	}


	/// <summary> Creates & prepares the stacks for recycling objects </summary>
	private void PrepareObjectPools()
	{
		int blockPoolSize = blockDefs.Length;
		gBlockPool = new Stack<Block>[blockPoolSize];
		for (int i = 0; i < blockPoolSize; ++i)
		{
			gBlockPool[i] = new Stack<Block>();
		}
	}
	

	/// <summary> Called before the first Update() </summary>
	void Start()
	{
		SetGameMode(GameModes.Original);
		ShowFrontendMenu();
	}
	

	/// <summary> Sets a new game mode & performs any appropriate actions </summary>
	/// <param name='gameMode'> eGameMode.... name </param>
	public void SetGameMode(GameModes gameMode)
	{
		gGameMode = gameMode;
		switch (gGameMode)
		{
			case GameModes.Original:
				SetStartingLevel(PlayerPrefs.GetInt(Constants.kPPStartingLevel, 1));
				gLevelIncreaseRate = gLevelIncreaseRateFull;
				break;
		
			case GameModes.Arcade:
				SetStartingLevel(1);
				gLevelIncreaseRate = gLevelIncreaseRateArcade;
				break;
		
			case GameModes.TimeChallenge:
			case GameModes.SpeedChallenge:
				SetStartingLevel(PlayerPrefs.GetInt(Constants.kPPStartingLevel, 1));
				gLevelIncreaseRate = 0;
				break;
		
			case GameModes.ScoreChallenge:
				SetStartingLevel(PlayerPrefs.GetInt(Constants.kPPStartingLevel, 1));
				gLevelIncreaseRate = gLevelIncreaseRateShorter;
				break;
		
			default:
				throw new Exception("Unhandled GameMode '"+gGameMode+"'");
		}

		// Prepare pause menu to only allow saving in Original mode
		gPauseSaveAndExitButton.SetActive(gGameMode == GameModes.Original);
		gPauseExitNoSaveButton.SetActive(gGameMode == GameModes.Original);
		gPauseQuitButton.SetActive(gGameMode != GameModes.Original);
	}
	
	
	/// <summary> Sets a new game mode & performs any appropriate actions </summary>
	/// <param name='gameMode'> eGameMode.... name </param>
	public void SetControlMethod(ControlMethods controlMethod)
	{
		gControlMethod = controlMethod;

		SetButtonControlsEnabled(gControlMethod == ControlMethods.TouchButtons);
		SetSwipeControlsEnabled(gControlMethod != ControlMethods.TouchButtons);

		PlayerPrefs.SetInt(Constants.kPPControlMethod, (int)gControlMethod);
		PlayerPrefs.Save();
	}


	/// <summary> Turns on/off all Button control GameObjects </summary>
	/// <param name="enable"> True to enable, false to disable </param>
	private void SetButtonControlsEnabled(bool enable)
	{
		for (int i = 0; i < gEnableOnlyForButtonControls.Length; ++i)
		{
			gEnableOnlyForButtonControls[i].SetActive(enable);
		}
	}
	
	
	/// <summary> Turns on/off all Button control GameObjects </summary>
	/// <param name="enable"> True to enable, false to disable </param>
	private void SetSwipeControlsEnabled(bool enable)
	{
		for (int i = 0; i < gEnableOnlyForSwipeControls.Length; ++i)
		{
			gEnableOnlyForSwipeControls[i].SetActive(enable);
		}
	}
	
	
	/// <summary> Sets the starting level and adjusts game speeds accordingly </summary>
	/// <param name='level'> New level to start game on </param>
	public void SetStartingLevel(int level)
	{
		gStartingLevel = level;
		gHighScore = PlayerPrefs.GetInt(gGameMode+Constants.kPPHiScore+level, 0);
		gHighScoreName = PlayerPrefs.GetString(gGameMode+Constants.kPPHiScoreName+level, string.Empty);
		RefreshHiScoreGUIString();
		RestoreSpeeds();
		UpdateBackground(false);
	}

	/// <summary> Updates the background colour, texture etc. </summary>
	/// <param name="changeMusic"> When true, change background music as necessary </param>
	private void UpdateBackground(bool changeMusic)
	{
		if (gLevel < 8.0f)
		{
			float colorPercent = gLevel / 8.0f;
			gTowerCameraScript.SetBackgroundColor(Color.Lerp(gColor0Start, gColor0End, colorPercent));
			SetInGameUIFontColor(gColor0Text);
			gRippleScript.SetMaterial(0);
			if (changeMusic) { gBGMScript.StartGameMusic(0); }
		}
		else if (gLevel < 16.0f)
		{
			float colorPercent = (gLevel - 8.0f) / 8.0f;
			gTowerCameraScript.SetBackgroundColor(Color.Lerp(gColor1Start, gColor1End, colorPercent));
			SetInGameUIFontColor(gColor0Text);
			gRippleScript.SetMaterial(1);
			if (changeMusic) { gBGMScript.StartGameMusic(1); }
		}
		else if (gLevel < 24.0f)
		{
			float colorPercent = (gLevel - 16.0f) / 8.0f;
			gTowerCameraScript.SetBackgroundColor(Color.Lerp(gColor2Start, gColor2End, colorPercent));
			SetInGameUIFontColor(gColor0Text);
			gRippleScript.SetMaterial(2);
			if (changeMusic) { gBGMScript.StartGameMusic(2); }
		}
		else if (gLevel < (float)gLevelMax)
		{
			float colorPercent = (gLevel - 24.0f) / ((float)(gLevelMax) - 24.0f);
			gTowerCameraScript.SetBackgroundColor(Color.Lerp(gColor3Start, gColor3End, colorPercent));
			SetInGameUIFontColor(gColor3Text);
			gRippleScript.SetMaterial(3);
			if (changeMusic) { gBGMScript.StartGameMusic(3); }
		}
		else
		{
			gTowerCameraScript.SetBackgroundColor(gColor4);
			SetInGameUIFontColor(gColor4Text);
			gRippleScript.SetMaterial(4);
			if (changeMusic) { gBGMScript.StartGameMusic(4); }
		}
	}


	/// <summary> Sets the colour of the in-game UI (score, level etc) </summary>
	/// <param name="color"> Colour to use </param>
	private void SetInGameUIFontColor(Color color)
	{
		gGUITextLevel.color = gGUITextHiScore.color = gGUITextScore.color = gGUITextTime.color = gGUITextRings.color = gGUIPauseButtonText.color = gLevelUp3DTextScript.gOpaqueColor = color;
	}
	

	/// <summary> Creates the high score + name (if set) for displaying on the GUI </summary>
	void RefreshHiScoreGUIString()
	{
		// Score
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
	}
	
	
	/// <summary> Saves the high score + record holder's name </summary>
	/// <param name="name"> Record holder's name </param>
	public void SaveHiScoreEntry(string name)
	{
		gHighScoreName = ((name == string.Empty) ? "Anonymous" : name);
		gHighScore = gScore;
		PlayerPrefs.SetInt(gGameMode+Constants.kPPHiScore+Convert.ToInt32(gStartingLevel), gScore);
		PlayerPrefs.SetString(gGameMode+Constants.kPPHiScoreName+Convert.ToInt32(gStartingLevel), gHighScoreName);
		PlayerPrefs.Save();
		gGameOverObjectHiScoreNameEntry.SetActive(false);
		gGameOverObjectHiScoreShare.SetActive(true);
		RefreshHiScoreGUIString();
	}
	
	
	/// <summary> Resets the current speed values to their starting values </summary>
	void RestoreSpeeds()
	{
		gLevel = gStartingLevel;
		gLevelInt = Mathf.FloorToInt(gStartingLevel);
		gGUITextLevel.text = "Level\n"+gLevelInt;
		LevelChanged(GetLevelPercent());
		gNewBlockTimer = gNewBlockAppearRate;
	}
	
	
	/// <summary> Prepares the tower from the current settings </summary>
	/// <param name='createNewBlocks'> When true, creates a new bunch of random blocks </param>
	public void RefreshTower(bool createNewBlocks)
	{
		// Calculate scale for block transforms
		gBlockScale = gTowerRadius * 6.0f / gColumns;
		
		// Start rotated half a block left, so there's always a pair of blocks centered on the screen
		if (createNewBlocks)
		{
			TowerCamera.Instance.ResetRotation();
		}

		// Set up starting blocks
		gBlocks = new Block[gColumns * (gRows + 1)];	// 1 extra row for block generators
		if (createNewBlocks)
		{
			CreateRandomBlocks();
		}
		TowerCamera.Instance.RefreshPosition();
		gNewBlockTimer = gNewBlockAppearRate;
		
		// Initialise selector boxes
		if (createNewBlocks)
		{
			SetSelectorPos(gColumns - 1, 2);
		}
		gSelectorLeft.transform.localScale = gSelectorRight.transform.localScale = new Vector3(gBlockScale, gBlockScale, gBlockScale);
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

		for (int row = 0; row <= gRows; ++row)	// Note rows + 1, for new block generators
		{
			for (int col = 0; col < gColumns; ++col)
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
		for (int row = 0; row < gRows; ++row)
		{
			for (int col = 0; col < gColumns; ++col)
			{
				// Create a block randomly
				if ((gRandom.Next() & 4) != 0)
				{
					int blockIdx = gRandom.Next() % gCurrentBlockTypes;
					gBlocks[BlockIdx(col, row)] = GetNewBlock(blockIdx, transform, gColumns, gTowerRadius, gBlockScale, col, row);
					GetBlock(col, row).fallingOffset = (float)gRandom.NextDouble();
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
		int rowNo = gRows - 1;
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
		gGUITextLevel.gameObject.SetActive(true);
		gGUITextHiScore.gameObject.SetActive(true);
		switch (gGameMode)
		{
			case GameModes.Original:
			case GameModes.Arcade:
				gGUITextScore.gameObject.SetActive(true);
				gGUITextTime.gameObject.SetActive(false);
				gProgressBarLevel.SetActive(true);
				gProgressBarPlayer.SetActive(true);
				break;
				
			case GameModes.TimeChallenge:
			case GameModes.SpeedChallenge:
				gGUITextScore.gameObject.SetActive(false);
				gGUITextTime.gameObject.SetActive(true);
				gProgressBarLevel.SetActive(false);
				gProgressBarPlayer.SetActive(true);
				gTimeChallengeStartTime = Time.fixedTime;
				break;
				
			case GameModes.ScoreChallenge:
				gGUITextScore.gameObject.SetActive(true);
				gGUITextTime.gameObject.SetActive(false);
				gProgressBarLevel.SetActive(true);
				gProgressBarPlayer.SetActive(false);
				gTimeChallengeStartTime = Time.fixedTime;
				break;

			default:
				throw new Exception("Unhandled GameMode "+gGameMode);
		}
		UpdateGameplayProgressBar(gProgressBarPlayerBar, 0.0f, Vector3.zero);
		UpdateGameplayProgressBar(gProgressBarLevelBar, 0.0f, Vector3.zero);
		UpdateBackground(true);
		gInGameGUI.SetActive(true);
	}
	
	
	/// <summary> Pops up the window displaying the specified contents </summary>
	/// <param name='menuGameObj'> GameObject which should be visible, all others will be turned off </param>
	private void PopupWindow(GameObject menuGameObj)
	{
		// Set the required menu active
		gGameOverObject.SetActive(menuGameObj == gGameOverObject);
		gPauseObject.SetActive(menuGameObj == gPauseObject);
		gLevelCompleteObject.SetActive(menuGameObj == gLevelCompleteObject);

		// Disable touch buttons
		SetButtonControlsEnabled(false);

		// Popup
		gPopupWindowObject.SetActive(true);
	}
	
	
	/// <summary> Called once per frame </summary>
	void Update()
	{
		float dTime = Time.deltaTime;
		
		UpdateKeyboard();
		UpdateJoystick();
		if (gControlMethod != ControlMethods.TouchButtons) { UpdateSwipes(); }
		UpdateSelectorSwapAnim(dTime);
		UpdateNewBlocks(dTime);
		UpdateBlocks(dTime);
		UpdateLevelProgress(dTime);
		gRippleScript.UpdateEffect();

		// Filled bar?
		if (IsPlayerBarFull())
		{
			// Ensure player bar has not overflowed
			gPlayerBarAmount = gPlayerBarCapacity;
			UpdateGameplayProgressBar(gProgressBarPlayerBar, 1.0f, gPlayerBarColorTotal / (float)(gPlayerBarAmount));

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


	/// <summary> Quicksaves the game and exits to the frontend menu </summary>
	public void SaveAndExit()
	{
		QuickSave();
		ShowFrontendMenu();
	}


	/// <summary> Pauses the game & pops up the pause menu </summary>
	public void PauseGame()
	{
		PauseDropsAndShockwaves(true);
		gBGMScript.PauseGameMusic();
		PopupWindow(gPauseObject);
	}
	
	
	/// <summary> Exit to the frontend menu without saving the game </summary>
	public void ExitNoSave()
	{
		ShowFrontendMenu();
	}


	/// <summary> Update when 'Game Over' is active </summary>
	void UpdateGameOver()
	{
		if (gGameOverObjectHiScoreShare.activeSelf)
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
			if (DoesGameModeSupportSaving()) { QuickSave(); }
			ShowFrontendMenu();
		}
	}

	
	/// <summary> Closes the popup window & continues gameplay </summary>
	public void UnpauseGame()
	{
		PauseDropsAndShockwaves(false);
		gBGMScript.UnpauseGameMusic();
	}
	
	
	/// <summary> Resets the score & the jar </summary>
	void ResetScore()
	{
		SetScore(0);
		ResetPlayerBar();
	}
	
	
	/// <summary> Starts the Game Over sequence </summary>
	/// <param name='gameOverType'> eGameOverTypes.... value </param>
	void GameOver(GameOverTypes gameOverType)
	{
		bool savePlayerPrefs = false;

		switch (gameOverType)
		{
			case GameOverTypes.PlayerDied:
				gGameOverObjectTitleTextMesh.text = "GAME\nOVER";
				gBGMScript.StartGameOverMusic();
				if (DoesGameModeSupportSaving()) { PlayerPrefs.SetInt(Constants.kQSExists, 0); }
				savePlayerPrefs = true;
				break;
			
			case GameOverTypes.TimeOrSpeedChallengeComplete:
				gBGMScript.StopGameMusic();
				GetComponent<AudioSource>().PlayOneShot(gLevelCompleteAudio);
				gGameOverObjectTitleTextMesh.text = "GAME\nCOMPLETED!";
				break;

			case GameOverTypes.ScoreChallengeComplete:
				gBGMScript.StopGameMusic();
				GetComponent<AudioSource>().PlayOneShot(gLevelEndedAudio);
				gGameOverObjectTitleTextMesh.text = "GAME\nCOMPLETED!";
				break;
				
			default:
				throw new Exception("Unhandled GameOverType '"+gameOverType+"'");
		}

		PopupWindow(gGameOverObject);
		
		// Check if highest level reached has increased
		if (gLevelInt > PlayerPrefs.GetInt(Constants.kPPHighestLevel))
		{
			PlayerPrefs.SetInt(Constants.kPPHighestLevel, gLevelInt);
			savePlayerPrefs = true;
		}

		// Save PlayerPrefs if anything's changed
		if (savePlayerPrefs)
			PlayerPrefs.Save();

		// Check if high score has been broken
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
	}
	
	
	/// <summary> Resets the player's progress bar </summary>
	private void ResetPlayerBar()
	{
		gPlayerBarAmount = 0;
		gPlayerBarColorTotal = Vector3.zero;
		UpdateGameplayProgressBar(gProgressBarPlayerBar, 0.0f, Vector3.zero);
	}
	

	/// <summary> Updates the size & colour of the on-screen progress bar </summary>
	/// <param name='progressBarTransform'> Transform of the progress bar to update </param>
	/// <param name='percent'> Value from 0.0f to 1.0f representing progress </param>
	/// <param name='colorRGB'> Colour to set the renderer's material </param>
	private void UpdateGameplayProgressBar(Transform progressBarTransform, float percent, Vector3 colorRGB)
	{
		float scaleY = percent * gProgressBarMaxScaleY;
		progressBarTransform.localPosition = new Vector3(progressBarTransform.localPosition.x, scaleY / 2.0f, progressBarTransform.localPosition.z);
		progressBarTransform.localScale = new Vector3(progressBarTransform.localScale.x, scaleY, progressBarTransform.localScale.z);
		progressBarTransform.GetComponent<Renderer>().material.color = new Color(colorRGB.x, colorRGB.y, colorRGB.z, progressBarTransform.GetComponent<Renderer>().material.color.a);
	}
	

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
		gLCLevelJustCompleted = gLevelInt;
		gLevelCompleteBonusTextMesh.text = string.Empty;
		gLCJarFull = IsPlayerBarFull();
		switch (gGameMode)
		{
			case GameModes.Original:
				gBGMScript.PauseGameMusic();
				GetComponent<AudioSource>().PlayOneShot(gLCJarFull ? gLevelCompleteAudio : gLevelEndedAudio);
				LevelCompleteSetState(LevelStates.Popup);
				PopupWindow(gLevelCompleteObject);
				gLevelCompleteButtonsObject.SetActive(false);
				gLevelCompleteTitleTextMesh.text = "LEVEL "+gLCLevelJustCompleted+ (gLCJarFull ? "\nCOMPLETED!" : "\nHAS ENDED.");
				break;
			
			case GameModes.Arcade:
				if (gLCJarFull)
				{
					gBGMScript.PauseGameMusic();
					LevelCompleteSetState(LevelStates.JarCount);
					PopupWindow(gLevelCompleteObject);
					gLevelCompleteButtonsObject.SetActive(false);
					gLevelCompleteTitleTextMesh.text = jarFullTitleString;
				}
				else
				{
					QuickLevelUp();
				}
				break;
			
			case GameModes.TimeChallenge:
			case GameModes.SpeedChallenge:
				GetComponent<AudioSource>().PlayOneShot(gLevelCompleteAudio);
				gLevel = gLevelInt + 1.0f;
				if ((gGameMode == GameModes.SpeedChallenge) || (Mathf.FloorToInt(gLevel) >= gStartingLevel + 5))
				{
					SetScore(Mathf.FloorToInt((Time.fixedTime - gTimeChallengeStartTime) * 1000.0f));	// *1000 to preserve milliseconds
					GameOver(GameOverTypes.TimeOrSpeedChallengeComplete);
				}
				else
				{
					gBGMScript.PauseGameMusic(); gBGMScript.UnpauseGameMusic();	// Instantly mutes, then starts fading back in
					QuickLevelUp(false);
					ResetPlayerBar();
				}
				break;
			
			case GameModes.ScoreChallenge:
				if (Mathf.FloorToInt(gLevel) >= gStartingLevel + 5)
				{
					GameOver(GameOverTypes.ScoreChallengeComplete);
				}
				else
				{
					QuickLevelUp();
				}
				break;
		}
		gLevelCompleteBonusTextMesh.text = string.Empty;
	}
	
	
	/// <summary> Triggers the "Level up" fading text </summary>
	/// <param name='playSound'> True to play the "level up" sound </param>
	private void QuickLevelUp(bool playSound = true)
	{
		if (playSound) { GetComponent<AudioSource>().PlayOneShot(gLevelUpAudio); }
		gLevelUp3DTextScript.ResetAnim();
		gLevelUp3DTextScript.gameObject.SetActive(true);
		UpdateBackground(true);
	}
	
	
	/// <summary> Updates the "level complete/ended" popup sequence between levels </summary>
	private void UpdateLevelComplete()
	{
		// Quit?
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (DoesGameModeSupportSaving()) { QuickSave(); }
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
					gLevelCompleteBonusTextMesh.text = jarBonusString;
					if (gLCJarFull) { gLevelCompleteBonusTextMesh.text += "\n"; }
				}
				break;

			case LevelStates.JarCount:
				if (gPlayerBarAmount > 0)
				{
					if (fixedTime > gRepeatingSoundTimer)
					{
						GetComponent<AudioSource>().PlayOneShot(gAudioJarEmptying);
						gRepeatingSoundTimer = fixedTime + 0.2f;
					}
					if (timeOffset > 0.03f)
					{
						gLCStateTime = Time.fixedTime;
						--gPlayerBarAmount;
						UpdateGameplayProgressBar(gProgressBarPlayerBar, (float)(gPlayerBarAmount) / (float)(gPlayerBarCapacity), gPlayerBarColorTotal / (float)(gPlayerBarAmount));
						gLCDropBonus += gLCBonusPerJarDrop;
						SetScore(gScore + gLCBonusPerJarDrop);
					}
				}
				else
				{
					if (gGameMode == GameModes.Arcade)
					{
						LevelCompleteSetState(LevelStates.FinalPause);
					}
					else
					{	
						LevelCompleteSetState(LevelStates.JarPause);
					}
				}
				// Bonus string, with new line if jar full
				gLevelCompleteBonusTextMesh.text = jarBonusString+gLCDropBonus;
				if (gLCJarFull) { gLevelCompleteBonusTextMesh.text += "\n"; }
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
						gLevelCompleteButtonsObject.SetActive(true);
						LevelCompleteSetState(LevelStates.FinalPause);
					}
				}
				if ((gLCJarFull) && (timeOffset >= 0.5f) && (gLCPrevTimeOffset < 0.5f))
				{
					GetComponent<AudioSource>().PlayOneShot(gLevelUpAudio);
					gLevelCompleteBonusTextMesh.text = jarBonusString+gLCDropBonus+jarFullString;
				}
				break;
			
			case LevelStates.FullJarCount:
				if (Mathf.FloorToInt(gLevel) == gLCLevelJustCompleted)
				{
					if (fixedTime > gRepeatingSoundTimer)
					{
						GetComponent<AudioSource>().PlayOneShot(gAudioBarFilling);
						gRepeatingSoundTimer = fixedTime + 0.15f;
					}
					gLCFullJarBonus += gLCBonusPerLevelPercent / 4;
					SetScore(gScore + (gLCBonusPerLevelPercent / 4));
					UpdateLevelProgress(0.25f);
				}
				else
				{
					gLevelCompleteButtonsObject.SetActive(true);
					LevelCompleteSetState(LevelStates.FinalPause);
				}
				gLevelCompleteBonusTextMesh.text = jarBonusString+gLCDropBonus+jarFullStringPlus+gLCFullJarBonus;
				break;

			case LevelStates.FinalPause:
				if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || (gGameMode == GameModes.Arcade))
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
		gLevel += gLevelIncreaseRate * dTime;
		
		// Has level just changed?
		if (Mathf.FloorToInt(gLevel) != gLevelInt)
		{
			// Update speeds & tower layout for next level
			float levelPercent = GetLevelPercentCapped();
			LevelChanged(levelPercent);
			
			// If it's in gameplay, trigger the "level complete" sequence
			// if (!gFrontendMenuObject.activeSelf && !IsGameFrozen())
			{
				LevelComplete();
			}

			gLevelInt = Mathf.FloorToInt(gLevel);
			gGUITextLevel.text = "Level\n"+gLevelInt;
		}
		
		// GameMode specific updates
		switch (gGameMode)
		{
			case GameModes.TimeChallenge:
			case GameModes.SpeedChallenge:
				TimeSpan timeSpan = TimeSpan.FromSeconds(Time.fixedTime - gTimeChallengeStartTime);
				gGUITextTime.text = string.Format("Time:\n{0:00}:{1:00}.{2:000}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
				break;
			
			default:
				float levelPercent = gLevel - (float)(gLevelInt);
				if ((levelPercent < 0.9f) || IsGameFrozen() || ((Mathf.FloorToInt(Time.fixedTime * 3.0f) & 1) == 0))	// Flash when almost full
				{
					UpdateGameplayProgressBar(gProgressBarLevelBar, 1.0f - levelPercent, (levelPercent > 0.8f) ? levelProgressBarVecRed : levelProgressBarVecGreen);
					gProgressBarLevelBar.GetComponent<Renderer>().enabled = true;
				}
				else
				{
					gProgressBarLevelBar.GetComponent<Renderer>().enabled = false;
				}
				break;
		}
	}
	

	/// <summary> Sets the speeds & tower layout </summary>
	/// <param name='percent'> Speed scale: 0.0f = slowest, 1.0f = fastest </param>
	void LevelChanged(float percent)
	{
		// Update speeds
		gFallSpeed = gFallSpeedSlowest + ((gFallSpeedFastest - gFallSpeedSlowest) * percent);
		gNewBlockAppearRate = gNewBlockAppearRateSlowest + ((gNewBlockAppearRateFastest - gNewBlockAppearRateSlowest) * percent);

		// Update block types & jar capacity
		gCurrentBlockTypes = gBlockTypesMin + Convert.ToInt32(Convert.ToSingle(blockDefs.Length - gBlockTypesMin) * percent);
		gPlayerBarCapacity = gJarCapacityMin + Convert.ToInt32(Convert.ToSingle(gJarCapacityMax - gJarCapacityMin) * percent);

		// Calculate columns & rows for new level
		int newColumns = gColumnsMin + Convert.ToInt32(Convert.ToSingle(gColumnsMax - gColumnsMin) * percent);
		int newRows = gRowsMin + Convert.ToInt32(Convert.ToSingle(gRowsMax - gRowsMin) * percent);

		// Update background effects
		gFlowerOfLifeScript.SetMaxActiveMaterials(Mathf.FloorToInt(gLevel));
		gRippleScript.gScrollSpeed = gRippleScript.gScrollSpeedSlowest - ((gRippleScript.gScrollSpeedSlowest - gRippleScript.gScrollSpeedFastest) * percent);

		// Special case for final texture - scrolls away from the player & slowly
		if (percent > 0.99f)
		{
			gRippleScript.gScrollSpeed.y *= 0.5f;
		}

		// Menu selection: always recreate tower with new set of blocks
/*		if (gFrontendMenuObject.activeSelf)
		{
			ClearBlocks(true);
			gColumns = newColumns;
			gRows = newRows;
			RefreshTower(true);
		}
		// Regular gameplay - if the columns or rows have changed, update the tower preserving the previous blocks
		else
*/		{
			// Columns/rows changed?
			if((gColumns != newColumns) || (gRows != newRows))
			{
				// Backup blocks
				List<BlockInfo> oldBlocks = BackupBlocks();
				
				// Create (bigger) tower
				ClearBlocks(false);
				gColumns = newColumns;
				gRows = newRows;
				RefreshTower(false);
	
				// Restore old blocks
				RestoreBlocks(oldBlocks);
	
				// Safely release all info structures now
				oldBlocks.Clear();
			}
		}

		// Update score multiplier
		gScoreDifficultyMult = percent;
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
			gBlocks[BlockIdx(info.mCol, info.mRow)] = GetNewBlock(info.mBlockID, transform, gColumns, gTowerRadius, gBlockScale, info.mCol, info.mRow);
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
		TowerCamera.Instance.SetState(TowerCamera.GameStates.Menu);
		
		// Restore starting speeds (will also recreate tower by calling LevelChanged())
		RestoreSpeeds();
	}


	/// <summary> Moves the selector left </summary>
	public void MoveLeft()
	{
		gSelectorLeft.GetComponent<AudioSource>().PlayOneShot(gSelectorMoveAudio[0]);
		SetSelectorPos(WrapCol(gSelectorLeftCol + 1), gSelectorRow);
		TowerCamera.Instance.RotateTowards(GetSelectorAngle());
		gSelectorSwapAnimOffset = 0.0f;
	}

	
	/// <summary> Moves the selector right </summary>
	public void MoveRight()
	{
		gSelectorLeft.GetComponent<AudioSource>().PlayOneShot(gSelectorMoveAudio[1]);
		SetSelectorPos(WrapCol(gSelectorLeftCol - 1), gSelectorRow);
		TowerCamera.Instance.RotateTowards(GetSelectorAngle());
		gSelectorSwapAnimOffset = 0.0f;
	}

	
	/// <summary> Moves the selector up </summary>
	public void MoveUp()
	{
		gSelectorLeft.GetComponent<AudioSource>().PlayOneShot(gSelectorMoveAudio[2]);
		SetSelectorPos(gSelectorLeftCol, Mathf.Min(gRows - 1, gSelectorRow + 1));
		gSelectorSwapAnimOffset = 0.0f;
	}

	
	/// <summary> Moves the selector down </summary>
	public void MoveDown()
	{
		gSelectorLeft.GetComponent<AudioSource>().PlayOneShot(gSelectorMoveAudio[3]);
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
			gSelectorLeft.GetComponent<AudioSource>().PlayOneShot(gSelectorSwitchAudio[(gSelectorLeftCol & 1)]);

			// Swap the blocks
			gBlocks[(gSelectorRow * gColumns) + gSelectorLeftCol] = oldRight;
			gBlocks[(gSelectorRow * gColumns) + rightCol] = oldLeft;
			
			// Set the new columns, positions & rotations
			if (oldLeft != null)
			{
				oldLeft.col = rightCol;
				oldLeft.gameObj.transform.localEulerAngles = new Vector3(0.0f, Block.CalcAngleDeg(oldLeft.col, gColumns), 0.0f);
				oldLeft.gameObj.transform.localPosition = Block.CalcPosition(oldLeft.col, oldLeft.row, oldLeft.gameObj.transform.localEulerAngles.y, gTowerRadius, gBlockScale);
			}
			if (oldRight != null)
			{
				oldRight.col = gSelectorLeftCol;
				oldRight.gameObj.transform.localEulerAngles = new Vector3(0.0f, Block.CalcAngleDeg(oldRight.col, gColumns), 0.0f);
				oldRight.gameObj.transform.localPosition = Block.CalcPosition(oldRight.col, oldRight.row, oldRight.gameObj.transform.localEulerAngles.y, gTowerRadius, gBlockScale);
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
	
	
	/// <summary> Updates directional swiping on a touch screen </summary>
	private void UpdateSwipes()
	{
		switch (gTouchGestureScript.UpdateGestures())
		{
			case TouchGestures.eGestureTypes.None:
				break;
			
			case TouchGestures.eGestureTypes.SwipeUp:
				MoveUp();
				break;
			
			case TouchGestures.eGestureTypes.SwipeLeft:
				if (gControlMethod == ControlMethods.SwipeTower) { MoveRight(); } else { MoveLeft(); }
				break;
			
			case TouchGestures.eGestureTypes.SwipeRight:
				if (gControlMethod == ControlMethods.SwipeTower) { MoveLeft(); } else { MoveRight(); }
				break;
			
			case TouchGestures.eGestureTypes.SwipeDown:
				MoveDown();
				break;
			
			case TouchGestures.eGestureTypes.Switch:
				SwitchBlocks();
				break;
			
			default:
				throw new Exception("Unhandled touch gesture type");
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
			for (int col = 0; col < gColumns; ++col)
			{
				Block block = GetBlock(col, gRows);
				if (block != null)
				{
					block.gameObj.transform.localScale = new Vector3(gBlockScale, gBlockScale, gBlockScale);
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
						GameOver(GameOverTypes.PlayerDied);
					}
				}
			}

			if (!IsGameFrozen())
			{
				// Reset timer
				gNewBlockTimer = gNewBlockAppearRate;
				
				// Create next batch of blocks
				int firstBlockIdxToCreate = gRandom.Next() % gColumns;
				int numBlocksToCreate = gRandom.Next() % gColumns;
				int prevBlockIdx = -1;
				for (int col = firstBlockIdxToCreate; col < firstBlockIdxToCreate + numBlocksToCreate; ++col)
				{
					int wrappedCol = WrapCol(col);
					int blockIdx = gRandom.Next() % gCurrentBlockTypes;

					// Avoid matching the block it's falling onto
					Block topMostBlock = FindTopmostBlock(wrappedCol);
					if ((topMostBlock != null) && (blockIdx == topMostBlock.blockID))
					{
						blockIdx = (blockIdx + 1) % gCurrentBlockTypes;
						// Debug.Log("Changed col "+wrappedCol+"'s blockID from "+gBlockPrefabsSolid[topMostBlock.mBlockID]+" to "+gBlockPrefabsSolid[blockIdx]+" (matched topmost block)");
					}
					// Avoid 2 matching blocks next to each other
					else if (blockIdx == prevBlockIdx)
					{
						blockIdx = (blockIdx + 1) % gCurrentBlockTypes;
						// Debug.Log("Changed col "+wrappedCol+"'s blockID from "+gBlockPrefabsSolid[prevBlockIdx]+" to "+gBlockPrefabsSolid[blockIdx]+" (matched block next to it)");
					}

					// Create block
					gBlocks[BlockIdx(wrappedCol, gRows)] = GetNewBlock(blockIdx, transform, gColumns, gTowerRadius, gBlockScale, wrappedCol, gRows);
					
					prevBlockIdx = blockIdx;
				}
			}
		}
		
		// Update growing
		float growScale = 1.0f - (gNewBlockTimer / gNewBlockAppearRate);
		for (int col = 0; col < gColumns; ++col)
		{
			Block block = GetBlock(col, gRows);
			if (block != null)
			{
				block.gameObj.transform.localScale = new Vector3(gBlockScale, gBlockScale * growScale, gBlockScale);
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
			audioSourceToUse = (localPosition.z > 0) ? gAudioSourceXposZpos.GetComponent<AudioSource>() : gAudioSourceXposZneg.GetComponent<AudioSource>();
		}
		else
		{
			audioSourceToUse = (localPosition.z > 0) ? gAudioSourceXnegZpos.GetComponent<AudioSource>() : gAudioSourceXnegZneg.GetComponent<AudioSource>();
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
		for (int row = 0; row < gRows; ++row)
		{
			for (int col = 0; col < gColumns; ++col)
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
							block.gameObj.transform.localPosition = new Vector3(block.gameObj.transform.localPosition.x, (block.row * gBlockScale), block.gameObj.transform.localPosition.z);
						}
					}
					
					// Check again in case it just landed / shifted
					if (block.fallingOffset > 0.0f)
					{
						// Falling normally
						block.fallingOffset -= gFallSpeed * dTime;
						Vector3 newPos = block.gameObj.transform.localPosition;
						newPos.y = (block.row * gBlockScale) + (block.fallingOffset * gBlockScale);
						block.gameObj.transform.localPosition = newPos;
					}
				}
			}
		}
		
		// Check for matches
		for (int row = 0; row < gRows; ++row)
		{
			for (int col = 0; col < gColumns; ++col)
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
						if (!delete && (row < gRows - 1))
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
				GameObject drop = GameObject.Instantiate(gFallingRingPrefab, blockTrans.position, blockTrans.rotation) as GameObject;
				drop.transform.localScale = new Vector3(gBlockScale * 0.275f, gBlockScale * 0.275f, gBlockScale * 0.275f);
				drop.GetComponent<Renderer>().material.color = new Color(blockColor.r * 0.5f, blockColor.g * 0.5f, blockColor.b * 0.5f);
				
				// Add to the size & colour of the player's progress bar
				if (gGameMode != GameModes.ScoreChallenge)
				{
					++gPlayerBarAmount;
					gPlayerBarColorTotal += new Vector3(blockColor.r, blockColor.g, blockColor.b);
					UpdateGameplayProgressBar(gProgressBarPlayerBar, (float)(gPlayerBarAmount) / (float)(gPlayerBarCapacity), gPlayerBarColorTotal / (float)(gPlayerBarAmount));
				}
			}

			// Add background pulse
			gFlowerOfLifeScript.StartPulse(blockColor);
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
					PlayBlockAudio(scorePopupPos, gBlockDisappearAudio[0], true);
					break;
				
				case 3:
					PlayBlockAudio(scorePopupPos, gBlockDisappearAudio[1], true);
					break;

				case 4:
					PlayBlockAudio(scorePopupPos, gBlockDisappearAudio[2], true);
					break;
				
				default:
					PlayBlockAudio(scorePopupPos, gBlockDisappearAudio[3], true);
					break;
			}

			// Give extra score for harder difficulty
			int scoreThisFrame = 1 << scoreChain;
			scoreThisFrame += Convert.ToInt32(Convert.ToSingle(scoreThisFrame) * gScoreDifficultyMult);
			scoreThisFrame += Convert.ToInt32(gLevel * 3.0f / Convert.ToSingle(gLevelMax - gLevelMin));
			scoreThisFrame *= 10;
			SetScore(gScore + scoreThisFrame);

			TextGrowAndFade.StartPopupText(scorePopupPos, TowerCamera.Instance.transform.rotation, gGUITextLevel.color, scoreThisFrame.ToString() + ((scoreChain > 2) ? "!" : ""));
		}
	}
	

	/// <summary> Returns the number wrapped into the range [0..gColums) </summary>
	/// <param name='col'> Unwrapped column </param>
	/// <returns> Wrapped column from 0 to (gColumns - 1) </returns>
	private int WrapCol(int col)
	{
		while (col < 0) { col += gColumns; }
		while (col >= gColumns) { col -= gColumns; }
		return col;
	}
	
	
	/// <summary>Positions the selectors at the specified col (and col+1) and row. </summary>
	/// <param name='colLeft'> Column for left half of selector (right half will add 1) </param>
	/// <param name='row'> Row (for both halves) </param>
	private void SetSelectorPos(int colLeft, int row)
	{
		gSelectorLeftCol = colLeft;
		gSelectorRow = row;

		gSelectorLeft.transform.localEulerAngles = new Vector3(0.0f, Block.CalcAngleDeg(colLeft, gColumns), 0.0f);
		gSelectorLeft.transform.localPosition = Block.CalcPosition(WrapCol(colLeft), row, gSelectorLeft.transform.localEulerAngles.y, gTowerRadius, gBlockScale);

		gSelectorRight.transform.localEulerAngles = new Vector3(0.0f, Block.CalcAngleDeg(WrapCol(colLeft + 1), gColumns), 0.0f);
		gSelectorRight.transform.localPosition = Block.CalcPosition(WrapCol(colLeft + 1), row, gSelectorRight.transform.localEulerAngles.y, gTowerRadius, gBlockScale);
	}


	/// <summary> Updates the selector's swapping animation </summary>
	/// <param name='dTime'> Time elapsed since last Update() </param>
	private void UpdateSelectorSwapAnim(float dTime)
	{
		gSelectorSwapAnimOffset -= gSelectorSwapAnimSpeed * dTime;
		if (gSelectorSwapAnimOffset < 0.0f) { gSelectorSwapAnimOffset = 0.0f; }
	
		Vector3 leftPos = Block.CalcPosition(WrapCol(gSelectorLeftCol), gSelectorRow, gSelectorLeft.transform.localEulerAngles.y, gTowerRadius, gBlockScale);
		Vector3 rightPos = Block.CalcPosition(WrapCol(gSelectorLeftCol + 1), gSelectorRow, gSelectorRight.transform.localEulerAngles.y, gTowerRadius, gBlockScale);

		gSelectorRight.transform.localPosition = new Vector3(Mathf.Lerp(rightPos.x, leftPos.x, gSelectorSwapAnimOffset), rightPos.y, Mathf.Lerp(rightPos.z, leftPos.z, gSelectorSwapAnimOffset));
		gSelectorLeft.transform.localPosition = new Vector3(Mathf.Lerp(leftPos.x, rightPos.x, gSelectorSwapAnimOffset), leftPos.y, Mathf.Lerp(leftPos.z, rightPos.z, gSelectorSwapAnimOffset));
	}
	

	/// <summary> Saves the current state of play </summary>
	private void QuickSave()
	{
		// Save game progress
		PlayerPrefs.SetFloat(Constants.kQSLevel, gLevel);
		PlayerPrefs.SetInt(Constants.kQSStartingLevel, Convert.ToInt32(gStartingLevel));
		PlayerPrefs.SetInt(Constants.kQSSelectorLeftCol, gSelectorLeftCol);
		PlayerPrefs.SetInt(Constants.kQSSelectorRow, gSelectorRow);
		PlayerPrefs.SetInt(Constants.kQSScore, gScore);
		
		// Save the rings
		PlayerPrefs.SetInt(Constants.kQSJarDrops, gPlayerBarAmount);
		PlayerPrefs.SetFloat(Constants.kQSJarTotalColorR, gPlayerBarColorTotal.x);
		PlayerPrefs.SetFloat(Constants.kQSJarTotalColorG, gPlayerBarColorTotal.y);
		PlayerPrefs.SetFloat(Constants.kQSJarTotalColorB, gPlayerBarColorTotal.z);

		// Save blocks in the tower
		List<BlockInfo> blockInfoList = BackupBlocks();
		string allBlocksStr = string.Empty;
		foreach (BlockInfo bInfo in blockInfoList)
		{
			// Convert blockInfo to 4 byte string
			allBlocksStr += bInfo.mCol.ToString("X2");
			allBlocksStr += bInfo.mRow.ToString("X2");
			allBlocksStr += bInfo.mBlockID.ToString("X2");
			if (bInfo.mFallingOffset < 0.0f) { bInfo.mFallingOffset = 0.0f; }
			if (bInfo.mFallingOffset >= 1.0f) { bInfo.mFallingOffset = 0.999999f; }
			uint fallingOffset = Convert.ToUInt32(bInfo.mFallingOffset * 255.0f);
			allBlocksStr += fallingOffset.ToString("X2");
		}
		PlayerPrefs.SetString(Constants.kQSBlocks, allBlocksStr);
		PlayerPrefs.SetInt(Constants.kQSExists, 1);
		PlayerPrefs.Save();
	}
	
	
	/// <summary> Loads the saved state from PlayerPrefs </summary>
	public void QuickLoad()
	{
		// Restore game progress
		SetStartingLevel(PlayerPrefs.GetInt(Constants.kQSStartingLevel));
		gLevel = PlayerPrefs.GetFloat(Constants.kQSLevel);
		gLevelInt = -1;	// Forces UpdateLevelProgress() to refresh "changed" level
		UpdateLevelProgress(0.0f);
		SetSelectorPos(PlayerPrefs.GetInt(Constants.kQSSelectorLeftCol), PlayerPrefs.GetInt(Constants.kQSSelectorRow));
		TowerCamera.Instance.RotateTowards(GetSelectorAngle());
		SetScore(PlayerPrefs.GetInt(Constants.kQSScore));
		
		// Restore the jar
		gPlayerBarAmount = PlayerPrefs.GetInt(Constants.kQSJarDrops);
		gPlayerBarColorTotal = new Vector3(PlayerPrefs.GetFloat(Constants.kQSJarTotalColorR), PlayerPrefs.GetFloat(Constants.kQSJarTotalColorG), PlayerPrefs.GetFloat(Constants.kQSJarTotalColorB));

		// Restore blocks in the tower
		ClearBlocks(true);
		List<BlockInfo> blockInfoList = new List<BlockInfo>();
		string allBlocksStr = PlayerPrefs.GetString(Constants.kQSBlocks);
		int strPos = 0;
		while (strPos < allBlocksStr.Length)
		{
			int col = int.Parse(allBlocksStr.Substring(strPos, 2), System.Globalization.NumberStyles.HexNumber);
			strPos += 2;
			int row = int.Parse(allBlocksStr.Substring(strPos, 2), System.Globalization.NumberStyles.HexNumber);
			strPos += 2;
			int blockID = int.Parse(allBlocksStr.Substring(strPos, 2), System.Globalization.NumberStyles.HexNumber);
			strPos += 2;
			int fallingOffsetInt = int.Parse(allBlocksStr.Substring(strPos, 2), System.Globalization.NumberStyles.HexNumber);
			strPos += 2;
			float fallingOffset = Convert.ToSingle(fallingOffsetInt) / 255.0f;

			blockInfoList.Add(new BlockInfo(col, row, blockID, fallingOffset));
		}
		RestoreBlocks(blockInfoList);
		UpdateNewBlocks(0.0f);
		
		// Remove "quick save exists" flag
		PlayerPrefs.SetInt(Constants.kQSExists, 0);
	}
}
