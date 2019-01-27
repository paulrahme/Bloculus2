using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMaster : MonoBehaviour
{
	public enum AudioClips { None, Select, Switch, Start, Swoosh, LevelUp }

	#region Inspector variables

	[Header("UI Hierarchy")]
	public UI_MainMenu mainMenu;
	public UI_HUD hud;
	public UI_InGameMenu pauseMenu;
	public UI_InGameMenu gameOverMenu;
	public PopupManager popups = null;

	[Header("Non-UI hierarchy")]
	public Camera myCamera = null;

	[Header("Audio")]
	[SerializeField] AudioSource audioSource = null;
	[SerializeField] AudioClip[] audioClipsSelect = null;
	[SerializeField] AudioClip[] audioClipsSwitch = null;
	[SerializeField] AudioClip audioClipStart = null;
	[SerializeField] AudioClip audioClipSwoosh = null;
	[SerializeField] AudioClip audioClipLevelUp = null;

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
		popups.ShowOrEnqueue(new PopupManager.PopupInfo()
		{
			_popupType = PopupManager.PopupTypes.YesNo,
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

	/// <summary> Plays the specified audio </summary>
	/// <param name="_clipName"> Audio to play </param>
	public void PlayAudio(AudioClips _clipName)
	{
		if (!Environment.instance.musicController.SfxEnabled)
			return;

		if (_clipName == AudioClips.None)
			return;

		AudioClip clip;
		switch (_clipName)
		{
			case AudioClips.Select:		clip = audioClipsSelect[Random.Range(0, audioClipsSelect.Length)]; break;
			case AudioClips.Switch:		clip = audioClipsSwitch[Random.Range(0, audioClipsSwitch.Length)];	break;
			case AudioClips.Start:		clip = audioClipStart; break;
			case AudioClips.Swoosh:		clip = audioClipSwoosh; break;
			case AudioClips.LevelUp:	clip = audioClipLevelUp; break;

			default: throw new UnityException("Unhandled Audio Clip " + _clipName);
		}
				
		audioSource.PlayOneShot(clip);
	}
}
