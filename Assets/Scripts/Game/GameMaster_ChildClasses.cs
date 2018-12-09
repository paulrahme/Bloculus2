using UnityEngine;

public partial class GameMaster : MonoBehaviour
{
	[System.Serializable]
	public class ViewLayout
	{
		[Header("Tower")]
		public Vector3[] towerPositions = { Vector3.zero };

		[Header("Camera")]
		public Vector3 cameraPos = new Vector3(0f, 0f, -10f);
		public float cameraFoV = 60;

		[Header("Environment")]
		public Vector3 groundPos = new Vector3(0f, -8f, 40f);
		public Vector3 groundScale = new Vector3(48f, 48f, 48f);
	}
}
