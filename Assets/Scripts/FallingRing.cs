using UnityEngine;

public class FallingRing : MonoBehaviour
{
	#region Inspector variables

	[SerializeField] Vector3	acceleration = new Vector3(0.0f, -0.2f, 0.0f);
	[SerializeField] float		spinSpeed = 720.0f;
	[SerializeField] bool		createShockwave = true;

	#endregion   // Inspector variables

	float targetYPos;
	Vector3 velocity;
	Vector3 cachedSpinVec;
	Transform myTrans;
	Material myMaterial;

	/// <summary> Initialises the colour </summary>
	/// <param name="_position"> Transform's position </param>
	/// <param name="_scale"> Transform's scale </param>
	/// <param name="_color"> Colour to tint the material </param>
	public void Init(Vector3 _position, float _scale, Color _color)
	{
		myTrans = transform;
		myTrans.parent = Environment.instance.transform;
		myTrans.position = _position;
		myTrans.localScale = new Vector3(_scale, _scale, _scale);
		myMaterial = GetComponent<Renderer>().material;
		myMaterial.color = new Color(_color.r * 0.5f, _color.g * 0.5f, _color.b * 0.5f);
		targetYPos = GroundController.instance.myTrans.position.y;
		velocity = Vector3.zero;
		cachedSpinVec = new Vector3(0.0f, spinSpeed, 0.0f);
		gameObject.SetActive(true);
	}
	
	/// <summary> Called from the parent Tower's Update() </summary>
	public void UpdateFalling(float _dTime)
	{
		if (myTrans.position.y <= targetYPos)
		{
			if (createShockwave)
			{
				// Add ripple & create pulse
				GroundController.instance.AddRipple(myTrans.position.x);
				Vector3 ripplePos = new Vector3(myTrans.position.x, GroundController.instance.transform.position.y, myTrans.position.z);
				Environment.instance.StartRipple(ripplePos, myMaterial.color);
			}

			// Disappear
			Environment.instance.RingFinishedFalling(this);
		}
		else
		{
			velocity += acceleration * _dTime;
			myTrans.position += velocity;
			if (spinSpeed != 0.0f)
				transform.eulerAngles += cachedSpinVec * _dTime;
		}
	}	
}
