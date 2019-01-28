using UnityEngine;

public class BlockDisappear : MonoBehaviour
{
	#region Inspector variables

	public float fadeTime = 0.25f;

	#endregion    // Inspector variables

	Color color;
	Material material;

	/// <summary> Creates (or reuses) a disappearing block GameObject </summary>
	/// <param name="_block"> Block from which to create </param>
	public static void StartDisappearing(Block _block, GameObject _disappearPrefab)
	{
		GameObject gameObj = RecyclePool.RetrieveOrCreate(RecyclePool.PoolTypes.BlockDisappear, _disappearPrefab);

		// Match pos/rot/scale of original block
		Transform trans = gameObj.transform;
		Transform blockTrans = _block.trans;
		trans.parent = blockTrans.parent;
		trans.localPosition = blockTrans.localPosition;
		trans.localRotation = blockTrans.localRotation;
		trans.localScale = blockTrans.localScale;

		// (Re)start the disappear anim
		gameObj.GetComponent<BlockDisappear>().ResetAnim();
	}
	
	/// <summary> Resets the disappear animation </summary>
	void ResetAnim()
	{
		color = Color.white;
	}

	/// <summary> Called when object/script activates </summary>
	void Awake()
	{
		material = GetComponent<Renderer>().material;
	}

	/// <summary> Called once per frame </summary>
	void Update()
	{
		color.a -= Time.deltaTime / fadeTime;
		if (color.a <= 0.0f)
			RecyclePool.Recycle(RecyclePool.PoolTypes.BlockDisappear, gameObject);
		else
			material.color = color;
	}	
}
