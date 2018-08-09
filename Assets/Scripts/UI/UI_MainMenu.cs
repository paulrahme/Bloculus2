using UnityEngine;

public class UI_MainMenu : MonoBehaviour
{
	public void StartGameMode_Original()
	{
		GameMaster.instance.StartGame(GameMode.GameModeTypes.Original);
		UIMaster.instance.hud.gameObject.SetActive(true);
		gameObject.SetActive(false);
	}

	public void StartGameMode_Arcade()
	{
		GameMaster.instance.StartGame(GameMode.GameModeTypes.Arcade);
		UIMaster.instance.hud.gameObject.SetActive(true);
		gameObject.SetActive(false);
	}
}
