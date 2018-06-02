using UnityEngine;
using System.Collections;

public class TitleFont : MonoBehaviour
{
	// Public variables
	public GUIText					gGUIText;															// GUIText component
	
	/// <summary> Called when object/script initiates </summary>
	void Awake()
	{
		gGUIText.pixelOffset = new Vector2(0.0f, (Screen.height / 2.0f) - 50.0f);
		this.enabled = false;
	}
}
