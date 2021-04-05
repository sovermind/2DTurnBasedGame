using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum EGameState {
	PlayerTurn,
	AIEnemyTurn,
	TurnSwitching
}

public enum EAttackAndPrimaryActiveSkillID {
	BasicAttack = 0,
	SkillOne,
	SkillTwo,
	SkillThree,
	SkillFour,
	AttackAndPrimaryActiveSkillCount
}

public class GameManager : MonoBehaviour {
	private static GameManager _instance;

	private Camera mainCam;
	private Grid mapGrid;
	protected GameObject mapGridGO;
	private bool nextTurnButtonHasPressed; // When the next turn button being pressed, but was not ready to start enemy turn yet

	private int _gameTurnCount;
	public int gameTurnCount {
		get {
			return _gameTurnCount;
		}
	}

	List<GameObject> PlayerControlCharacters;
	List<GameObject> AIEnemyCharacters;

	Character _curActivePlayerCharacter;
	public Character curActivePlayerCharacter {
		get {
			return _curActivePlayerCharacter;
		}
	}

	[SerializeField]
	public bool playerFirstStartTurn = true;

	[SerializeField]
	public Button nextTurnButton;

	[SerializeField]
	public Button[] basicAttackAndSkillButtons;

	[SerializeField]
	private SkillSO _defaultActiveSkill;
	static public SkillSO defaultActiveSkill;

	[SerializeField]
	public Texture2D shootingCursor;

	[SerializeField]
	public GameObject textPopupGO;

	[SerializeField]
	private EGameState _gameState;
	public EGameState gameState {
		get {
			return _gameState;
		}
	}

	//private bool _isOkToSwitchGameState;

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

	private bool _hasNotCallControllerEndTurn;

	public void SetGameState(EGameState newState) {
		Debug.Log("set game state to " + newState);
		if (_gameState == newState) {
			Debug.Log("old GS: " + _gameState + ", new GS: " + newState);
			return;
		}

		if (newState == EGameState.AIEnemyTurn || newState == EGameState.PlayerTurn) {
			UIDisplayController.SwitchTurnTo(newState);
		}

		// From old state -> new state corresponding to different cases
		switch (_gameState) {
			case EGameState.PlayerTurn:
				switch (newState) {
					case EGameState.AIEnemyTurn:
						foreach (GameObject curPlayerGO in PlayerControlCharacters) {
							Character curPlayer = curPlayerGO.GetComponent<Character>();
							curPlayer.SwitchActionStateTo(ECharacterActionState.InActive);
						}
						break;
					case EGameState.TurnSwitching:
						break;
					default:
						break;
				}
				break;

			case EGameState.AIEnemyTurn:
				switch (newState) {
					case EGameState.PlayerTurn:
						break;
					case EGameState.TurnSwitching:
						break;
					default:
						break;
				}

				break;

			case EGameState.TurnSwitching:
				_hasNotCallControllerEndTurn = true;
				_gameTurnCount = _gameTurnCount + 1;
				switch (newState) {
					case EGameState.PlayerTurn:
						// Reset the current active player character, and switch that to Idle state
						SetCurActivePlayerCharacter(PlayerControlCharacters[0].GetComponent<Character>());
						curActivePlayerCharacter.SwitchActionStateTo(ECharacterActionState.Idle);
						break;
					case EGameState.AIEnemyTurn:
						break;
					default:
						break;
				}
				break;
			default:
				break;
		}

		// Switch state
		_gameState = newState;
	}

	public void Awake() {
		if (_instance == null) {
			_instance = this;
		}
		_gameState = EGameState.PlayerTurn;
		defaultActiveSkill = _defaultActiveSkill;
		_hasNotCallControllerEndTurn = true;

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

	public List<GameObject> GetAllPlayerControlCharacters() {
		return PlayerControlCharacters;
	}

	public List<GameObject> GetAllAIEnemyCharacters() {
		return AIEnemyCharacters;
	}

	public void SetCurActivePlayerCharacter(Character chara) {
		_curActivePlayerCharacter = chara;
		// Move the camera to focus at the character
		//mainCam.transform.position = new Vector3(chara.transform.position.x, chara.transform.position.y, mainCam.transform.position.z);
	}

	private void Start() {
		_gameTurnCount = 1;
		nextTurnButtonHasPressed = false;
		mainCam = Camera.main;
		mapGridGO = GameObject.Find("WorldMapGrid");
		mapGrid = new Grid();
		if (mapGridGO != null) {
			mapGrid = mapGridGO.GetComponent<Grid>();
		}
		PlayerControlCharacters = new List<GameObject>();
		AIEnemyCharacters = new List<GameObject>();
		PlayerControlCharacters.AddRange(GameObject.FindGameObjectsWithTag("Player"));
		AIEnemyCharacters.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));

