using UnityEngine;
using UnityEngine.UI;
using System;

public class UI_Popup : MonoBehaviour
{
	#region Inspector variables

	[Header("ID")]
	public PopupManager.PopupTypes popupType = PopupManager.PopupTypes.Default;

	[Header("Hierarchy")]
	[SerializeField] Text title = null;
	[SerializeField] Text messageBody = null;
	[SerializeField] Text confirmLabel = null;
	[SerializeField] Text cancelLabel = null;

	#endregion // Inspector variables

	Action confirmCallback, cancelCallback;
	internal PopupManager parentManager;
	Transform myTrans;

	/// <summary> Populates and enables the popup </summary>
	/// <param name="_popupInfo"> Contents </param>
	public void StartShowing(PopupManager.PopupInfo _popupInfo)
	{
		if (myTrans == null)
			myTrans = transform;

		title.text = _popupInfo.title;
		messageBody.text = _popupInfo.messageBody;
		confirmLabel.text = _popupInfo.confirmText;
		cancelLabel.text = _popupInfo.cancelText;
		confirmCallback = _popupInfo.confirmCallback;
		cancelCallback = _popupInfo.cancelCalback;

		UpdateAnim(0f);
		gameObject.SetActive(true);
	}

	/// <summary> Updates the animation - called from the PopupManager's Update() </summary>
	/// <param name="_scale"> Transform's scale </param>
	public void UpdateAnim(float _scale)
	{
		myTrans.localScale = Helpers.vec3One * _scale;
	}

	/// <summary> Called from Button's OnClick event </summary>
	public void OnConfirm()
	{
		parentManager.PopupDismissed(confirmCallback);
	}

	/// <summary> Called from Button's OnClick event </summary>
	public void OnCancel()
	{
		parentManager.PopupDismissed(cancelCallback);
	}
}
