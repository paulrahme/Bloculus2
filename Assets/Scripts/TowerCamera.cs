using UnityEngine;
using System;

public class TowerCamera : MonoBehaviour
{
	public enum GameStates { Gameplay, Menu };					// Behaviour states
	enum RotationStates { Idle, Clockwise, AntiClockwise };		// Rotating behaviour

	#region Inspector variables

	[SerializeField] float rotateSpeed = 270.0f;				// Rotation in degrees per second
	[SerializeField] float lerpSpeed = 0.3f;					// Speed for lerping towards target position
	[SerializeField] Camera myCam = null;						// Cached camera

	#endregion	// Inspector variables

	RotationStates rotationState;								// Current rotation behaviour
	GameStates gameState;										// Current behaviour
	public float targetAngle;									// Target rotation (degrees)
	float rawAngle;												// Unwrapped rotation, ie. not wrapped to [0..360)
	float pivotRadius;											// Radius of orbit spinning around the the tower
	Vector3 targetPosition;										// Positon to lerp towards
	bool isLerping;												// True when lerping towards target position

	/// <summary> Singleton instance </summary>
	public static TowerCamera Instance { get; private set; }

	/// <summary> Called when object/script activates </summary>
	void Awake()
	{
		if (Instance != null)
			throw new UnityException("Singleton instance already exists");
		Instance = this;

		targetPosition = transform.position;
		isLerping = false;
		SetState(GameStates.Menu);
	}

	/// <summary> Called once per frame </summary>
	void Update()
	{
		float dTime = Time.deltaTime;

		switch (gameState)
		{
			case GameStates.Gameplay:
				UpdateRotation(dTime);
				break;
			
			case GameStates.Menu:
				if (isLerping)
					UpdateLerping();
				UpdateRotation(dTime);
				break;

			default:
				throw new Exception("Unhandled camera state: "+gameState);
		}
	}

	/// <summary> Changes state and reacts accordingly </summary>
	/// <param name='newState'> eStates.... state name </param>
	public void SetState(GameStates newState)
	{
		gameState = newState;
		switch (gameState)
		{
			case GameStates.Gameplay:
				transform.position = targetPosition;
				isLerping = false;
				break;
			
			case GameStates.Menu:
				break;
			
			default:
				throw new Exception("Unhandled state "+gameState);
		}
	}

	/// <summary> Changes the color of the background </summary>
	/// <param name="_color"> New colour </param>
	public void SetBackgroundColor(Color _color)
	{
		myCam.backgroundColor = _color;
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
		pivotRadius = minPivotDist - (Tower.gInstance.gBlockScale * Tower.gInstance.gRows);
		targetPosition = new Vector3(transform.position.x, height, transform.position.z);
		isLerping = true;
	}

	/// <summary> Updates the camera's position, lerping towards the target position </summary>
	private void UpdateLerping()
	{
		transform.position = new Vector3(	Mathf.Lerp(transform.position.x, targetPosition.x, lerpSpeed),
											Mathf.Lerp(transform.position.y, targetPosition.y, lerpSpeed), 
											Mathf.Lerp(transform.position.z, targetPosition.z, lerpSpeed));
		if ((targetPosition - transform.position).magnitude < 0.1f)
		{
			transform.position = targetPosition;
			isLerping = false;
		}
	}

	/// <summary> Resets the rotation back to the starting angle </summary>
	public void ResetRotation()
	{
		rawAngle = targetAngle = -(180.0f / (float)Tower.gInstance.gColumns);
		rotationState = RotationStates.AntiClockwise;
	}

	/// <summary> Starts pivot-rotating towards the specified angle </summary>
	/// <param name='angle'> Angle in degrees </param>
	public void RotateTowards(float angle)
	{
		targetAngle = angle;
		
		// Handle 360 degree wrap around
		if (targetAngle - rawAngle > 180.0f)
		{
			rawAngle += 360.0f;
		}
		else if (rawAngle - targetAngle > 180.0f)
		{
			rawAngle -= 360.0f;
		}

		rotationState = (targetAngle > rawAngle) ? RotationStates.AntiClockwise : RotationStates.Clockwise;
	}

	/// <summary> Rotates the tower towards the current selection </summary>
	/// <param name='dTime'> Time elapsed since last Update() </param>
	private void UpdateRotation(float dTime)
	{
		switch (rotationState)
		{
			case RotationStates.Idle:
				break;
			
			case RotationStates.Clockwise:
				rawAngle -= rotateSpeed * dTime;
				if (rawAngle < targetAngle)
				{
					rawAngle = targetAngle;
					rotationState = RotationStates.Idle;
				}
				UpdatePivot();
				break;
			
			case RotationStates.AntiClockwise:
				rawAngle += rotateSpeed * dTime;
				if (rawAngle > targetAngle)
				{
					rawAngle = targetAngle;
					rotationState = RotationStates.Idle;
				}
				UpdatePivot();
				break;
			
			default:
				throw new Exception("Unhandled tower state: "+rotationState);
		}
	}
	
	/// <summary> Updates the pivoted position & angle </summary>
	void UpdatePivot()
	{
		float angleRad = (rawAngle + 180.0f) * Mathf.PI / 180.0f;
		targetPosition = new Vector3(pivotRadius * Mathf.Sin(angleRad), targetPosition.y, pivotRadius * Mathf.Cos(angleRad));
		if (gameState == GameStates.Menu)
		{
			isLerping = true;
		}
		else
		{
			transform.position = targetPosition;
		}
		transform.eulerAngles = new Vector3(0.0f, rawAngle + 180, 0.0f);
	}
}
