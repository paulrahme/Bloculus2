using UnityEngine;
using System.Collections;

public class Constants
{
	// Misc settings
	public const int				kMinHighestLevel = 10;													// Cubes for this level & lower are always unlocked

	// Player Prefs
	public const string				kPPVersionNo = "VersionNo";												// Version number of saved data
	public const string				kPPHiScore = "HiScore";													// Level's high score (concat with game mode & level number)
	public const string				kPPHiScoreName = "HiScoreName";											// Level's high score record holder (concat with game mode & level number)
	public const string				kPPHighestLevel = "HighestLevel";										// Highest level reached / unlocked
	public const string				kPPStartingLevel = "StartingLevel";										// Preferred starting level
	public const string				kPPBlockStyle = "BlockStyle";											// Preferred starting level
	public const string				kPPMusicEnabled = "MusicEnabled";										// Music enabled?
	public const string				kPPSoundEnabled = "SoundEnabled";										// Sound FX enabled?
	public const string				kPPControlMethod = "ControlMethod";										// Control method - swipe, buttons, etc
	public const string				kPPShowReleaseNotes = "ShowReleaseNotes";								// Don't show "What's new" (concat with version number)

	// Quick save
	public const string				kQSExists = "QuickSaveExists";											// Whether or not there is a game quick-saved
	public const string				kQSLevel = "QuickSaveLevel";											// Level value
	public const string				kQSStartingLevel = "QuickSaveStartingLevel";							// Which level the game started on
	public const string				kQSJarDrops = "QuickSaveJarDrops";										// How full the jar is
	public const string				kQSJarTotalColorR = "QuickSaveJarTotalColorRed";						// Colour of the fluid
	public const string				kQSJarTotalColorG = "QuickSaveJarTotalColorGreen";						// Colour of the fluid
	public const string				kQSJarTotalColorB = "QuickSaveJarTotalColorBlue";						// Colour of the fluid
	public const string				kQSBlocks = "QuickSaveBlocks";											// String of block info
	public const string				kQSScore = "QuickSaveScore";											// Score
	public const string				kQSSelectorLeftCol = "QuickSaveSelectorLeftCol";						// Left selector's position
	public const string				kQSSelectorRow = "QuickSaveSelectorRow";								// Left selector's position
}
