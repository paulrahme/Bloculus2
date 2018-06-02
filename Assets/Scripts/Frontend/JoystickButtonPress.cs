using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickButtonPress : MonoBehaviour
{
	[SerializeField] MenuButton menuButton;
	[SerializeField] string joystickButton = "Switch Blocks";

	/// <summary> Called once per frame </summary>
	void Update()
	{
		if (Input.GetButtonDown(joystickButton))
			menuButton.ButtonPressed();
	}
}
