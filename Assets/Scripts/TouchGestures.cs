using UnityEngine;
using System.Collections;
using System;

public class TouchGestures : MonoBehaviour
{
	// Constants & enums
	public enum						eGestureTypes { None, SwipeUp, SwipeLeft, SwipeRight, SwipeDown, Switch };
	private enum					eStates { None, RecentlyDown, SwipeRecognised, TwoFingersRecognised };
	
	// Public variables
	public float					gSwipeRecogniseDistance = 5.0f;									// Distance for a swipe - longer = slower but more accurate
	
	// Private variables
	private Vector3					gTouchDownPos;													// The initial position touched
	private eStates					gState;															// What the finger/stylus is currently doing
	private float					gSwipeRecogniseDistanceSquared;									// Cached squared distance
	
	/// <summary> Called when the object/script initiates </summary>
	void Awake()
	{
		gState = eStates.None;
		gSwipeRecogniseDistanceSquared = gSwipeRecogniseDistance * gSwipeRecogniseDistance;
	}
	
	/// <summary> Call this once per frame to update gestures </summary>
	public eGestureTypes UpdateGestures()
	{
		switch (gState)
		{
			case eStates.None:
				if (Input.GetMouseButtonDown(0))
				{
					gTouchDownPos = Input.mousePosition;
					gState = eStates.RecentlyDown;
				}
				return eGestureTypes.None;
				
			case eStates.RecentlyDown:
				// Two fingers?
				if (Input.touches.Length > 1)
				{
					gState = eStates.TwoFingersRecognised;
					return eGestureTypes.Switch;
				}
				else
				{
					// Moved far enough yet?
					Vector3 posDiff = Input.mousePosition - gTouchDownPos;
					if (posDiff.sqrMagnitude > gSwipeRecogniseDistanceSquared)
					{
						gState = eStates.SwipeRecognised;
	
						// Find axis it's moved further on
						if (Mathf.Abs(posDiff.y) > Mathf.Abs(posDiff.x))
						{
							// Moved vertically
							return (posDiff.y > 0) ? eGestureTypes.SwipeUp : eGestureTypes.SwipeDown;
						}
						else
						{
							// Moved horizontally
							return (posDiff.x > 0) ? eGestureTypes.SwipeRight : eGestureTypes.SwipeLeft;
						}
					}
					else
					{
						// If finger/stylus has released, end gesture
						if (Input.GetMouseButtonUp(0)) { gState = eStates.None; }
	
						return eGestureTypes.None;
					}
				}

			case eStates.SwipeRecognised:
			case eStates.TwoFingersRecognised:
				// Wait until finger/stylus has released, then end gesture
				if (Input.GetMouseButtonUp(0)) { gState = eStates.None; }

				return eGestureTypes.None;
				
			default:
				throw new Exception("Unhandled finger/stylus state '"+gState+"'");
		}
	}	
}
