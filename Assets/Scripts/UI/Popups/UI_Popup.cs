using UnityEngine;
using UnityEngine.UI;
using System;

public class UI_Popup : MonoBehaviour
{
	#region Inspector variables

	[Header("ID")]
	public UI_PopupManager.PopupTypes popupType = UI_PopupManager.PopupTypes.Default;

	[Header("Hierarchy")]
	[SerializeField] Text title = null;
	[SerializeField] Text messageBody = null;
	[SerializeField] Text confirmLabel = null;
	[SerializeField] Text cancelLabel = null;

	#endregion // Inspector variables

	Action confirmCallback, cancelCallback;
	internal UI_PopupManager parentManager;

	/// <summary> Populates and enables the popup </summary>
	/// <param name="_popupInfo"> Contents </param>
	public void Show(UI_PopupManager.PopupInfo _popupInfo)
	{
		title.text = _popupInfo.title;
		messageBody.text = _popupInfo.messageBody;
		confirmLabel.text = _popupInfo.confirmText;
		cancelLabel.text = _popupInfo.cancelText;
		confirmCallback = _popupInfo.confirmCallback;
		cancelCallback = _popupInfo.cancelCalback;

		gameObject.SetActive(true);
	}

	/// <summary> Disables the popup </summary>
	public void Hide()
	{
		gameObject.SetActive(false);
		parentManager.PopupDismissed();
	}

	/// <summary> Called from Button's OnClick event </summary>
	public void OnConfirm()
	{
		if (confirmCallback != null)
			confirmCallback();

		Hide();
	}

	/// <summary> Called from Button's OnClick event </summary>
	public void OnCancel()
	{
		if (cancelCallback != null)
			cancelCallback();

		Hide();
	}
}
