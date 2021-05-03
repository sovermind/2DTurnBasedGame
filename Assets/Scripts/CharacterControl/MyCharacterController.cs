using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


/// <summary>
/// This is the base class for all character controller.
/// </summary>
[RequireComponent(typeof(Character))]
public class MyCharacterController : MonoBehaviour {

	public Character curCharactor;
	public GameObject characterDetailInfoPanel;       // UI panel which shows character's detail information
	protected Camera mainCam;
	protected NavigationManager charNavigation;       // character navigation manager
	protected List<HexCell> curPath;                  // current path ready to be execute
	protected bool isMoving;
	protected GameObject mapGridGO;
	protected Grid mapGrid;
	protected bool waitInCoroutine;

	protected virtual void Start() {
		mainCam = Camera.main;
		mapGridGO = GameObject.Find("WorldMapGrid");
		mapGrid = new Grid();
		if (mapGridGO != null) {
			mapGrid = mapGridGO.GetComponent<Grid>();
		}
		curCharactor = GetComponent<Character>();
		charNavigation = new NavigationManager();
		isMoving = false;
		waitInCoroutine = false;
		characterDetailInfoPanel.SetActive(false);
	}

	protected virtual void Update() {
		Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
		if (CommonUtil.IsPosInsideBound2D(mouseWorldPos, curCharactor.charSpriteRenderer.bounds)) {
			// Keep updating the info based on character's status
			UpdateCharacterInfoPanel();
			if (!characterDetailInfoPanel.activeSelf) {
				characterDetailInfoPanel.SetActive(true);
			}
		} else {
			if (characterDetailInfoPanel.activeSelf) {
				characterDetailInfoPanel.SetActive(false);
			}
		}

		// Show attack range if character is in attack mode
		if (curCharactor.characterCurrentActionState == ECharacterActionState.Attacking) {
			HighlightAttackRangeCells();
		} 
		// Show move range cells when the character in idle
		if (curCharactor.characterCurrentActionState == ECharacterActionState.Idle) {
			HighlightMovingRangeCells();
		}	
	}

	public void ControllerEndThisTurn() {
		curCharactor.EndThisTurn();
	}

	protected void HighlightAttackRangeCells() {
		List<HexCell> allAttackableCells = curCharactor.GetAllAttackableCells();
		MapManager.SetHighlightCells(allAttackableCells, ETileHighlightType.AttackRange);
	}

	protected void HighlightMovingRangeCells() {
		List<HexCell> allMovingRangeCells = curCharactor.GetAllMoveableCells();
		MapManager.SetHighlightCells(allMovingRangeCells, ETileHighlightType.MoveRange);
	}

	/// <summary>
	/// Use this Coroutine method to wait for some time then switch to new state
	/// Primarily because animation need time to play
	/// </summary>
	/// <param name="tSec"></param>
	/// <param name="newState"></param>
	/// <returns></returns>
	protected IEnumerator WaitSecondsThenTransitTo(float tSec, ECharacterActionState newState) {
		waitInCoroutine = true;
		yield return new WaitForSeconds(tSec);
		waitInCoroutine = false;
		curCharactor.SwitchActionStateTo(newState);
	}

	private void UpdateCharacterInfoPanel() {
		CharacterStatus charStatus = curCharactor.charStatus;

		// Level
		curCharactor.infoPanelController.SetLevelDisplay(charStatus.charLevel, charStatus.maxLevel);

		// Exp.
		curCharactor.infoPanelController.SetExperienceDisplay(charStatus.currentXP, charStatus.nextLevelRequiredXP[charStatus.charLevel]);

		// Attack, defend
		curCharactor.infoPanelController.SetAttackDisplay(charStatus.attack);
		curCharactor.infoPanelController.SetDefendDisplay(charStatus.defend);
	}
}
