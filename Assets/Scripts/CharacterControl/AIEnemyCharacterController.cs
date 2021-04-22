using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Character))]
public class AIEnemyCharacterController : MyCharacterController {
	// Use this variable to control how long it will wait until switch to new state
	// Primarily used for the animation to have time to play
	[SerializeField]
	private float waitSecsThenTransit = 0.3f;

	// Start is called before the first frame update
	protected override void Start() {
		base.Start();
		if (GameManager.GetInstance.GetAllPlayerControlCharacters().Count > 0) {
			curCharactor.SetCurTargetCharacter(GameManager.GetInstance.GetAllPlayerControlCharacters()[0].GetComponent<Character>());
		}
	}

	protected override void Update() {
		base.Update();

		switch (GameManager.GetInstance.gameState) {
			case EGameState.AIEnemyTurn:
				if (waitInCoroutine) {
					break;
				}
				AIEnemyCheckCharacterStates();
				break;
			default:
				break;
		}
	}

	void AIEnemyCheckCharacterStates() {
		// (TODO): Choose the appropriate target char
		curCharactor.SetCurTargetCharacter(GameManager.GetInstance.GetAllPlayerControlCharacters()[0].GetComponent<Character>());

		// Get the target world position
		HexCell curTargetCell = curCharactor.curTargetCharacter.charCurHexCell;

		// Get the current character world position and hex cell
		Vector3 curPos = transform.position;
		HexCell curCell = HexMap.hexMap.GetHexCellFromWorldPos(curPos);

		switch (curCharactor.characterCurrentActionState) {
			case ECharacterActionState.InActive:
				break;
			case ECharacterActionState.Idle:
				// If still have action points, move towards it's target
				if (curCharactor.charStatus.actionPoints > 0 && !curCharactor.attackDone) {
					// Make enemy facing the target cell
					curCharactor.SetCharacterFacingDirection(curTargetCell);
					// Get a path from cur pos to target pos
					int totalPathCost = Int32.MaxValue;
					charNavigation.ComputePath(curCell, curTargetCell, ref totalPathCost);
					StartCoroutine(WaitSecondsThenTransitTo(waitSecsThenTransit, ECharacterActionState.Moving));
				} else {
					// If no action points available, for now just end the turn
					curCharactor.hasFinishedThisTurn = true;
				}

				break;
			case ECharacterActionState.Moving:
				// If path is complete or we can attack directly
				if (charNavigation.IsPathComplete() || curCharactor.IsTargetAttackable(curTargetCell)) {
					curCharactor.SwitchActionStateTo(ECharacterActionState.Attacking);
				}
				else {
					// If we are not moving, command the moving coroutine
					if (!isMoving) {
						StartCoroutine(MovingCoroutine());
					}
				}
				break;
			case ECharacterActionState.Attacking:
				if (curCharactor.IsTargetAttackable(curTargetCell)) {
					if (!curCharactor.hasStartAttack) {
						curCharactor.PerformAttack();
					}

					if (curCharactor.attackDone) {
						// When finish attack, switch back to Idle
						curCharactor.SwitchActionStateTo(ECharacterActionState.Idle);
					}

				} else {
					// If not within attack range, directly end the attacking state and switch to next (currently in active)
					curCharactor.SwitchActionStateTo(ECharacterActionState.InActive);
					curCharactor.hasFinishedThisTurn = true;
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
			HexCell curTargetCell = curCharactor.curTargetCharacter.charCurHexCell;
			if (curCharactor.charStatus.actionPoints >= nextMoveCost && !curCharactor.IsTargetAttackable(curTargetCell)) {
				charNavigation.MoveOneStep(true);
				curCharactor.charStatus.SetAP(curCharactor.charStatus.actionPoints - nextMoveCost);
				yield return new WaitForSeconds(curCharactor.movingIntervalSec);
			} else {
				break;
			}
		}
		curCharactor.SwitchActionStateTo(ECharacterActionState.Attacking);
		isMoving = false;
	}
}
