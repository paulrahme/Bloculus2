using UnityEngine;

public class UI_MainMenu : MonoBehaviour
{
	public enum Screens { Title, GameMode }

	#region Inspector variables

	[Header("Hierarchy")]
	[SerializeField] GameObject titleContainer = null;
	[SerializeField] GameObject gameModesContainer = null;
	[SerializeField] GameObject settingsContainer = null;
	[SerializeField] GameObject backButton = null;
	[SerializeField] GameObject settingsButton = null;

	#endregion // Inspector variables

	Screens currentScreen;

	/// <summary> Called before first Update() </summary>
	void Start()
	{
		ShowScreen_Title();
	}

	#region Screens

	/// <summary> Switches to specified screen </summary>
	/// <param name="_screen"> Screen to show </param>
	void SwitchToScreen(Screens _screen)
	{
		currentScreen = _screen;

		switch (currentScreen)
		{
			case Screens.Title:
				titleContainer.SetActive(true);
				gameModesContainer.SetActive(false);
				break;

			case Screens.GameMode:
				titleContainer.SetActive(false);
				gameModesContainer.SetActive(true);
				break;

			default:
				throw new UnityException("Unhandled currentScreen " + currentScreen);
		}

		backButton.SetActive(currentScreen != Screens.Title);
	}

	/// <summary> Interface for Buttons' OnClick events </summary>
	public void ShowScreen_Title()		{ SwitchToScreen(Screens.Title); }
	public void ShowScreen_GameModes()	{ SwitchToScreen(Screens.GameMode); }

	public void ShowScreen_Settings()
	{
		settingsContainer.SetActive(true);
		settingsButton.SetActive(false);
		backButton.SetActive(true);
	}

	#endregion // Screens

	#region Game Modes

	/// <summary> Starts the specified game </summary>
	/// <param name="_gameMode"> Game mode to start </param>
	void StartGameMode(GameMode.GameModeTypes _gameMode)
	{
		GameMaster.instance.StartGame(_gameMode);
		UIMaster.instance.GameplayStarted();
	}

	/// <summary> Interface for Buttons' OnClick events </summary>
	public void StartGameMode_Original()			{ StartGameMode(GameMode.GameModeTypes.Original); }
	public void StartGameMode_Arcade()				{ StartGameMode(GameMode.GameModeTypes.Arcade); }
	public void StartGameMode_PVPLocalContinuous()	{ StartGameMode(GameMode.GameModeTypes.PVPLocal_Continuous); }
	public void StartGameMode_PVPLocalRace()		{ StartGameMode(GameMode.GameModeTypes.PVPLocal_Race); }

	#endregion // Game Modes

	#region Misc navigation

	/// <summary> Called from Back Button's OnClick event </summary>
	public void BackButtonPressed()
	{
		if (settingsContainer.activeSelf)
		{
			settingsContainer.SetActive(false);
			settingsButton.SetActive(true);
			SwitchToScreen(currentScreen);
		}
		else switch (currentScreen)
		{
			case Screens.Title:
				UIMaster.instance.ShowQuitConfirmation();
				break;
				
			case Screens.GameMode:
				SwitchToScreen(Screens.Title);
				break;

			default:
				throw new UnityException("Unhandled currentScreen " + currentScreen);
		}
	}

	#endregion // Misc navigation
}
