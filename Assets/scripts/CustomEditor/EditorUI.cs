using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorUI : SingletonComponent<EditorUI> {

	[SerializeField] Button ghostDirectionButton;
	Text ghostDirectionButtonText;

	void Start () {
		ghostDirectionButtonText = ghostDirectionButton.GetComponentInChildren<Text> ();

		ghostDirectionButton.onClick.AddListener (OnGhostDirectionButtonClick);
	}

	void OnGhostDirectionButtonClick () {
		EditorManager.Instance.ToggleGhostDirection ();
	}

	/* Events / Actions triggered by editor manager */

	public void SetNodeSelected (bool visible) {
		ghostDirectionButton.gameObject.SetActive (visible);
	}
}
