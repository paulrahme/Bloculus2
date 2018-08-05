using UnityEngine;

public class UI_MainMenu : MonoBehaviour
{
	public void StartGameMode_Original()
	{
		GameMaster.instance.StartGame(GameMode.GameModeTypes.Original);
		gameObject.SetActive(false);
	}
}
