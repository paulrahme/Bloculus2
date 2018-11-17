using UnityEngine;

public class UI_Popup_SlidePos : UI_Popup
{
	#region Inspector variables

	[Header("Popup Animation")]
	[SerializeField] AnimationCurve showAnimCurve = new AnimationCurve(new Keyframe(0f, 1f, -7f, -7f), new Keyframe(0.4f, 0f, -0.6f, -0.6f), new Keyframe(1f, 0f, 0.1f, 0.1f));
	[SerializeField] AnimationCurve hideAnimCurve = new AnimationCurve(new Keyframe(0f, 0f, -1f, -1f), new Keyframe(1f, 1f, 1f, 1f));

	[Header("SlidePos Specific")]
	[SerializeField] Vector2 slideDistance = new Vector2(0f, -2000f);

	#endregion // Inspector variables

	RectTransform myRectTrans;
	Vector2 origAnchoredPos;

	/// <summary> Populates and enables the popup </summary>
	/// <param name="_popupInfo"> Contents </param>
	public override void StartShowing(PopupManager.PopupInfo _popupInfo)
	{
		if (myRectTrans == null)
		{
			myRectTrans = GetComponent<RectTransform>();
			origAnchoredPos = myRectTrans.anchoredPosition;
		}

		base.StartShowing(_popupInfo);
	}

	/// <summary> Updates the animation - called from the PopupManager's Update() </summary>
	/// <param name="_progress"> Anim progress </param>
	public override void UpdateShowAnim(float _progress)
	{
		myRectTrans.anchoredPosition = origAnchoredPos + (slideDistance * showAnimCurve.Evaluate(_progress));
	}

	/// <summary> Updates the animation - called from the PopupManager's Update() </summary>
	/// <param name="_progress"> Anim progress </param>
	public override void UpdateHideAnim(float _progress)
	{
		myRectTrans.anchoredPosition = origAnchoredPos + (slideDistance * hideAnimCurve.Evaluate(_progress));
	}
}
