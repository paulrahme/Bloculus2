using UnityEngine;

public class Shockwave : MonoBehaviour
{
	#region Inspector variables

	[SerializeField] float	lifetime = 2.0f;
	[SerializeField] float	growScale = 5.0f;

	#endregion	// Inspector variables

	float fadeAmount;

	/// <summary> Creates (or reuses) a shockwave GameObject </summary>
	/// <param name="_position"> Centre position </param>
	/// <param name="_color"> Ripple's colour </param>
	public static void StartRipple(GameObject _prefab, Vector3 _position, Color _color)
	{
		GameObject gameObj = RecyclePool.RetrieveOrCreate(RecyclePool.PoolTypes.Shockwave, _prefab);

		// Set position & rotation
		gameObj.transform.parent = Environment.instance.transform;
		gameObj.transform.position = _position;
		gameObj.transform.localScale = Vector3.zero;
		gameObj.GetComponent<Renderer>().material.color = _color;

		// (Re)start popup animation
		Shockwave shockwaveScript = gameObj.GetComponent<Shockwave>();
		shockwaveScript.Reset();

		Environment.instance.shockwaves.Add(shockwaveScript);
	}

	/// <summary> Restarts the animation </summary>
	void Reset()
	{
		fadeAmount = 1.0f;
	}

	/// <summary> Called from Environment's Update() </summary>
	public void UpdateRipple(float _dTime)
	{
		fadeAmount -= _dTime / lifetime;
		if (fadeAmount <= 0.0f)
		{
			Environment.instance.shockwaves.Remove(this);
			RecyclePool.Recycle(RecyclePool.PoolTypes.Shockwave, gameObject);
		}
		else
		{
			// Grow
			float scale = (1.0f - fadeAmount) * growScale;
			transform.localScale = new Vector3(scale, scale, scale);
			
			// Fade
			Color newColor = GetComponent<Renderer>().material.color;
			newColor.a = fadeAmount;
			GetComponent<Renderer>().material.color = newColor;
		}
	}	
}