		// Make the first character in the player controlled characters array to be active
		if (PlayerControlCharacters.Count > 0) {
			SetCurActivePlayerCharacter(PlayerControlCharacters[0].GetComponent<Character>());
			curActivePlayerCharacter.SwitchActionStateTo(ECharacterActionState.Idle);
		} else {
			Debug.LogWarning("No player controlled character found?!");
		}

		_isLeftClickDownGamePlay = false;
		_isLeftClickUpGamePlay = false;

		Button nextTurnBtn = nextTurnButton.GetComponent<Button>();		
		nextTurnBtn.onClick.AddListener(StartNextTurnButtonListener);

		// TODO: Right now for testing purpose, will manually set up skills
		bool setSkillSuccess = curActivePlayerCharacter.SetActiveSkillToPrimaryBattleSkill("WarriorBleedBlade", 1);
		Debug.Log("set skill success: " + setSkillSuccess);

		//curActivePlayerCharacter.SetActiveSkillToPrimaryBattleSkill("Bleed Blade", 1);
		List<SkillSO> basicAttackAndprimarySkills = curActivePlayerCharacter.GetCurrentBasicAttackAndPrimaryBattleSkills();

		for (int i = 0; i < (int)EAttackAndPrimaryActiveSkillID.AttackAndPrimaryActiveSkillCount; i++) {
			Button attackAndSkillBtn = basicAttackAndSkillButtons[i];
			string btnNumb = string.Empty;
			string btnName = attackAndSkillBtn.name;
			for (int j = 0; j < btnName.Length; j++) {
				if (char.IsDigit(btnName[j])) {
					btnNumb += btnName[j];
				}
			}
			// By default the btn is 0, which is the basic attack button
			int btnN = 0;
			if (btnNumb.Length > 0) {
				btnN = int.Parse(btnNumb);
			}
			attackAndSkillBtn.onClick.AddListener(delegate { SkillAttackButtonListener(btnN); });

			// Set up the button sprite according to current primary skills stored in character
			if (basicAttackAndprimarySkills[i].skillIcon != null) {
				attackAndSkillBtn.image.sprite = basicAttackAndprimarySkills[i].skillIcon;
			}
		}
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

				CheckToStartEnemyTurn();

				break;
			case EGameState.TurnSwitching:
				// Here we need to handle end turn calculation for turn switching
				// 1. Do buff calculation for player & enemy (include animations)
				// 2. Reset character's properties
				if (_hasNotCallControllerEndTurn) {
					_hasNotCallControllerEndTurn = false;
					foreach (GameObject curEnemyGO in AIEnemyCharacters) {
						AIEnemyCharacterController curEnemyCharacterController = curEnemyGO.GetComponent<AIEnemyCharacterController>();
						curEnemyCharacterController.ControllerEndThisTurn();
					}
					foreach (GameObject curPlayerGO in PlayerControlCharacters) {
						PlayerCharacterController curPlayerCharController = curPlayerGO.GetComponent<PlayerCharacterController>();
						curPlayerCharController.ControllerEndThisTurn();
					}
				} else {
					Debug.Log("Ready to end turn for all characters");
					// Check if all end turn switch is done and can start a new turn now
					foreach (GameObject curEnemyGO in AIEnemyCharacters) {
						Character enemy_char = curEnemyGO.GetComponent<Character>();
						if (enemy_char.characterCurrentActionState != ECharacterActionState.InActive) {
							return;
						}
					}
					foreach (GameObject curPlayerGO in PlayerControlCharacters) {
						Character player_char = curPlayerGO.GetComponent<Character>();
						if (player_char.characterCurrentActionState != ECharacterActionState.InActive) {
							return;
						}
					}
					Debug.Log("all characters are inactive now");
					if (playerFirstStartTurn) {
						SetGameState(EGameState.PlayerTurn);
					} else {
						SetGameState(EGameState.AIEnemyTurn);
					}
					
				}

