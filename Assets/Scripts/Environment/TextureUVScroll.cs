using UnityEngine;
using System.Collections;

public class TextureUVScroll : MonoBehaviour
{
	// Public variables
	public Vector2					gScrollSpeed;															// How quickly to scroll each axis, in cycles per second

	// Private variables
	private float					gTextureSize;															// Cached texture width/height
	private Vector2					gScrollOffset;															// Offset calculation


	/// <summary> Called whern object/script activates </summary>
	void Awake()
	{
		gTextureSize = GetComponent<Renderer>().material.mainTexture.width;
		gScrollOffset = Vector2.zero;
	}


	/// <summary> Called once per frame </summary>
	void Update()
	{
		// Apply scroll this frame
		gScrollOffset += gScrollSpeed * Time.deltaTime;

		// Keep it wrapped within the texture's size (v high values go crazy on some plaforms, eg. iOS)
		if (gScrollOffset.x < 0) { gScrollOffset.x += gTextureSize; }
		else if (gScrollOffset.x >= gTextureSize) { gScrollOffset.x -= gTextureSize; }
		if (gScrollOffset.y < 0) { gScrollOffset.y += gTextureSize; }
		else if (gScrollOffset.y >= gTextureSize) { gScrollOffset.y -= gTextureSize; }

		// Apply it to the material
		GetComponent<Renderer>().material.mainTextureOffset = gScrollOffset;
	}	
}
