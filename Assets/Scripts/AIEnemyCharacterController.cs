using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Character))]
public class AIEnemyCharacterController : MyCharacterController {

	GameObject[] PlayerControlCharacters;
	GameObject curTargetCharGO;

	// Start is called before the first frame update
	protected override void Start() {
		base.Start();
		PlayerControlCharacters = GameObject.FindGameObjectsWithTag("Player");
		curTargetCharGO = new GameObject();
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
		// (TODO): Choose the appropriate target char
		curTargetCharGO = PlayerControlCharacters[0];

		// Get the target world position
		Vector3 curTargetCharPos = curTargetCharGO.transform.position;
		HexCell curTargetCell = HexMap.hexMap.GetHexCellFromWorldPos(curTargetCharPos);

		// Get the current character world position and hex cell
		Vector3 curPos = transform.position;
		HexCell curCell = HexMap.hexMap.GetHexCellFromWorldPos(curPos);

		switch (curCharactor.characterCurrentActionState) {
			case ECharacterActionState.InActive:
				break;
			case ECharacterActionState.Idle:
				// If still have action points, move towards it's target
				if (curCharactor.actionPoints > 0) {
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
				// (TODO): perform attacking action
				Debug.Log("AI Enemy attacking!!!!");

				// When finish attack, enemy finishes this turn
				curCharactor.SwitchActionStateTo(ECharacterActionState.InActive);
				curCharactor.hasFinishedThisTurn = true;
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
			Debug.Log("next cell: " + nextCell.hexCellPos);
			gameObject.transform.position = charNavigation.GetCurPathCellWorldPos();
			HexCell curTargetCell = HexMap.hexMap.GetHexCellFromWorldPos(curTargetCharGO.transform.position);
			Debug.Log("curTargetCell: " + curTargetCell.hexCellPos + ", cur char cell: " + curCharactor.charCurHexCell.hexCellPos);
			if (curCharactor.actionPoints >= nextMoveCost && !curCharactor.IsTargetAttackable(curTargetCell)) {
				Debug.Log("Move one step forward");
				charNavigation.MoveOneStep(true);
				curCharactor.actionPoints = curCharactor.actionPoints - nextMoveCost;
				yield return new WaitForSeconds(curCharactor.movingIntervalSec);
			} else {
				break;
			}
		}
		// Heres the problem!!!!
		curCharactor.SwitchActionStateTo(ECharacterActionState.Attacking);
		isMoving = false;
	}
}
