using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMaster : MonoBehaviour
{
	#region Inspector variables
	public UI_MainMenu mainMenu;
	public UI_HUD hud;
	public UI_InGameMenu pauseMenu;
	public UI_InGameMenu gameOverMenu;
	public UI_PopupManager popups;
	public Camera myCamera;
	#endregion	// Inspector variables

	/// <summary> Singleton instance </summary>
	public static UIMaster instance;

	/// <summary> Called when object/script activates </summary>
	void Awake()
	{
#if UNITY_EDITOR
		if (GameMaster.instance == null)
		{
			SceneManager.LoadScene("Assets/Scenes/Game.unity");
			return;
		}
#endif
		if (instance != null)
			throw new UnityException("Singleton instance already exists");
		instance = this;

		myCamera.enabled = false;	// Not needed during runtime
	}

	/// <summary> Called before first Update() </summary>
	void Start()
	{
		mainMenu.gameObject.SetActive(true);
	}

	/// <summary> Called on start of game </summary>
	public void GameplayStarted()
	{
		hud.SetGameplayElementsVisible(true);
		hud.gameObject.SetActive(true);
		mainMenu.gameObject.SetActive(false);
	}

	/// <summary> Shows the pause screen </summary>
	public void Pause()
	{
		hud.SetGameplayElementsVisible(false);
		pauseMenu.gameObject.SetActive(true);
	}

	/// <summary> Shows the Game Over screen </summary>
	public void GameOver()
	{
		hud.SetGameplayElementsVisible(false);
		gameOverMenu.gameObject.SetActive(true);
	}

	/// <summary> Disables pause screen & goes back to gameplay </summary>
	public void Unpause()
	{
		pauseMenu.gameObject.SetActive(false);
		gameOverMenu.gameObject.SetActive(false);
		hud.SetGameplayElementsVisible(true);
	}

	/// <summary> Pops up the Quit confirmation dialog </summary>
	public void ShowQuitConfirmation()
	{
		popups.Show(new UI_PopupManager.PopupInfo()
		{
			_popupType = UI_PopupManager.PopupTypes.Default,
			title = "Quit?",
			messageBody = "Are you sure you want to close Bloculus?",
			confirmText = "Quit",
			cancelText = "Cancel",
			confirmCallback = Quit,
		});
	}

	/// <summary> Disables pause screen & quits back to main menu </summary>
	public void QuitToMainMenu()
	{
		hud.gameObject.SetActive(false);
		pauseMenu.gameObject.SetActive(false);
		gameOverMenu.gameObject.SetActive(false);
		mainMenu.gameObject.SetActive(true);
	}

	/// <summary> Quits & closes the game </summary>
	void Quit()
	{
		Debug.Log("Quit - Closing application!");
		Application.Quit();
	}
}
