using UnityEngine;

public class UI_HUD : MonoBehaviour
{
	public void PauseGame()
	{
		UIMaster.instance.Pause();
		GameMaster.instance.Pause();
	}
}
