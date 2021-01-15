﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Character))]
public class PlayerCharacterController : MyCharacterController {
	GameObject[] AllEnemyCharacters;
	List<HexCell> allPossibleDest;

	// Start is called before the first frame update
	protected override void Start() {
		base.Start();
		AllEnemyCharacters = GameObject.FindGameObjectsWithTag("Enemy");
		allPossibleDest = new List<HexCell>();
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
				if (needToCalAllPossibleDestinations) {
					allPossibleDest = HexMap.hexMap.AllPossibleDestinationCells(charCurCell, curCharactor.actionPoints);
					needToCalAllPossibleDestinations = false;
					MapManager.SetHighlightCells(allPossibleDest, ETileHighlightType.MoveRange);
				}



				// left mouse click actions: 
				// 1. move to cell based on remaining AP
				// 2. If click on enemy, show enemy information
				if (leftMouseClicked) {
					HexCell clickCell = HexMap.hexMap.GetHexCellFromWorldPos(clickedUnityCellCenterWorldPos);
					// Check if the click cell is a cell that char can move to
					bool isClickCellMovable = false;
					foreach (HexCell destCell in allPossibleDest) {
						isClickCellMovable = (destCell.Equals(clickCell));
						if (isClickCellMovable) {
							break;
						}
					}

					bool isClickCellEnemy = false;
					// Check if the click cell has enemy character
					foreach (GameObject enemyGO in AllEnemyCharacters) {
						Vector3 curEnemyPos = enemyGO.transform.position;
						HexCell curEnemyCell = HexMap.hexMap.GetHexCellFromWorldPos(curEnemyPos);
						if (curEnemyCell.Equals(clickCell)) {
							isClickCellEnemy = true;
							break;
						}
					}

					if (isClickCellEnemy) {
						// TODO: Show enemy information
					} else if (isClickCellMovable) {
						// If player can move there and it's not occupied by enemy, switch to moving state and move there
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
			case ECharacterActionState.Attacking:
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
