﻿using UnityEngine;

public class UI_MainMenu : MonoBehaviour
{
	void StartGameMode(GameMode.GameModeTypes _gameMode)
	{
		GameMaster.instance.StartGame(_gameMode);
		UIMaster.instance.GameplayStarted();
	}

	public void StartGameMode_Original()
	{
		StartGameMode(GameMode.GameModeTypes.Original);
	}

	public void StartGameMode_Arcade()
	{
		StartGameMode(GameMode.GameModeTypes.Arcade);
	}

	public void StartGameMode_PVPLocal()
	{
		StartGameMode(GameMode.GameModeTypes.PVPLocal);
	}
}