				break;
			default:
				break;
		}
	}

	/// <summary>
	/// Check to see if it's ok to start enemy's turn
	/// currently these need to be satisfied to start enemy's turn:
	/// 1. NextTurn button being pressed
	/// 2. all enemy is inactive state means all animation of enemy are finished
	/// 3. all players are either idle or inactive. So the switch turn does not interrupt anything that characters are doing
	/// 4. if player starts turn first, goes to turn switching
	/// 5. if enemy starts turn first, switch to enemy turn
	/// </summary>
	private void CheckToStartEnemyTurn() {
		bool isOkToSwitchGameState = true;
		// 1. next turn button being pressed
		if (nextTurnButtonHasPressed) {
			// 2. Check enemy states
			foreach (GameObject AIEnemyGO in AIEnemyCharacters) {
				Character enemy = AIEnemyGO.GetComponent<Character>();
				Debug.Log("enemy.characterCurrentActionState: " + enemy.characterCurrentActionState);
				if (enemy.characterCurrentActionState != ECharacterActionState.InActive) {
					isOkToSwitchGameState = false;
					break;
				}
			}

			// 3. Check player states
			if (isOkToSwitchGameState) {
				foreach (GameObject playerGO in PlayerControlCharacters) {
					Character curPlayer = playerGO.GetComponent<Character>();
					if (curPlayer.characterCurrentActionState != ECharacterActionState.Idle &&
						curPlayer.characterCurrentActionState != ECharacterActionState.InActive) {
						isOkToSwitchGameState = false;
						break;
					}
				}
			}

			if (isOkToSwitchGameState) {
				// 4. If player start the turn first, Switch to enemy turn
				if (playerFirstStartTurn) {
					SetGameState(EGameState.AIEnemyTurn);
				}
				// 5. If enemy start the turn first, switch to turn switching
				else {
					SetGameState(EGameState.TurnSwitching);
				}
			}


			// Reset the button pressed
			nextTurnButtonHasPressed = false;
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

		// need to check if all players are inactive state. (If still in hurting state, the animation will being paused)
		bool isOkToSwitchGameState = true;
		foreach (GameObject playerChar in PlayerControlCharacters) {
			Character pc = playerChar.GetComponent<Character>();
			if (pc.characterCurrentActionState != ECharacterActionState.InActive) {
				isOkToSwitchGameState = false;
				break;
			}
		}

		if (waitForActiveEnemies.Count != 0) {
			bool startNextEnemy = true;
			// Now all the waiting enemies should has not finish this turn. So if someone has started, meaning it's doing some action
			// wait for all waiting enemies not start this turn, then send one of them to be active
			foreach (Character curWaitEnemy in waitForActiveEnemies) {
				if (curWaitEnemy.hasStartedThisTurn) {
					startNextEnemy = false;
					break;
				}
			}
			if (startNextEnemy) {
				waitForActiveEnemies[0].SwitchActionStateTo(ECharacterActionState.Idle);
			}
		} else {
			// Now no wait enemies available, we can check if it's ok to end enemy turn now
			if (isOkToSwitchGameState) {
				if (playerFirstStartTurn) {
					SetGameState(EGameState.TurnSwitching);
				} else {
					SetGameState(EGameState.PlayerTurn);
				}
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
						SetCurActivePlayerCharacter(curPlayerCharacter);
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
			nextTurnButtonHasPressed = true;
		}
	}

	/// <summary>
	/// Basic attack and primary active skill button listeners.
	/// </summary>
	/// <param name="btnNumb">0 - basic attack; 1-4 correspond to skill 1-4</param>
	public void SkillAttackButtonListener(int btnNumb) {
		Debug.Log("btn pressed " + btnNumb);
		if (btnNumb < 0 || btnNumb > (int)(EAttackAndPrimaryActiveSkillID.AttackAndPrimaryActiveSkillCount)) {
			Debug.LogWarning("Invalid button number passed to call back " + btnNumb);
			return;
		}
		Vector2 hotSpot = new Vector2(shootingCursor.width/2, shootingCursor.height/2);
		Cursor.SetCursor(shootingCursor, hotSpot, CursorMode.Auto);
		curActivePlayerCharacter.curChosenAttackMethod = (EAttackAndPrimaryActiveSkillID)btnNumb;
		curActivePlayerCharacter.SwitchActionStateTo(ECharacterActionState.Attacking);
	}

	public static void ChangeMouseCursorToDefault() {
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
	}
}
