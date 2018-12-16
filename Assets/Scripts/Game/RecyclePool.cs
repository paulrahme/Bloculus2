using System.Collections.Generic;
using UnityEngine;

public class RecyclePool
{
	public enum PoolTypes
	{
		BlockDisappear,
		FallingRing,
		Shockwave,
	}

	static Stack<GameObject>[] pools;

	/// <summary> Gets the specified recycle pool </summary>
	/// <param name="_type"> Pool type from enum </param>
	/// <returns> The requested Stack </returns>
	static Stack<GameObject> GetStack(PoolTypes _type)
	{
		// First time? Create all stacks
		if (pools == null)
		{
			int poolCount = System.Enum.GetValues(typeof(PoolTypes)).Length;
			pools = new Stack<GameObject>[poolCount];
			for (int type = 0; type < poolCount; ++type)
				pools[type] = new Stack<GameObject>();
		}

		return pools[(int)_type];
	}

	/// <summary> Recycle a GameObject to its pool </summary>
	/// <param name="_type"> Pool type </param>
	/// <param name="_gameObj"> GameObject to recycle </param>
	public static void Recycle(PoolTypes _type, GameObject _gameObj)
	{
		_gameObj.SetActive(false);
		GetStack(_type).Push(_gameObj);
	}

	/// <summary> Retrieves (if found), else Instantiates, the specified GameObject </summary>
	/// <param name="_type"> Pool type </param>
	/// <param name="_prefab"> Prefab to Instantiate from </param>
	/// <returns> The GameObject retrieved or created </returns>
	public static GameObject RetrieveOrCreate(PoolTypes _type, GameObject _prefab)
	{
		// Pop from stack if non-empty, else instantiate
		GameObject gameObj;
		Stack<GameObject> stack = GetStack(_type);
		if (stack.Count != 0)
			gameObj = stack.Pop();
		else
			gameObj = Object.Instantiate(_prefab);

		gameObj.SetActive(true);

		return gameObj;
	}

	/// <summary> Clears all recyce pools, destroying all recycled objects </summary>
	public static void ClearAllPools()
	{
		if (pools == null)
			return;
		
		for (int i = 0; i < pools.Length; ++i)
		{
			Stack<GameObject> pool = pools[i];
			while (pool.Count > 0)
				Object.Destroy(pool.Pop());
			pool.Clear();
		}
	}
}
