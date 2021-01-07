using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the base class for all character controller.
/// </summary>
public class MyCharacterController : MonoBehaviour {

	public Character curCharactor;
	protected Camera mainCam;
	protected SpriteRenderer charSpriteRenderer;      // character sprite render
	protected NavigationManager charNavigation;       // character navigation manager
	protected List<HexCell> curPath;                  // current path ready to be execute
	protected bool isMoving;
	protected GameObject mapGridGO;
	protected Grid mapGrid;
	protected bool needToCalAllPossibleDestinations;

	protected virtual void Start() {
		mainCam = Camera.main;
		mapGridGO = GameObject.Find("WorldMapGrid");
		mapGrid = new Grid();
		if (mapGridGO != null) {
			mapGrid = mapGridGO.GetComponent<Grid>();
		}
		curCharactor = GetComponent<Character>();
		charSpriteRenderer = GetComponent<SpriteRenderer>();
		charNavigation = new NavigationManager();
		isMoving = false;
		needToCalAllPossibleDestinations = true;
	}

	protected virtual void Update() {
		Vector3 mouseClickWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
		if (IsInsideBound2D(mouseClickWorldPos, charSpriteRenderer.bounds)) {
			HighlightAttackRangeCells(true);
		} else {
			HighlightAttackRangeCells(false);
		}
	}

	/// <summary>
	/// Check if the given point is within the bound
	/// </summary>
	/// <param name="checkPoint">World position of the point we want to check</param>
	/// <param name="bound">The bound that is checked against. Also in world frame</param>
	/// <returns></returns>
	protected bool IsInsideBound2D(Vector3 checkPoint, Bounds bound) {
		if (checkPoint.x >= bound.min.x && checkPoint.x < bound.max.x
			&& checkPoint.y >= bound.min.y && checkPoint.y < bound.max.y) {
			return true;
		}
		return false;
	}

	public void ControllerEndThisTurn() {
		needToCalAllPossibleDestinations = true;
		curCharactor.EndThisTurn();
	}

	protected void HighlightAttackRangeCells(bool on) {
		// Highlight the attacking range
		List<HexCell> allAttackableCells = new List<HexCell>();
		allAttackableCells = HexMap.hexMap.AllCellsWithinRadius(curCharactor.charCurHexCell, curCharactor.attackRangeRadius);
		if (on) {
			MapManager.SetHighlightCells(allAttackableCells, ETileHighlightType.AttackRange);
		} else {
			MapManager.ClearHighlightedCells(allAttackableCells, ETileHighlightType.AttackRange);
		}
		
	}
}
