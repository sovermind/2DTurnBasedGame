using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Character))]
public class PlayerCharacterController : MyCharacterController {

	// Start is called before the first frame update
	protected override void Start() {
		base.Start();
	}

    // Update is called once per frame
    protected override void Update() {
		base.Update();

		switch (GameManager.GetInstance.gameState) {
			case EGameState.PlayerTurn:
				PlayerCheckCharacterStates();
				break;
			default:
				break;
		}

	}

	void PlayerCheckCharacterStates() {
		// Grab the position of mouse
		Vector3 mouseClickWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
		Vector3Int clickCellInUnity = mapGrid.WorldToCell(mouseClickWorldPos);
		Vector3 clickedUnityCellCenterWorldPos = mapGrid.CellToWorld(clickCellInUnity);

		bool leftMouseClicked = Input.GetMouseButtonUp(0);

		// Character's current cell
		HexCell charCurCell = curCharactor.charCurHexCell;

		// Switch through all different states possible
		switch (curCharactor.characterCurrentActionState) {
			case ECharacterActionState.InActive:
				if (leftMouseClicked) {
					if (IsInsideBound2D(mouseClickWorldPos, charSpriteRenderer.bounds)) {
						curCharactor.SwitchActionStateTo(ECharacterActionState.Idle);
					}
				}

				break;
			case ECharacterActionState.Idle:
				// Highlight the place that the character is able to move
				List<HexCell> allPossibleDest = new List<HexCell>();
				if (needToCalAllPossibleDestinations) {
					allPossibleDest = HexMap.hexMap.AllPossibleDestinationCells(charCurCell, curCharactor.actionPoints);
					needToCalAllPossibleDestinations = false;
					MapManager.SetHighlightCells(allPossibleDest, ETileHighlightType.MoveRange);
				}



				// If left mouse clicked, we will move to position if possible
				// set up the destination point and send to navigation manager
				if (leftMouseClicked) {
					HexCell clickCell = HexMap.hexMap.GetHexCellFromWorldPos(clickedUnityCellCenterWorldPos);
					int totalPathCost = Int32.MaxValue;
					charNavigation.ComputePath(charCurCell, clickCell, ref totalPathCost);
					Debug.Log("MyCharacterController: cur path total cost: " + totalPathCost + ", cur ap: " + curCharactor.actionPoints);
					// If the current action points is still sufficient for the total cost of path, move to place
					if (curCharactor.actionPoints >= totalPathCost) {
						curCharactor.actionPoints = curCharactor.actionPoints - totalPathCost;
						needToCalAllPossibleDestinations = true;
						curCharactor.SwitchActionStateTo(ECharacterActionState.Moving);
					}
				}

				break;
			case ECharacterActionState.Moving:
				if (charNavigation.IsPathComplete()) {
					curCharactor.SwitchActionStateTo(ECharacterActionState.Idle);
					Debug.Log("No active valid path, set character back to Idle");
				} else {
					// If we are not moving, command the moving coroutine
					if (!isMoving) {
						StartCoroutine(MovingCoroutine());
					}
				}
				break;
			default:
				break;
		}

	}

	IEnumerator MovingCoroutine() {
		isMoving = true;
		while (!charNavigation.IsPathComplete()) {
			gameObject.transform.position = charNavigation.GetCurPathCellWorldPos();
			charNavigation.MoveOneStep(true);
			yield return new WaitForSeconds(curCharactor.movingIntervalSec);
		}
		curCharactor.SwitchActionStateTo(ECharacterActionState.Idle);
		isMoving = false;
	}
}
