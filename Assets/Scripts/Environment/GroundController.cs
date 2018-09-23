using UnityEngine;

public class GroundController : MonoBehaviour
{
	const int gridRows = 64;

	#region Inspector variables

	[SerializeField] float		rippleStrength = 0.03f;
	[SerializeField] float		rippleDropOffMult = 0.22f;
	[SerializeField] Vector2	scrollSpeedFastest = new Vector2(0.0f, -6.0f);
	[SerializeField] Vector2	scrollSpeedSlowest = new Vector2(0.0f, -0.6f);
	[SerializeField] Material[]	levelMaterials = null;

	#endregion // Inspector variables

	internal Transform myTrans;
	Vector2 scrollSpeed;
	Vector2 scrollWrapAmounts;
	Vector2 scrollOffset;
	Renderer mainRenderer;
	Material mainMaterial;
	Mesh mainMesh;
	Vector3[] meshVertices;
	int numVertices;

	/// <summary> Singleton instance </summary>
	public static GroundController instance;

	/// <summary> Called when object/script activates </summary>
	void Awake()
	{
		if (instance != null)
			throw new UnityException("Singleton instance already exists");
		instance = this;

		// Cache renderer + material
		myTrans = transform;
		mainRenderer = GetComponent<Renderer>();
		mainMaterial = mainRenderer.material;

		// Calculate scroll amounts
		Texture mainTexture = mainMaterial.mainTexture;
		scrollWrapAmounts = new Vector2(mainTexture.width * mainTexture.texelSize.x, mainTexture.height * mainTexture.texelSize.y);
		scrollOffset = Vector2.zero;

		// Cache vertices
		mainMesh = GetComponent<MeshFilter>().mesh;
		meshVertices = mainMesh.vertices;
		numVertices = meshVertices.Length;
	}

	/// <summary> Sets the position, scale, etc for the current gameplay's view </summary>
	/// <param name="_layout"> Layout info struct </param>
	public void SetLayout(GameMaster.ViewLayout _layout)
	{
		myTrans.localPosition = _layout.groundPos;
		myTrans.localScale = _layout.groundScale;
	}

	/// <summary> Sets the (tiled) material to use </summary>
	/// <param name="materialIdx"> Index into material array </param>
	public void SetMaterial(int materialIdx)
	{
		mainRenderer.material = levelMaterials[materialIdx];
		mainMaterial = mainRenderer.material;
	}

	/// <summary> Sets the scrolling speed of the background </summary>
	/// <param name="_progress"> Game's progress through all levels </param>
	public void SetScrollSpeed(float _progress)
	{
		scrollSpeed = scrollSpeedSlowest - ((scrollSpeedSlowest - scrollSpeedFastest) * _progress);

		// Special case for final texture - scrolls away from the player, slower
		if (_progress > 0.99f)
			scrollSpeed.y *= 0.5f;
	}
	
	/// <summary> Called once per frame by the Environment script, when the game is in play </summary>
	public void UpdateEffect()
	{
		for (var i = 1; i < numVertices - 1; i++)
		{
			// End of the mesh - don't look at next row
			if (i > numVertices - gridRows - 2)
			{
				meshVertices[i].z += meshVertices[i - 1].z + meshVertices[i + 1].z;
				meshVertices[i].z *= 0.3f;
			}
			else
			{
				meshVertices[i].z += (meshVertices[i + gridRows - 1].z + meshVertices[i + gridRows + 1].z) * 2.0f;
				meshVertices[i].z *= rippleDropOffMult;
			}
		}

		mainMesh.vertices = meshVertices;
		
		// Scroll the texture
		scrollOffset += scrollSpeed * Time.deltaTime;
		
		// Keep it wrapped within the texture's size (v high values go crazy on some plaforms, eg. iOS)
		if (scrollOffset.x < 0)
			scrollOffset.x += scrollWrapAmounts.x;
		else if (scrollOffset.x >= scrollWrapAmounts.x)
			scrollOffset.x -= scrollWrapAmounts.x;

		if (scrollOffset.y < 0)
			scrollOffset.y += scrollWrapAmounts.y;
		else if (scrollOffset.y >= scrollWrapAmounts.y)
			scrollOffset.y -= scrollWrapAmounts.y;
		
		// Apply it to the material
		mainMaterial.mainTextureOffset = scrollOffset;
	}
	
	/// <summary> Starts a new ripple </summary>
	/// <param name='xPos'> Starting posiiton </param>
	public void AddRipple(float xPos)
	{
		if (xPos < -0.5f)
		{
			meshVertices[2013].z -= rippleStrength;
			meshVertices[2014].z -= rippleStrength;
		}
		else if (xPos > 0.5f)
		{
			meshVertices[2015].z -= rippleStrength;
			meshVertices[2016].z -= rippleStrength;
		}
		else
		{
			meshVertices[2017].z -= rippleStrength;
			meshVertices[2018].z -= rippleStrength;
		}

		// Swap the ripple direction for the next update
		rippleStrength = -rippleStrength;
	}
}
