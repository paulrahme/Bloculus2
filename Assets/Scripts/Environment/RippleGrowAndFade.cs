using UnityEngine;
using System.Collections.Generic;

public class RippleGrowAndFade : MonoBehaviour
{
	#region Inspector variables

	[SerializeField] float	lifetime = 2.0f;
	[SerializeField] float	growScale = 5.0f;

	#endregion	// Inspector variables

	float fadeAmount;
	static Stack<GameObject> recycleStack = new Stack<GameObject>();

	/// <summary> Creates (or reuses) a shockwave GameObject </summary>
	/// <param name="_position"> Centre position </param>
	/// <param name="_color"> Ripple's colour </param>
	public static void StartRipple(GameObject _prefab, Vector3 _position, Color _color)
	{
		GameObject gameObj = (recycleStack.Count > 0) ? recycleStack.Pop() : Instantiate(_prefab);

		// Set position & rotation
		gameObj.transform.parent = null;
		gameObj.transform.position = _position;
		gameObj.transform.localScale = Vector3.zero;
		gameObj.GetComponent<Renderer>().material.color = _color;

		// (Re)start popup animation
		RippleGrowAndFade shockwaveScript = gameObj.GetComponent<RippleGrowAndFade>();
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
			transform.parent = GameMaster.instance.recycledObjectPool;
			Environment.instance.shockwaves.Remove(this);
			recycleStack.Push(gameObject);
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
