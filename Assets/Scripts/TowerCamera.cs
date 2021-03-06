using UnityEngine;

public class TowerCamera : MonoBehaviour
{
	#region Inspector variables

	[SerializeField] Camera myCam = null;
	[SerializeField] AnimationCurve blendAnimCurve = null;
	[SerializeField] float blendDuration = 0.25f;

	#endregion // Inspector variables

	Transform myTrans;
	Vector3 basePos;
	Vector3 sourcePos, targetPos;
	float sourceFov, targetFov;
	float blendSpeed;
	float blendProgress;
	bool blendPos, blendFov;

	/// <summary> Singleton instance </summary>
	public static TowerCamera instance;

	/// <summary> Called when object/script activates </summary>
	void Awake()
	{
		if (instance != null)
			throw new UnityException("Singleton instance already exists");
		instance = this;

		myTrans = transform;
		blendSpeed = 1.0f / blendDuration;
		blendPos = blendFov = false;
	}

	/// <summary> Called once per frame </summary>
	void Update()
	{
		bool finished = false;

		// Update progress, checking if reached the end
		blendProgress += blendSpeed * Time.deltaTime;
		if (blendProgress > 1.0f)
		{
			blendProgress = 1.0f;
			finished = true;
		}

		// Convert 0-1 progress into anim curve
		float animatedProgress = blendAnimCurve.Evaluate(blendProgress);
		if (blendPos)
			myTrans.position = Vector3.LerpUnclamped(sourcePos, targetPos, blendProgress);
		if (blendFov)
			myCam.fieldOfView = (sourceFov * (1.0f - animatedProgress)) + (targetFov * animatedProgress);

		// If finished, turn off updating
		if (finished)
			blendPos = blendFov = enabled = false;
	}

	/// <summary> Sets up the position + FoV for the current type of gameplay </summary>
	/// <param name="_layout"> Layout info struct </param>
	public void SetLayout(GameMaster.ViewLayout _layout)
	{
		basePos = myTrans.localPosition = sourcePos = targetPos = _layout.cameraPos;
		myCam.fieldOfView = sourceFov = targetFov = _layout.cameraFoV;
		blendPos = blendFov = false;
	}

	/// <summary> Changes the color of the background </summary>
	/// <param name="_color"> New colour </param>
	public void SetBackgroundColor(Color _color)
	{
		myCam.backgroundColor = _color;
	}

	/// <summary> Starts blending to a new height + distance </summary>
	public void StartBlending(float _height, float _distance)
	{
		sourcePos = myTrans.position;
		targetPos.x = sourcePos.x;
		targetPos.y = basePos.y + _height;
		targetPos.z = basePos.z + _distance;

		// Debug.Log("Blending to new height '" + _height + "', distance '" + _distance + "'\nPos '" + sourcePos + "' -> '" + targetPos + "'");

		blendPos = true;
		blendProgress = 0.0f;
		enabled = true;
	}

	/// <summary> Starts blending to a new field of view </summary>
	public void StartBlendingFov(float _fov)
	{
		sourceFov = myCam.fieldOfView;
		targetFov = _fov;
		blendFov = true;
		blendProgress = 0.0f;
		enabled = true;
	}
}
