using UnityEngine;
using System.Collections;

public class FallAndDisappear : MonoBehaviour
{
	#region Inspector variables

	[SerializeField] float		lifetime = 2.0f;
	[SerializeField] Vector3	acceleration = new Vector3(0.0f, -0.2f, 0.0f);
	[SerializeField] float		spinSpeed = 720.0f;
	[SerializeField] bool		createShockwave = true;

	#endregion	// Inspector variables

	float lifeCounter;
	Vector3 velocity;
	Vector3 cachedSpinVec;
	Transform myTrans;
	Material myMaterial;
	
	/// <summary> Called when object/script activates </summary>
	void Awake()
	{
		myTrans = transform;
		myMaterial = GetComponent<Renderer>().material;
		lifeCounter = 1.0f;
		velocity = Vector3.zero;
		cachedSpinVec = new Vector3(0.0f, spinSpeed, 0.0f);
	}
	
	/// <summary> Called from the parent Tower's Update() </summary>
	public void UpdateFalling(Tower _parentTower)
	{
		float dTime = Time.deltaTime;

		lifeCounter -= dTime / lifetime;
		if (lifeCounter <= 0.0f)
		{
			if (createShockwave)
			{
				// Add ripple & create pulse
				GroundController.Instance.AddRipple(myTrans.position.x);
				Vector3 ripplePos = new Vector3(myTrans.position.x, GroundController.Instance.transform.position.y, _parentTower.transform.position.z);
				RippleGrowAndFade.StartRipple(_parentTower.rippleRingPrefab, ripplePos, myMaterial.color);
			}

			// Disappear
			Destroy(gameObject);	// TODO: pool/recycle
		}
		else
		{
			velocity += acceleration * dTime;
			myTrans.position += velocity;
			if (spinSpeed != 0.0f)
				transform.eulerAngles += cachedSpinVec * dTime;
		}
	}	
}
