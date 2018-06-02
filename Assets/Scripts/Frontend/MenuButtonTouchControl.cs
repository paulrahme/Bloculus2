using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuButtonTouchControl : MonoBehaviour
{
	// Enums
	public enum eControlActions { Left, Right, Up, Down, Switch };

	// Public variables
	public eControlActions				gControlAction;					// Control to perform when tapped
	public float						gPulseSpeed = 0.05f;			// Speed at which the pulse fades back to transparent

	// Private variables
	private Color						gColor;							// Colour for pulse effect
	private float						gOriginalAlpha;					// Original transparency (pulse fades back to this)


	/// <summary> Called when object/script activates </summary>
	void Awake()
	{
		gColor = GetComponent<Renderer>().material.color;
		gOriginalAlpha = gColor.a;
	}


	/// <summary> Called once per frame </summary>
	void Update()
	{
		if ((TowerCamera.gInstance.gGameObjectJustTapped == gameObject))
		{
			gColor.a = 1.0f;
			switch (gControlAction)
			{
				case eControlActions.Left:
					Tower.gInstance.MoveLeft();
					break;

				case eControlActions.Right:
					Tower.gInstance.MoveRight();
					break;

				case eControlActions.Up:
					Tower.gInstance.MoveUp();
					break;

				case eControlActions.Down:
					Tower.gInstance.MoveDown();
					break;

				case eControlActions.Switch:
					Tower.gInstance.SwitchBlocks();
					break;

				default:
					throw new UnityException("Unhandled ControlAction "+gControlAction);
			}
		}

		// Update pulse effect
		if (gColor.a > gOriginalAlpha)
		{
			gColor.a -= gPulseSpeed;
			GetComponent<Renderer>().material.color = gColor;
		}
	}
}
