using UnityEngine;

public class TowerControlKeyboard : TowerControl
{
	KeyCode leftKey, rightKey, upKey, downKey, switchKey;

	/// <summary> Initialises with the specified key controls </summary>
	public void Init(KeyCode _leftKey, KeyCode _rightKey, KeyCode _upKey, KeyCode _downKey, KeyCode _switchKey)
	{
		leftKey = _leftKey; rightKey = _rightKey; upKey = _upKey; downKey = _downKey; switchKey = _switchKey;
	}

	protected override bool MovedLeft()			{ return Input.GetKeyDown(leftKey); }
	protected override bool MovedRight()		{ return Input.GetKeyDown(rightKey); }
	protected override bool MovedUp()			{ return Input.GetKeyDown(upKey); }
	protected override bool MovedDown()			{ return Input.GetKeyDown(downKey); }
	protected override bool SwitchedBlocks()	{ return Input.GetKeyDown(switchKey); }
}
