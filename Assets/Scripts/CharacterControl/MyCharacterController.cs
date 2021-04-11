using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the base class for all character controller.
/// </summary>
[RequireComponent(typeof(Character))]
public class MyCharacterController : MonoBehaviour {

	public Character curCharactor;
	public GameObject characterDetailInfoPanel;
	protected Camera mainCam;
	protected NavigationManager charNavigation;       // character navigation manager
	protected List<HexCell> curPath;                  // current path ready to be execute
	protected bool isMoving;
	protected GameObject mapGridGO;
	protected Grid mapGrid;
	protected bool needToCalAllPossibleDestinations;
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
		needToCalAllPossibleDestinations = true;
		waitInCoroutine = false;
		characterDetailInfoPanel.SetActive(false);
	}

	protected virtual void Update() {
		Vector3 mouseClickWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
		if (CommonUtil.IsPosInsideBound2D(mouseClickWorldPos, curCharactor.charSpriteRenderer.bounds)) {
			//HighlightAttackRangeCells(true);
			characterDetailInfoPanel.SetActive(true);
		} else {
			//HighlightAttackRangeCells(false);
			characterDetailInfoPanel.SetActive(false);			
		}
	}

	public void ControllerEndThisTurn() {
		needToCalAllPossibleDestinations = true;
		curCharactor.EndThisTurn();
	}

	protected void HighlightAttackRangeCells(bool on) {
		// Highlight the attacking range
		List<HexCell> allAttackableCells = curCharactor.GetAllAttackableCells();
		if (on) {
			MapManager.SetHighlightCells(allAttackableCells, ETileHighlightType.AttackRange);
		} else {
			MapManager.ClearHighlightedCells(allAttackableCells, ETileHighlightType.AttackRange);
		}
		
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
}
