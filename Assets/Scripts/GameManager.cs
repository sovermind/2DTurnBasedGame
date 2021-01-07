using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EGameState {
	PlayerTurn,
	AIEnemyTurn
}

public class GameManager : MonoBehaviour {
	private static GameManager _instance;

	GameObject[] PlayerControlCharacters;
	GameObject[] AIEnemyCharacters;

	public Button nextTurnButton;


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
		//_gameState = EGameState.AIEnemyTurn;
		PlayerControlCharacters = GameObject.FindGameObjectsWithTag("Player");
		AIEnemyCharacters = GameObject.FindGameObjectsWithTag("Enemy");

		Button nextTurnBtn = nextTurnButton.GetComponent<Button>();
		nextTurnBtn.onClick.AddListener(StartNextTurnButtonListener);
	}

	void Update() {
		switch (_gameState) {
			case EGameState.AIEnemyTurn:
				bool allEnemyFinishThisTurn = true;
				foreach (GameObject AIEnemy in AIEnemyCharacters) {
					Character curEnemyCharacter = AIEnemy.GetComponent<Character>();
					if (!curEnemyCharacter.hasFinishedThisTurn) {
						allEnemyFinishThisTurn = false;
						break;
					}
				}
				if (allEnemyFinishThisTurn) {
					SetGameState(EGameState.PlayerTurn);
				}
				break;
			case EGameState.PlayerTurn:
				break;
			default:
				break;
		}
	}

	public void StartNextTurnButtonListener() {
		if (_gameState == EGameState.PlayerTurn) {
			Debug.Log("switch to enemy turn");
			SetGameState(EGameState.AIEnemyTurn);
			// Need to reset all variables that player and enemy have
		}
	}
}
