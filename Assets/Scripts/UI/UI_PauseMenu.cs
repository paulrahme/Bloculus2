using UnityEngine;

public class UI_PauseMenu : MonoBehaviour
{
	public void UnpauseGame()
	{
		GameMaster.instance.UnpauseGame();
		UIMaster.instance.Unpause();
	}

	public void QuitGame()
	{
		GameMaster.instance.UnpauseGame();
		GameMaster.instance.QuitGame();
		UIMaster.instance.Quit();
	}
}
