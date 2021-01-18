using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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

	Character curActivePlayerCharacter;

	[SerializeField]
	public Button nextTurnButton;

	[SerializeField]
	public Button attackButton;

	[SerializeField]
	public Texture2D shootingCursor;

	[SerializeField]
	private EGameState _gameState;

	public EGameState gameState {
		get {
			return _gameState;
		}
	}

	private static bool _isLeftClickDownGamePlay;
	public static bool isLeftClickDownGamePlay {
		get {
			return _isLeftClickDownGamePlay;
		}
	}

	private static bool _isLeftClickUpGamePlay;
	public static bool isLeftClickUpGamePlay {
		get {
			return _isLeftClickUpGamePlay;
		}
	}

	private static Vector2 cursorHotSpot = Vector2.zero;

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
			// Reset the current active player character, and switch that to Idle state
			curActivePlayerCharacter = PlayerControlCharacters[0].GetComponent<Character>();
			curActivePlayerCharacter.SwitchActionStateTo(ECharacterActionState.Idle);
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

		// Make the first character in the player controlled characters array to be active
		if (PlayerControlCharacters.Length > 0) {
			curActivePlayerCharacter = PlayerControlCharacters[0].GetComponent<Character>();
			curActivePlayerCharacter.SwitchActionStateTo(ECharacterActionState.Idle);
		} else {
			Debug.LogWarning("No player controlled character found?!");
		}

		_isLeftClickDownGamePlay = false;
		_isLeftClickUpGamePlay = false;

		Button nextTurnBtn = nextTurnButton.GetComponent<Button>();
		Button attackBtn = attackButton.GetComponent<Button>();
		
		nextTurnBtn.onClick.AddListener(StartNextTurnButtonListener);
		attackBtn.onClick.AddListener(AttackButtonListener);
	}

	/// <summary>
	/// This is called the first thing in the Update function. In unity project settings, make sure GameManager has a higher priority in script execution order
	/// Then all the other script can directly grab information from GameManager's variables rather than do the same logic check over and over
	/// </summary>
	void CheckUserInput() {
		if (Input.GetMouseButtonDown(0)) {
			// Check if the mouse was clicked over a UI element
			if (EventSystem.current.IsPointerOverGameObject()) {
				_isLeftClickDownGamePlay = false;
			}
			else {
				_isLeftClickDownGamePlay = true;
			}
		}
		// if the left click down is on the game play, then no matter where the button is up, it's still game play press
		// if the left click down is on UI display, then no matter where the button is up, it's not a game play press
		if (Input.GetMouseButtonUp(0)) {
			if (_isLeftClickDownGamePlay) {
				_isLeftClickUpGamePlay = true;
			} else {
				_isLeftClickUpGamePlay = false;
			}
		} else {
			_isLeftClickUpGamePlay = false;
		}
	}

	void Update() {
		CheckUserInput();
		switch (_gameState) {
			case EGameState.AIEnemyTurn:
				IssueCommandToEnemies();
				break;
			case EGameState.PlayerTurn:
				
				if (GameManager.isLeftClickUpGamePlay) {
					IssueCommandToPlayers();
				}
				
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
		//Vector3Int clickCellInUnity = mapGrid.WorldToCell(mouseClickWorldPos);
		//Vector3 clickedUnityCellCenterWorldPos = mapGrid.CellToWorld(clickCellInUnity);

		foreach (GameObject playerCharGO in PlayerControlCharacters) {
			Character curPlayerCharacter = playerCharGO.GetComponent<Character>();
			if (CommonUtil.IsPosInsideBound2D(mouseClickWorldPos, curPlayerCharacter.charSpriteRenderer.bounds)) {
				// clicking on a player controlled character
				if (curPlayerCharacter.charCurHexCell == curActivePlayerCharacter.charCurHexCell) {
					// clicking on the current active char, may be show the current character info?
				} else {
					// Player has chosen a different character to control
					// Need to make the current active character inactive.
					// If not allowed, meaning current active character is doing something, don't switch current active character
					if (curActivePlayerCharacter.SwitchActionStateTo(ECharacterActionState.InActive)) {
						// update the current active player character
						curActivePlayerCharacter = curPlayerCharacter;
						if (curActivePlayerCharacter.characterCurrentActionState == ECharacterActionState.InActive) {
							curActivePlayerCharacter.SwitchActionStateTo(ECharacterActionState.Idle);
						} else {
							Debug.LogWarning("current chosen character is already active before player select it, how come????");
						}
						
					}
				}
				break;
			}
		}

	}

	public void StartNextTurnButtonListener() {
		if (_gameState == EGameState.PlayerTurn) {
			SetGameState(EGameState.AIEnemyTurn);
			// Need to reset all variables that player and enemy have
		}
	}

	public void AttackButtonListener() {
		Debug.Log("attack button pressed!!!!!");
		
		Cursor.SetCursor(shootingCursor, cursorHotSpot, CursorMode.Auto);
		curActivePlayerCharacter.SwitchActionStateTo(ECharacterActionState.Attacking);
	}

	public static void ChangeMouseCursorToDefault() {
		Cursor.SetCursor(null, cursorHotSpot, CursorMode.Auto);
	}
}
