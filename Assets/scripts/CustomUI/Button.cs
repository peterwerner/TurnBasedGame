using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomUI {

	public class Button : UnityEngine.UI.Button {

		public delegate void Callback ();
		public delegate bool CallbackBool ();

		RectTransform rect;
		float currScale = 0, targetScale = 0; // normalized scale
		Vector3 currPos = Vector3.zero, targetPos = Vector3.zero; // normalized position
		bool destroyOnHidden = false;
		bool pressed = false;
		Callback onPress, onCancel;
		CallbackBool isUseable;

		public void Show (Callback onPress, Callback onCancel, CallbackBool isUseable) {
			targetScale = 1;
			this.onPress = onPress;
			this.onCancel = onCancel;
			this.isUseable = isUseable;
		}

		public void Hide (bool destroyOnHidden = true) {
			targetScale = 0;
			this.destroyOnHidden = destroyOnHidden;
		}

		public void Cancel () {
		}

		// Sets the position in normalized coordinates
		public void SetPosition (Vector3 targetPos, bool immediate = false) {
			this.targetPos = targetPos;
			if (immediate) {
				currPos = targetPos;
			}
		}

		protected override void Awake () {
			rect = GetComponent<RectTransform> ();
			rect.localPosition = currPos * rect.rect.width;
			rect.localScale = currScale * Vector3.one;
		}

		void Update () {
			if (!pressed && IsPressed()) {
				pressed = true;
				onPress ();
			}
			pressed = IsPressed();

			interactable = isUseable();

			currScale = Mathf.MoveTowards (currScale, targetScale, Time.deltaTime / colors.fadeDuration);
			rect.localScale = currScale * Vector3.one;
			currPos = Vector3.MoveTowards (currPos, targetPos, Time.deltaTime / colors.fadeDuration);
			rect.localPosition = currPos * rect.rect.width;
			// Optionally destroy this object if fully hidden
			if (destroyOnHidden && Mathf.Approximately (currScale, 0) && Mathf.Approximately(targetScale, 0)) {
				Destroy (gameObject);
			}
		}
	}

}