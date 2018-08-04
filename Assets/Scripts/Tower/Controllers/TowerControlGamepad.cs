using UnityEngine;

public class TowerControlGamepad : TowerControl
{
	protected override bool MovedLeft()			{ return (Input.GetAxis("Horizontal") < -0.1f); }
	protected override bool MovedRight()		{ return (Input.GetAxis("Horizontal") > 0.1f); }
	protected override bool MovedUp()			{ return (Input.GetAxis("Vertical") < -0.1f); }
	protected override bool MovedDown()			{ return (Input.GetAxis("Vertical") > 0.1f); }
	protected override bool SwitchedBlocks()	{ return Input.GetButtonDown("Switch Blocks"); }
}
