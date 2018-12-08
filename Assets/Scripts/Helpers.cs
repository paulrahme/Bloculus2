using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class Helpers
{
	public static readonly Vector3 vec3Zero = new Vector3(0f, 0f, 0f);		// Cached properly because Vector3.zero isn't
	public static readonly Vector3 vec3One = new Vector3(1f, 1f, 1f);		// Cached properly because Vector3.one isn't

	/// <summary> Recursively sets the GameObject & all child objects to visible/invisible </summary>
	/// <param name='gameObj'> Root GameObject </param>
	/// <param name='visible'> True for visible, false for invisible </param>
	public static void SetGameObjectVisibility(GameObject gameObj, bool visible)
	{
		// Set GameObject's own renderer if it has one
		if (gameObj.GetComponent<Renderer>() != null)
		{
			gameObj.GetComponent<Renderer>().enabled = visible;
		}
		
		// Iterate through child objects, setting their renderers
		Renderer[] renderers = gameObj.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in renderers)
		{
			renderer.enabled = visible;
		}
	}

	/// <summary> Recursively sets the color of all materials in all children </summary>
	/// <param name='color'> Color to set </param>
	public static void SetGameObjectTintColor(GameObject gameObj, Color color)
	{
		// Set GameObject's own material if it has a renderer
		if (gameObj.GetComponent<Renderer>() != null)
		{
			foreach (Material material in gameObj.GetComponent<Renderer>().materials)
			{
				material.color = color;
			}
		}
		
		// Iterate through child objects, setting their materials
		foreach (Renderer renderer in gameObj.GetComponentsInChildren<Renderer>())
		{
			foreach (Material material in renderer.materials)
			{
				material.color = color;
			}
		}
	}

	/// <summary> Wraps the string to fit in the specified number of characters across </summary>
	/// <param name="strOrig"> String to wrap </param>
	/// <param name="width"> Max characters per line </param>
	/// <returns> Wrapped string </returns>
	public static string WordWrap(string strOrig, int width)
	{
		// Handle null string
		if (string.IsNullOrEmpty(strOrig))
		{
			return string.Empty;
		}
		else
		{
			char[] chars = strOrig.ToCharArray();
			
			int pos = Mathf.Min(width, chars.Length);
			while (pos < chars.Length)
			{
				// Trace back to the last space
				while (chars[pos] != ' ')
				{
					--pos;
					if (pos == 0) { return new string(chars); }
				}
				chars[pos] = '\n';
	
				// Jump to the next line
				pos += width;
			}
			
			return new string(chars);
		}
	}
	
 	/// <summary> Sets the target rotation, wrapping to the [0..360) range </summary>
	/// <param name='angle'> Rotation in degrees </param>
	public static float WrapAngle(float angle)
	{
		while (angle > 360.0f)
			angle -= 360.0f;
		while (angle < 0.0f)
			angle += 360.0f;
		return angle;
	}

	/// <summary> Sets a transform's parent and resets its local position, rotation and scale </summary>
	/// <param name="_trans"> Transform to set </param>
	/// <param name="_parent"> Parent transform </param>
	public static void ParentAndResetTransform(Transform _trans, Transform _parent)
	{
		_trans.parent = _parent;
		_trans.localPosition = _trans.localEulerAngles = vec3Zero;
		_trans.localScale = vec3One;
	}

	/// <summary> Recursively gets the full name in the hierarchy </summary>
	/// <param name="_transform"> (Final child) transform </param>
	public static string GetFullName(Transform _transform)
	{
		Stack<string> names = new Stack<string>();

		while (_transform != null)
		{
			names.Push(_transform.name);
			_transform = _transform.parent;
		}

		StringBuilder sb = new StringBuilder();
		if (names.Count > 0)
			sb.Append(names.Pop());
		while (names.Count > 0)
			sb.Append("/").Append(names.Pop());

		return sb.ToString();
	}
}