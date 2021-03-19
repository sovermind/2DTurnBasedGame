using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextPopup : MonoBehaviour {

	/// <summary>
	/// Create a text pop up at desired position with a given text wants to show up
	/// The pop up will disappear after certain time
	/// </summary>
	public static void Create(Vector3 pos, string popupText) {
		// https://www.youtube.com/watch?v=iD1_JczQcFY
		GameObject thePopup = Instantiate(GameManager.GetInstance.textPopupGO, pos, Quaternion.identity);
		TextMeshPro textMesh = thePopup.GetComponent<TextMeshPro>();
		textMesh.SetText(popupText);
	}

	void Update() {
        
    }
}
