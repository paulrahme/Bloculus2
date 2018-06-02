using UnityEngine;
using System.Collections;

public class Ripple : MonoBehaviour
{
	// Public variables
	public float					gRippleAmount = 0.025f;											// How much to offset the starting vertices
	public Vector2					gScrollSpeedFastest = new Vector2(0.0f, -0.1f);					// Texture UV scroll speed
	public Vector2					gScrollSpeedSlowest = new Vector2(0.0f, -0.01f);				// Texture UV scroll speed
	public Material[]				gLevelMaterials;												// Textures for level groups

	// Public (non-editor) variables
	public Vector2					gScrollSpeed;													// Texture UV scroll speed

	// Private variables
	private Vector2					gScrollWrapAmounts;												// Where the scrolling should wrap around
	private Vector2					gScrollOffset;													// Offset calculation
	private Mesh					gMesh;															// Object's mesh
	private Vector3[]				gVertices;														// Mesh's vertices
	private int						gNumVertices;													// Number of vertices

	// Helper/inline functions
	public void						SetMaterial(int materialIdx) { GetComponent<Renderer>().material = gLevelMaterials[materialIdx]; }
	
	/// <summary> Called when the object/script initiates </summary>
	void Awake()
	{
		gScrollWrapAmounts = new Vector2(GetComponent<Renderer>().material.mainTexture.width * GetComponent<Renderer>().material.mainTexture.texelSize.x, GetComponent<Renderer>().material.mainTexture.height * GetComponent<Renderer>().material.mainTexture.texelSize.y);
		gScrollOffset = Vector2.zero;
		gMesh = GetComponent<MeshFilter>().mesh;
		gNumVertices = gMesh.vertices.Length;
		gVertices = gMesh.vertices;
	}
	
	/// <summary> Called once per frame by the Tower script, when the game is in play</summary>
	public void UpdateEffect()
	{
		const int gridRows = 64;
		for (var i = 1; i < gNumVertices - 1; i++)
		{
			if (i > gNumVertices - gridRows - 2)
			{
				gVertices[i].z += gVertices[i - 1].z + gVertices[i + 1].z;
				gVertices[i].z *= 0.3f;
			}
			else
			{
				gVertices[i].z += (gVertices[i + gridRows - 1].z + gVertices[i + gridRows + 1].z) * 2.0f;
				gVertices[i].z *= 0.21f;
			}
		}

		gMesh.vertices = gVertices;
		
		// Swap the ripple direction for the next update
		gRippleAmount = -gRippleAmount;
		
		// Scroll the texture
		// Apply scroll this frame
		gScrollOffset += gScrollSpeed * Time.deltaTime;
		
		// Keep it wrapped within the texture's size (v high values go crazy on some plaforms, eg. iOS)
		if (gScrollOffset.x < 0) { gScrollOffset.x += gScrollWrapAmounts.x; }
		else if (gScrollOffset.x >= gScrollWrapAmounts.x) { gScrollOffset.x -= gScrollWrapAmounts.x; }
		if (gScrollOffset.y < 0) { gScrollOffset.y += gScrollWrapAmounts.y; }
		else if (gScrollOffset.y >= gScrollWrapAmounts.y) { gScrollOffset.y -= gScrollWrapAmounts.y; }
		
		// Apply it to the material
		GetComponent<Renderer>().material.mainTextureOffset = gScrollOffset;
	}
	
	/// <summary> Starts a new ripple </summary>
	/// <param name='xPos'> Starting posiiton </param>
	public void AddRipple(float xPos)
	{
		if (xPos < -0.5f)
		{
			gVertices[2013].z -= gRippleAmount;
			gVertices[2014].z -= gRippleAmount;
		}
		else if (xPos > 0.5f)
		{
			gVertices[2015].z -= gRippleAmount;
			gVertices[2016].z -= gRippleAmount;
		}
		else
		{
			gVertices[2017].z -= gRippleAmount;
			gVertices[2018].z -= gRippleAmount;
		}
	}
}
