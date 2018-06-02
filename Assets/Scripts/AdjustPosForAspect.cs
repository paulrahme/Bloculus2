using UnityEngine;
using System.Collections;

public class AdjustPosForAspect : MonoBehaviour
{
	public Vector3		gPositionOffset10_16 = Vector3.zero;					// Transform's localPosition offset in 16:10 aspect ratios
	public Vector3		gPositionOffset2_3 = Vector3.zero;						// Transform's localPosition offset in 3:2 aspect ratios
	public Vector3		gPositionOffset3_4 = Vector3.zero;						// Transform's localPosition offset in 4:3 aspect ratios

	/// <summary> Called when object/script activates. </summary>
	void Awake()
	{
		if ((float)Screen.height / (float)Screen.width < 1.35f)
		{
			transform.localPosition += gPositionOffset3_4;
		}
		else if ((float)Screen.height / (float)Screen.width < 1.55f)
		{
			transform.localPosition += gPositionOffset2_3;
		}
		else if ((float)Screen.height / (float)Screen.width < 1.7f)
		{
			transform.localPosition += gPositionOffset10_16;
		}

		this.enabled = false;
	}
}
