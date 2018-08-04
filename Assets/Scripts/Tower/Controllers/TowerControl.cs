using UnityEngine;

public abstract class TowerControl : MonoBehaviour
{
	#region Override these

	protected abstract bool MovedLeft();
	protected abstract bool MovedRight();
	protected abstract bool MovedUp();
	protected abstract bool MovedDown();
	protected abstract bool SwitchedBlocks();

	#endregion	// Override these

	/// <summary> Called once per frame from the Tower </summary>
	public void UpdateControls(float _dTime, ref Tower.ControlData _data)
	{
		if (MovedLeft())
			_data.horizontalDir = Tower.ControlData.HorizontalDirs.Left;
		else if (MovedRight())
			_data.horizontalDir = Tower.ControlData.HorizontalDirs.Right;
		else
			_data.horizontalDir = Tower.ControlData.HorizontalDirs.None;

		if (MovedUp())
			_data.verticalDir = Tower.ControlData.VerticalDirs.Up;
		else if (MovedDown())
			_data.verticalDir = Tower.ControlData.VerticalDirs.Down;
		else
			_data.verticalDir = Tower.ControlData.VerticalDirs.None;

		_data.switchBlocks = SwitchedBlocks();
	}
}
