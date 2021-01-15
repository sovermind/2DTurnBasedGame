﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EGameState {
	PlayerTurn,
	AIEnemyTurn
}

public class GameManager : MonoBehaviour {
	private static GameManager _instance;

	private Camera mainCam;
	private Grid mapGrid;
	protected GameObject mapGridGO;

	GameObject[] PlayerControlCharacters;
	GameObject[] AIEnemyCharacters;

	public Button nextTurnButton;
	public Button attackButton;


	[SerializeField]
	private EGameState _gameState;

	public EGameState gameState {
		get {
			return _gameState;
		}
	}

	public void SetGameState(EGameState newState) {
		if (_gameState == newState) {
			return;
		}

		// Check from which state to which state
		// All enemy finished, give control back to player
		if (_gameState == EGameState.AIEnemyTurn && newState == EGameState.PlayerTurn) {
			foreach (GameObject curEnemyGO in AIEnemyCharacters) {
				AIEnemyCharacterController curEnemyCharacterController = curEnemyGO.GetComponent<AIEnemyCharacterController>();
				curEnemyCharacterController.ControllerEndThisTurn();
			}
		}

		// Player has finished, give control to AI enemy
		if (_gameState == EGameState.PlayerTurn && newState == EGameState.AIEnemyTurn) {
			foreach (GameObject curPlayerGO in PlayerControlCharacters) {
				PlayerCharacterController curPlayerCharController = curPlayerGO.GetComponent<PlayerCharacterController>();
				curPlayerCharController.ControllerEndThisTurn();
			}
		}

		// Switch state
		_gameState = newState;
	}

	public void Awake() {
		if (_instance == null) {
			_instance = this;
		}
		_gameState = EGameState.PlayerTurn;

		DontDestroyOnLoad(gameObject);
	}

	public static GameManager GetInstance {
		get {
			if (_instance == null) {
				GameObject go = new GameObject();
				_instance = go.AddComponent<GameManager>();
			}
			return _instance;
		}
	}

	private void Start() {
		mainCam = Camera.main;
		mapGridGO = GameObject.Find("WorldMapGrid");
		mapGrid = new Grid();
		if (mapGridGO != null) {
			mapGrid = mapGridGO.GetComponent<Grid>();
		}

		PlayerControlCharacters = GameObject.FindGameObjectsWithTag("Player");
		AIEnemyCharacters = GameObject.FindGameObjectsWithTag("Enemy");

		Button nextTurnBtn = nextTurnButton.GetComponent<Button>();
		Button attackBtn = attackButton.GetComponent<Button>();
		
		nextTurnBtn.onClick.AddListener(StartNextTurnButtonListener);
		attackBtn.onClick.AddListener(AttackButtonListener);
	}

	void Update() {
		switch (_gameState) {
			case EGameState.AIEnemyTurn:
				IssueCommandToEnemies();
				break;
			case EGameState.PlayerTurn:
				IssueCommandToPlayers();
				break;
			default:
				break;
		}
	}

	/// <summary>
	/// Check current all enemies's states, make sure only one enemy is active each time
	/// When all enemies finished action, end the current enemy turn, switch to player's turn
	/// </summary>
	public void IssueCommandToEnemies() {
		List<Character> waitForActiveEnemies = new List<Character>();
		foreach (GameObject AIEnemy in AIEnemyCharacters) {
			Character curEnemyCharacter = AIEnemy.GetComponent<Character>();
			if (!curEnemyCharacter.hasFinishedThisTurn) {
				waitForActiveEnemies.Add(curEnemyCharacter);
			}
		}

		if (waitForActiveEnemies.Count == 0) {
			SetGameState(EGameState.PlayerTurn);
		} else {
			bool startNextEnemy = true;
			// Now all the waiting enemies should has not finish this turn. So if someone has started, meaning it's doing some action
			// wait for all waiting enemies not start this turn, then send one of them to be active
			foreach (Character curWaitEnemy in waitForActiveEnemies) {
				if (curWaitEnemy.hasStartedThisTurn) {
					startNextEnemy = false;
				}
			}
			if (startNextEnemy) {
				waitForActiveEnemies[0].SwitchActionStateTo(ECharacterActionState.Idle);
			}
		}
	}

	/// <summary>
	/// This should keep track which player-controlled character is currently being chosen
	/// At any given time, makes sure only one character is active
	/// </summary>
	public void IssueCommandToPlayers() {
		// Grab the position of mouse
		Vector3 mouseClickWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
		Vector3Int clickCellInUnity = mapGrid.WorldToCell(mouseClickWorldPos);
		Vector3 clickedUnityCellCenterWorldPos = mapGrid.CellToWorld(clickCellInUnity);

		bool leftMouseClicked = Input.GetMouseButtonUp(0);
		foreach (GameObject playerCharGO in PlayerControlCharacters) {
			Character curPlayerCharacter = playerCharGO.GetComponent<Character>();
		}
	}

	public void StartNextTurnButtonListener() {
		if (_gameState == EGameState.PlayerTurn) {
			SetGameState(EGameState.AIEnemyTurn);
			// Need to reset all variables that player and enemy have
		}
	}

	public void AttackButtonListener() {

	}
}
