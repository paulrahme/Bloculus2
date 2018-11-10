using UnityEngine;
using UnityEngine.UI;

public class UI_Settings : MonoBehaviour
{
	#region Inspector variables

	[Header("Hierarchy")]
	[SerializeField] Text player1ControlsText = null;
	[SerializeField] Text player2ControlsText = null;

	#endregion // Inspector variables

	/// <summary> Called when object/script is enabled in the hierarchy </summary>
	void OnEnable()
	{
		player1ControlsText.text = GameMaster.instance.GetPlayerControls(0).ToString();
		player2ControlsText.text = GameMaster.instance.GetPlayerControls(1).ToString();
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
}
