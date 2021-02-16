using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIDisplayController : MonoBehaviour {

	// The UI panel which contain the turn message
	private GameObject turnMessagePanel;
	// The turn message TMP
	private GameObject turnMessage;
	// The TMP UGUI component in TMP
	private static TextMeshProUGUI turnMessageTMP;

	private static bool isSwitchingState = false;
	private Animator switchTurnAnimator;

	private void Start() {
		turnMessagePanel = GameObject.Find("TurnMessagePanel");
		turnMessage = turnMessagePanel.transform.GetChild(0).gameObject;
		turnMessageTMP = turnMessage.GetComponent<TextMeshProUGUI>();

		switchTurnAnimator = turnMessage.GetComponent<Animator>();
	}

	private void Update() {
		//turnMessageTMP.SetText("some random text");
		if (isSwitchingState) {
			StartCoroutine(WaitForSwitchTurnAnimation());
		}
	}

	public static bool SwitchTurnTo(EGameState newState) {
		if (turnMessageTMP == null) {
			return false;
		}

		if (newState == EGameState.PlayerTurn) {
			turnMessageTMP.SetText("Your Turn");
		}
		else if (newState == EGameState.AIEnemyTurn) {
			turnMessageTMP.SetText("Enemy's Turn");
		}

		isSwitchingState = true;

		return true;
	}

	IEnumerator WaitForSwitchTurnAnimation() {
		Time.timeScale = 0f;
		isSwitchingState = false;
		Debug.Log("set trigger to play animation");
		switchTurnAnimator.SetTrigger("SwitchTurn");
		yield return new WaitForSecondsRealtime(0.8f);
		
		Time.timeScale = 1f;
	}
}
