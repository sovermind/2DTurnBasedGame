using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Character))]
public class AIEnemyCharacterController : MyCharacterController {

	GameObject[] PlayerControlCharacters;

	// Start is called before the first frame update
	protected override void Start() {
		base.Start();
		PlayerControlCharacters = GameObject.FindGameObjectsWithTag("Player");
	}

	protected override void Update() {
		base.Update();

		switch (GameManager.GetInstance.gameState) {
			case EGameState.AIEnemyTurn:
				AIEnemyCheckCharacterStates();
				break;
			default:
				break;
		}

	}

	void AIEnemyCheckCharacterStates() {

		switch (curCharactor.characterCurrentActionState) {
			case ECharacterActionState.InActive:
				curCharactor.SwitchActionStateTo(ECharacterActionState.Idle);
				break;
			case ECharacterActionState.Idle:
				// If still have action points, move towards it's target
				if (curCharactor.actionPoints > 0) {
					GameObject curTargetChar = PlayerControlCharacters[0];
					Vector3 curTargetCharPos = curTargetChar.transform.position;
					Vector3 curPos = transform.position;

					HexCell curCell = HexMap.hexMap.GetHexCellFromWorldPos(curPos);
					HexCell curTargetCell = HexMap.hexMap.GetHexCellFromWorldPos(curTargetCharPos);

					// Highlight the place that the character is able to move
					List<HexCell> allPossibleDest = new List<HexCell>();
					if (needToCalAllPossibleDestinations) {
						allPossibleDest = HexMap.hexMap.AllPossibleDestinationCells(curCell, curCharactor.actionPoints);
						needToCalAllPossibleDestinations = false;
						MapManager.SetHighlightCells(allPossibleDest, ETileHighlightType.MoveRange);
					}					

					// Get a path from cur pos to target pos
					int totalPathCost = Int32.MaxValue;
					charNavigation.ComputePath(curCell, curTargetCell, ref totalPathCost);
					curCharactor.SwitchActionStateTo(ECharacterActionState.Moving);
				} else {
					// If no action points available, for now just end the turn
					//curCharactor.SwitchActionStateTo(ECharacterActionState.InActive);
					//curCharactor.EndThisTurn();
					curCharactor.hasFinishedThisTurn = true;
				}

				break;
			case ECharacterActionState.Moving:
				if (charNavigation.IsPathComplete()) {
					curCharactor.SwitchActionStateTo(ECharacterActionState.Idle);
					Debug.Log("No active valid path, set character back to Idle");
				}
				else {
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
			HexCell nextCell = charNavigation.GetNextCellInPath();
			int nextMoveCost = MapManager.GetTileCostFromHexCell(nextCell);
			gameObject.transform.position = charNavigation.GetCurPathCellWorldPos();
			if (curCharactor.actionPoints >= nextMoveCost) {
				charNavigation.MoveOneStep(true);
				curCharactor.actionPoints = curCharactor.actionPoints - nextMoveCost;
				yield return new WaitForSeconds(curCharactor.movingIntervalSec);
			} else {
				break;
			}
		}
		curCharactor.SwitchActionStateTo(ECharacterActionState.Idle);
		isMoving = false;
	}
}
