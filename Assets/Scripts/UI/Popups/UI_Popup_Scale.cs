using UnityEngine;

public class UI_Popup_Scale : UI_Popup
{
	#region Inspector variables

	[Header("Popup Animation")]
	[SerializeField] AnimationCurve showAnimCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1.1f), new Keyframe(0.8f, 0.95f), new Keyframe(1f, 1f));
	[SerializeField] AnimationCurve hideAnimCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.2f, 1.2f), new Keyframe(1f, 0f));

	#endregion // Inspector variables

	/// <summary> Updates the animation - called from the PopupManager's Update() </summary>
	/// <param name="_progress"> Anim progress </param>
	public override void UpdateShowAnim(float _progress)
	{
		myTrans.localScale = Helpers.vec3One * showAnimCurve.Evaluate(_progress);
	}

	/// <summary> Updates the animation - called from the PopupManager's Update() </summary>
	/// <param name="_progress"> Anim progress </param>
	public override void UpdateHideAnim(float _progress)
	{
		myTrans.localScale = Helpers.vec3One * hideAnimCurve.Evaluate(_progress);
	}
}
