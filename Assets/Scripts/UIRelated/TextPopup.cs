using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Idea comes from this youtube video on popup text: https://www.youtube.com/watch?v=iD1_JczQcFY

public class TextPopup : MonoBehaviour {
	[SerializeField]
	private float yOffset;

	[SerializeField]
	private float moveUpYSpeed;

	[SerializeField]
	private float moveUpYDist;

	[SerializeField]
	private float fadedSpeed;

	[SerializeField]
	private float shrinkRatioPerFrame;

	public float textSizeIncreaseRatio = 1.0f;

	/// <summary>
	/// Create a text pop up at desired position with a given text wants to show up
	/// The pop up will disappear after certain time
	/// </summary>
	public static void Create(Vector3 pos, string popupText, float increaseSizeRatio) {
		GameObject thePopup = Instantiate(GameManager.GetInstance.textPopupGO, pos, Quaternion.identity);
		TextMeshPro textMesh = thePopup.GetComponent<TextMeshPro>();
		textMesh.SetText(popupText);
		TextPopup txtPop = thePopup.GetComponent<TextPopup>();
		txtPop.textSizeIncreaseRatio = increaseSizeRatio;
	}

	TextMeshPro textMesh;
	Vector3 originPos;
	bool isMovingUp;

	private void Awake() {
		textMesh = transform.GetComponent<TextMeshPro>();
		originPos = transform.position;
		Vector3 spawnPos = new Vector3(originPos.x, originPos.y + yOffset);
		transform.position = spawnPos;
		originPos = spawnPos;

		isMovingUp = true;
	}

	void Update() {
		// The pop up should show up and then disappear gradually
		if (transform.position.y - originPos.y <= moveUpYDist && isMovingUp) {
			transform.position += new Vector3(0, moveUpYSpeed) * Time.deltaTime;
			// Increase the size by certain ratio
			transform.localScale = transform.localScale * textSizeIncreaseRatio;
			if (transform.position.y - originPos.y >= moveUpYDist) {
				isMovingUp = false;
			}
		} else if (!isMovingUp) {
			// Shrink size per frame
			transform.localScale = transform.localScale * shrinkRatioPerFrame;

			//  and also faded out
			Color curColor = textMesh.color;
			curColor.a -= fadedSpeed * Time.deltaTime;
			textMesh.color = curColor;
			if (curColor.a < 0) {
				Destroy(gameObject);
			}
		}
		
    }
}
