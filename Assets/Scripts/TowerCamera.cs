using UnityEngine;
using System.Collections;
using System;

public class TowerCamera : MonoBehaviour
{
	// Enums & constants
	public enum					eStates { Gameplay, Menu };													// Behaviour states
	public enum					eRotation { Idle, Clockwise, AntiClockwise };								// Rotating behaviour
	
	// Public variables
	public float				gRotateSpeed = 360.0f;														// Rotation in degrees per second
	public float				gLerpSpeed = 0.3f;															// Speed for lerping towards target position
	public eRotation			gRotState;																	// Current rotation behaviour

	// Public (non-editor) variables
	[HideInInspector]
	public GameObject			gGameObjectJustTapped;														// Which GameObject (if any) has just been tapped
	[HideInInspector]
	public eStates				gState { get; private set; }												// Current behaviour
	[HideInInspector]
	public float				gTargetRotation;															// Target rotation in degrees
	
	// Private variables
	private float				gRotation;																	// Unwrapped rotation in degrees, ie. not wrapped to [0..360)
	private float				gPivotRadius;																// Radius of orbit spinning around the the tower
	private bool				gIsLerping;																	// When true, camera position is lerping towards target position
	private Vector3				gTargetPosition;															// Positon to lerp towards

	// Helper/inline functions
	public void					SetBackgroundColor(Color color) { GetComponent<Camera>().backgroundColor = color; }

	// Instance
    public static TowerCamera	gInstance { get; private set; }
	
	
	/// <summary> Changes state and reacts accordingly </summary>
	/// <param name='newState'> eStates.... state name </param>
	public void SetState(eStates newState)
	{
		gState = newState;
		switch (gState)
		{
			case eStates.Gameplay:
				transform.position = gTargetPosition;
				gIsLerping = false;
				break;
			
			case eStates.Menu:
				break;
			
			default:
				throw new Exception("Unhandled state "+gState);
		}
	}


	/// <summary> Called when object/script initiates </summary>
	void Awake()
	{
		gInstance = this;
		gTargetPosition = transform.position;
		gIsLerping = false;
		SetState(eStates.Menu);
	}


	/// <summary> Called once per frame </summary>
	void Update()
	{
		float dTime = Time.deltaTime;

		switch (gState)
		{
			case eStates.Gameplay:
				UpdateMouseOverObject();
				UpdateRotation(dTime);
				break;
			
			case eStates.Menu:
				UpdateMouseOverObject();
				if (gIsLerping) { UpdateLerping(); }
				UpdateRotation(dTime);
				break;

			default:
				throw new Exception("Unhandled camera state: "+gState);
		}
	}


	/// <summary> Updates the position from the tower's size </summary>
	public void RefreshPosition()
	{
		float height = Tower.gInstance.gRows * Tower.gInstance.gBlockScale / 2.0f;
		float minPivotDist;
		if ((Screen.orientation == ScreenOrientation.Portrait) || (Screen.orientation == ScreenOrientation.PortraitUpsideDown))
		{
			minPivotDist = -12.0f;
		}
		else
		{
			minPivotDist = -6.0f;
		}
		gPivotRadius = minPivotDist - (Tower.gInstance.gBlockScale * Tower.gInstance.gRows);
		gTargetPosition = new Vector3(transform.position.x, height, transform.position.z);
		gIsLerping = true;
	}
	
	
	/// <summary> Updates which GameObject the mouse is over </summary>
	private void UpdateMouseOverObject()
	{
		bool tapped = false;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
		if (Input.GetMouseButtonDown(0))
		{
			tapped = true;
			ScreenTapped(Input.mousePosition);
		}
#else
		for (int i = 0; i < Input.touchCount; ++i)
		{
			Touch touch = Input.touches[i];
			if (touch.phase == TouchPhase.Began)
			{
				tapped = true;
				ScreenTapped(new Vector3(touch.position.x, touch.position.y, 0.0f));
			}
		}
#endif

		// Screen not tapped?
		if (!tapped)
		{
			gGameObjectJustTapped = null;
		}
	}


	/// <summary> What to do when the screen was tapped </summary>
	/// <param name="inputPos"> Input position </param>
	private void ScreenTapped(Vector3 inputPos)
	{
		Ray ray = GetComponent<Camera>().ScreenPointToRay(inputPos);
		RaycastHit hit;
		
		if (Physics.Raycast(ray, out hit))
		{
			gGameObjectJustTapped = hit.transform.gameObject;
		}
	}
	
	
	/// <summary> Updates the camera's position, lerping towards the target position </summary>
	private void UpdateLerping()
	{
		transform.position = new Vector3(	Mathf.Lerp(transform.position.x, gTargetPosition.x, gLerpSpeed),
											Mathf.Lerp(transform.position.y, gTargetPosition.y, gLerpSpeed), 
											Mathf.Lerp(transform.position.z, gTargetPosition.z, gLerpSpeed));
		if ((gTargetPosition - transform.position).magnitude < 0.1f)
		{
			transform.position = gTargetPosition;
			gIsLerping = false;
		}
	}
	

	/// <summary> Resets the rotation back to the starting angle </summary>
	public void ResetRotation()
	{
		gRotation = gTargetRotation = -(180.0f / (float)Tower.gInstance.gColumns);
		gRotState = eRotation.AntiClockwise;
	}
	

	/// <summary> Starts pivot-rotating towards the specified angle </summary>
	/// <param name='angle'> Angle in degrees </param>
	public void RotateTowards(float angle)
	{
		gTargetRotation = angle;
		
		// Handle 360 degree wrap around
		if (gTargetRotation - gRotation > 180.0f)
		{
			gRotation += 360.0f;
		}
		else if (gRotation - gTargetRotation > 180.0f)
		{
			gRotation -= 360.0f;
		}

		gRotState = (gTargetRotation > gRotation) ? eRotation.AntiClockwise : eRotation.Clockwise;
	}


	/// <summary> Rotates the tower towards the current selection </summary>
	/// <param name='dTime'> Time elapsed since last Update() </param>
	private void UpdateRotation(float dTime)
	{
		switch (gRotState)
		{
			case eRotation.Idle:
				break;
			
			case eRotation.Clockwise:
				gRotation -= gRotateSpeed * dTime;
				if (gRotation < gTargetRotation)
				{
					gRotation = gTargetRotation;
					gRotState = eRotation.Idle;
				}
				UpdatePivot();
				break;
			
			case eRotation.AntiClockwise:
				gRotation += gRotateSpeed * dTime;
				if (gRotation > gTargetRotation)
				{
					gRotation = gTargetRotation;
					gRotState = eRotation.Idle;
				}
				UpdatePivot();
				break;
			
			default:
				throw new Exception("Unhandled tower state: "+gRotState);
		}
	}
	
	
	/// <summary> Updates the pivoted position & angle </summary>
	private void UpdatePivot()
	{
		float angleRad = (gRotation + 180.0f) * Mathf.PI / 180.0f;
		gTargetPosition = new Vector3(gPivotRadius * Mathf.Sin(angleRad), gTargetPosition.y, gPivotRadius * Mathf.Cos(angleRad));
		if (gState == eStates.Menu)
		{
			gIsLerping = true;
		}
		else
		{
			transform.position = gTargetPosition;
		}
		transform.eulerAngles = new Vector3(0.0f, gRotation + 180, 0.0f);
	}
}
