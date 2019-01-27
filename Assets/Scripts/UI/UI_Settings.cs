using UnityEngine;
using UnityEngine.UI;

public class UI_Settings : MonoBehaviour
{
	[System.Serializable]
	public class Toggle
	{
		public GameObject showWhenTrue = null;
		public GameObject showWhenFalse = null;

		/// <summary> Refreshes the hierarchy </summary>
		/// <param name="_show"> True or false state of the toggle </param>
		public void Refresh(bool _show)
		{
			showWhenTrue?.SetActive(_show);
			showWhenFalse?.SetActive(!_show);
		}
	}

	#region Inspector variables

	[Header("Hierarchy")]
	[SerializeField] Text player1ControlsText = null;
	[SerializeField] Text player2ControlsText = null;
	[SerializeField] Toggle musicEnabledHierarchy = null;
	[SerializeField] Toggle sfxEnabledHierarchy = null;

	#endregion // Inspector variables

	/// <summary> Called when object/script is enabled in the hierarchy </summary>
	void OnEnable()
	{
		player1ControlsText.text = GameMaster.instance.GetPlayerControls(0).ToString();
		player2ControlsText.text = GameMaster.instance.GetPlayerControls(1).ToString();
		musicEnabledHierarchy.Refresh(Environment.instance.musicController.MusicEnabled);
		sfxEnabledHierarchy.Refresh(Environment.instance.musicController.SfxEnabled);
	}

	/// <summary> Switches to the next control type </summary>
	/// <param name="_playerIdx"> Player index (0 = P1, etc) </param>
	/// <param name="_textLabel"> UI Text label to update </param>
	void ChangePlayerControls(int _playerIdx, Text _textLabel)
	{
		GameMaster.ControllerTypes controllerType = GameMaster.instance.ChangePlayerControls(_playerIdx);
		_textLabel.text = controllerType.ToString();
	}

	/// <summary> Interface for Buttons' OnClick events </summary>
	public void ChangeControlsPlayer1() { ChangePlayerControls(0, player1ControlsText); }
	public void ChangeControlsPlayer2() { ChangePlayerControls(1, player2ControlsText); }
	public void ToggleMusic() { musicEnabledHierarchy.Refresh(Environment.instance.musicController.ToggleMusic()); }
	public void ToggleSFX() { sfxEnabledHierarchy.Refresh(Environment.instance.musicController.ToggleSFX()); }
}
