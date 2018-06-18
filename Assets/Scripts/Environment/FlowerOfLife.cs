using UnityEngine;
using System.Collections.Generic;

public class FlowerOfLife : MonoBehaviour
{
	# region Inspector variables

	public float	pulseStartAlpha = 0.5f;						// Transparency for pulses to start at
	public float	pulseDisappearSpeed = 0.3f;					// How long a pulse disappears (in units per second)
	
	#endregion	// Inspector variables

	int				gactiveMaterials;							// Number of materials currently pulse-able
	List<Material>	pulsingMaterials = new List<Material>();	// Currently pulsing objects
	Material[]		ringMaterials;								// Array of materials
	List<Material>	materialsToDelete = new List<Material>();	// Materials to delete this frame

	// Inline/helper functions
	public void		SetMaxActiveMaterials(int max) { gactiveMaterials = Mathf.Min(max, ringMaterials.Length); }
	
	/// <summary> Called when object/script activates </summary>
	void Awake()
	{
		MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
		ringMaterials = new Material[meshRenderers.Length];
		for (int i = 0; i < ringMaterials.Length; ++i)
		{
			ringMaterials[i] = meshRenderers[i].material;
			ringMaterials[i].color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
		}
	}

	/// <summary> Called once per frame </summary>
	void Update()
	{
		materialsToDelete.Clear();
		float pulseFadeAmount = pulseStartAlpha * pulseDisappearSpeed * Time.deltaTime;

		// Process pulsing materials
		for (int i = 0; i < pulsingMaterials.Count; ++i)
		{
			Material material = pulsingMaterials[i];
			Color matColor = material.color;
			matColor.a -= pulseFadeAmount;
			if (matColor.a < 0.0f)
			{
				material.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
				materialsToDelete.Add(material);
			}
			else
				material.color = matColor;
		}

		// Remove any materials finished pulsing from the list
		for (int i = 0; i < materialsToDelete.Count; ++i)
			pulsingMaterials.Remove(materialsToDelete[i]);
	}
	
	/// <summary> Starts a random ring pulsing </summary>
	/// <param name='_color'> Colour to pulse </param>
	public void StartPulse(Color _color)
	{
		Material material = ringMaterials[Tower.gInstance.randomGen.Next(gactiveMaterials)];
		material.color = new Color(_color.r, _color.g, _color.b, pulseStartAlpha);
		if (!pulsingMaterials.Contains(material))
			pulsingMaterials.Add(material);
	}
}
