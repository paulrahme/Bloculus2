using UnityEngine;

public class Shockwave : MonoBehaviour
{
	#region Inspector variables

	[SerializeField] float	lifetime = 2.0f;
	[SerializeField] float	growScale = 5.0f;

	#endregion	// Inspector variables

	float fadeAmount;
	Material myMaterial;
	Color color;
	Vector3 localScale;

	/// <summary> Restarts the animation </summary>
	public void Init(Color _color)
	{
		if (myMaterial == null)
			myMaterial = GetComponent<Renderer>().material;

		color = myMaterial.color = _color;
		fadeAmount = 1.0f;
	}

	/// <summary> Called from Environment's Update() </summary>
	public void UpdateRipple(float _dTime)
	{
		fadeAmount -= _dTime / lifetime;

		if (fadeAmount > 0.0f)
		{
			// Grow
			localScale.x = localScale.y = localScale.z = (1.0f - fadeAmount) * growScale;
			transform.localScale = localScale;

			// Fade
			color.a = fadeAmount;
			myMaterial.color = color;
		}
		else
			Environment.instance.ShockwaveFinished(this);
	}	
}
