using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum ECharacterActionState {
	// TODO: may need to revisit this. unity seems to have none and everything by default.
	// Decimal       // Binary
	None = 0,        // 000000
	Everything = 1,  // 000001
	InActive = 2,    // 000010
	Idle = 4,        // 000100
	Moving = 8,      // 001000
	Attacking = 16,  // 010000
	Hurting = 32     // 100000
}

struct SkillStatus {
	public int curLevel;

	public SkillStatus(int cl) {
		curLevel = 0;
	}
}

[RequireComponent(typeof(SpriteRenderer))]
public class Character : MonoBehaviour {
	[Header("UI Related")]
	public StatsBarController healthBar;

	private SpriteRenderer _charSpriteRenderer;
	public SpriteRenderer charSpriteRenderer {
		get {
			return _charSpriteRenderer;
		}
	}

	private float damageTextNormalSizeRatio = 1.05f;
	private float damageTextCritSizeRatio = 1.15f;

	[Header("Constants")]
	[SerializeField]
	private float _movingIntervalSec = 0.2f;           // Time interval in sec for character move from current cell to the next
	public float movingIntervalSec {
		get {
			return _movingIntervalSec;
		}
	}

	[Header("Character states")]
	[SerializeField]
	private ECharacterActionState _characterCurrentActionState;    // Current state of the character
	public ECharacterActionState characterCurrentActionState {
		get {
			return _characterCurrentActionState;
		}
	}

	private ECharacterActionState _characterPrevActionState;

	public bool hasFinishedThisTurn;

	public bool hasStartedThisTurn;

	private bool _hasStartAttack;
	public bool hasStartAttack {
		get {
			return _hasStartAttack;
		}
	}

	private bool _attackDone;
	public bool attackDone {
		get {
			return _attackDone;
		}
	}


	private string _curCharacterName;
	[Header("Character properties")]
	[SerializeField]
	private uint _health;
	public uint health {
		get {
			return _health;
		}
	}

	[SerializeField]
	private uint _maxHealth;
	public uint maxHealth {
		get {
			return _maxHealth;
		}
	}

	[SerializeField]
	private int _attack;
	public int attack {
		get {
			return _attack;
		}
	}

	[SerializeField]
	private int _defend;
	public int defend {
		get {
			return _defend;
		}
	}

	[SerializeField]
	private int _actionPoints;
	public int actionPoints {
		set {
			_actionPoints = value;
		}

		get {
			return _actionPoints;
		}
	}

	[SerializeField]
	private int _maxActionPoints = 4;
	public int maxActionPoints {
		get {
			return _maxActionPoints;
		}
	}

	[Header("Character Skills")]
	public SkillSO curCharbasicAttack;
	public SkillSO[] allPossibleActiveSkills;
	private Dictionary<SkillSO, SkillStatus> allSkillDict;
	private SkillSO[] AttackAndPrimaryActiveSkills =
		new SkillSO[(int)(EAttackAndPrimaryActiveSkillID.AttackAndPrimaryActiveSkillCount)];

	public BuffController charBuffController;

	private Animator charAnimator;

	private HexCell _charCurHexCell;
	public HexCell charCurHexCell {
		get {
			Vector3 worldPos = MapManager.GetMapGridCellCenterWorldPos(transform.position);
			_charCurHexCell = HexMap.hexMap.GetHexCellFromWorldPos(worldPos);
			return _charCurHexCell;
		}
	}

	// This should be the current target that this character is focused on(attack, move towards, heal, etc.). But what if character has area attack?
	// Should this later be a list instead?
	private Character _curTargetCharacter;
	public Character curTargetCharacter {
		get {
			return _curTargetCharacter;
		}
	}

	private EAttackAndPrimaryActiveSkillID _curChosenAttackMethod;
	public EAttackAndPrimaryActiveSkillID curChosenAttackMethod {
		get {
			return _curChosenAttackMethod;
		}

		set {
			_curChosenAttackMethod = value;
		}
	}

	public void SetCurTargetCharacter(Character targetChar) {
		//if (targetChar == null) {
		//	Debug.LogWarning("Set target character is null");
		//	return;
		//}
		_curTargetCharacter = targetChar;
	}

	private void Awake() {
		// Game Manager on start will switch character state to Idle so these following variables are needed before that being called
		// Thus these has to be in Awake() (or some function before GameManager's start())
		_characterCurrentActionState = ECharacterActionState.InActive;
		_characterPrevActionState = ECharacterActionState.InActive;
		charAnimator = GetComponent<Animator>();
		_charSpriteRenderer = GetComponent<SpriteRenderer>();

		// On start animation should be inactive
		charAnimator.SetBool("IsInActive", true);
		charAnimator.SetBool("IsWalking", false);

		for (int i = 0; i < (int)(EAttackAndPrimaryActiveSkillID.AttackAndPrimaryActiveSkillCount); i++) {
			AttackAndPrimaryActiveSkills[i] = GameManager.defaultActiveSkill;
		}
		AttackAndPrimaryActiveSkills[(int)(EAttackAndPrimaryActiveSkillID.BasicAttack)] = curCharbasicAttack;

		// Construct the dictionary for all the skills
		allSkillDict = new Dictionary<SkillSO, SkillStatus>();
		if (allPossibleActiveSkills.Length > 0) {
			foreach (SkillSO activeSkillso in allPossibleActiveSkills) {
				if (!allSkillDict.ContainsKey(activeSkillso)) {
					allSkillDict.Add(activeSkillso, new SkillStatus(0));
				}
				else {
					Debug.LogWarning("Potential duplicate skill SO exist: " + activeSkillso.skillName);
				}
			}
		}

		charBuffController = new BuffController();
	}

	// Start is called before the first frame update
	void Start() {
		_curCharacterName = this.name;
		hasFinishedThisTurn = false;
		hasStartedThisTurn = false;
		_hasStartAttack = false;
		_attackDone = false;
		_health = _maxHealth;
		_actionPoints = _maxActionPoints;
		// Make sure the character starts off at the center of the hexcell
		transform.position = HexMap.hexMap.GetWorldPosFromHexCell(charCurHexCell);

		// UI Control init
		healthBar.SetStatsMaxAmount((int)_maxHealth);
	}

	// Update is called once per frame
	void Update() {

		switch (_characterCurrentActionState) {
			case ECharacterActionState.Hurting:
				// Check if the normalized time greater than 100% and make sure it's not in transition
				if (charAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !charAnimator.IsInTransition(0)) {
					SwitchActionStateTo(_characterPrevActionState);
				}
				break;
			default:
				break;
		}
	}

	/// <summary>
	/// Switch the action state to the new state. Will check if the transition is allowed.
	/// If not allowed, will return false
	/// </summary>
	/// <param name="newState"></param>
	/// <returns></returns>
	public bool SwitchActionStateTo(ECharacterActionState newState) {
		Debug.Log("Switch action state from: " + _characterCurrentActionState + " to: " + newState);
		_characterPrevActionState = _characterCurrentActionState;

		// If for some reason the character has not been set to inactive yet and we ask to transit state, make it in active first
		//if (_characterCurrentActionState == ECharacterActionState.None) {
		//	_characterCurrentActionState = ECharacterActionState.InActive;
		//}
		// State transition should depend on the current state
		switch (_characterCurrentActionState) {
			case ECharacterActionState.InActive:
				switch (newState) {
					case ECharacterActionState.InActive:
						break;
					case ECharacterActionState.Idle:
						charAnimator.SetBool("IsInActive", false);
						charAnimator.SetBool("IsWalking", false);
						hasStartedThisTurn = true;
						break;
					case ECharacterActionState.Hurting:
						charAnimator.SetTrigger("Hurt");
						charAnimator.SetBool("IsInActive", false);
						break;
					default:
						Debug.LogWarning("Invalid state transition from Inactive to " + newState);
						return false;
				}
				break;
			case ECharacterActionState.Idle:
				switch (newState) {
					case ECharacterActionState.InActive:
						charAnimator.SetBool("IsInActive", true);
						charAnimator.SetBool("IsWalking", false);
						break;
					case ECharacterActionState.Idle:
						break;
					case ECharacterActionState.Moving:
						charAnimator.SetBool("IsInActive", false);
						charAnimator.SetBool("IsWalking", true);
						break;
					case ECharacterActionState.Attacking:
						charAnimator.SetBool("IsWalking", false);
						break;
					case ECharacterActionState.Hurting:
						charAnimator.SetTrigger("Hurt");
						break;
					default:
						break;
				}
				break;
			case ECharacterActionState.Moving:
				switch (newState) {
					case ECharacterActionState.InActive:
						charAnimator.SetBool("IsInActive", true);
						charAnimator.SetBool("IsWalking", false);
						break;
					case ECharacterActionState.Idle:
						charAnimator.SetBool("IsWalking", false);
						break;
					case ECharacterActionState.Moving:
						break;
					case ECharacterActionState.Attacking:
						charAnimator.SetBool("IsWalking", false);
						break;
					case ECharacterActionState.Hurting:
						charAnimator.SetTrigger("Hurt");
						break;
					default:
						break;
				}
				break;
			case ECharacterActionState.Attacking:
				switch (newState) {
					case ECharacterActionState.InActive:
						charAnimator.SetBool("IsInActive", true);
						charAnimator.SetBool("IsWalking", false);
						break;
					case ECharacterActionState.Idle:
						charAnimator.SetBool("IsInActive", false);
						charAnimator.SetBool("IsWalking", false);
						break;
					case ECharacterActionState.Attacking:
						break;
					case ECharacterActionState.Hurting:
						charAnimator.SetTrigger("Hurt");
						break;
					default:
						Debug.LogWarning("Invalid state transition from Attacking to " + newState);
						return false;
				}
				break;
			case ECharacterActionState.Hurting:
				switch (newState) {
					case ECharacterActionState.InActive:
						charAnimator.SetBool("IsInActive", true);
						charAnimator.SetBool("IsWalking", false);
						break;
					case ECharacterActionState.Idle:
						charAnimator.SetBool("IsInActive", false);
						charAnimator.SetBool("IsWalking", false);
						break;
					case ECharacterActionState.Moving:
						charAnimator.SetBool("IsInActive", false);
						charAnimator.SetBool("IsWalking", true);
						break;
					case ECharacterActionState.Attacking:
						break;
					default:
						Debug.LogWarning("Invalid state transition from Hurting to " + newState);
						return false;
				}
				break;
			default:
				break;
		}

		_characterCurrentActionState = newState;

		return true;
	}


	/// <summary>
	/// End the current turn. Does not make much a difference for AI. 
	/// But for player controlled characters, player can choose to end the turn despite the current stats of characters
	/// </summary>
	public void EndThisTurn() {
		// Before ending this turn, make sure all buff has been calculated
		charBuffController.EndTurnCalculation(this);

		hasFinishedThisTurn = false;
		hasStartedThisTurn = false;
		_hasStartAttack = false;
		_attackDone = false;
		SwitchActionStateTo(ECharacterActionState.InActive);
		// restore action points
		_actionPoints = maxActionPoints;
	}

	/// <summary>
	/// Calculate all attackable cells. This should depend on the current chosen attack method
	/// </summary>
	/// <returns></returns>
	public List<HexCell> GetAllAttackableCells() {
		// First get all potential cells within attack range
		List<HexCell> allPotentialAttackableCells = HexMap.hexMap.AllCellsWithinRadius(charCurHexCell, AttackAndPrimaryActiveSkills[(int)curChosenAttackMethod].attackRangeRadInCell);

		// (TODO): Then check if there's obstacles in between

		return allPotentialAttackableCells;
	}

	/// <summary>
	/// Check if the character can attack the target cell
	/// </summary>
	/// <param name="targetCell"></param>
	/// <returns></returns>
	public bool IsTargetAttackable(HexCell targetCell) {
		List<HexCell> allAttackableCells = GetAllAttackableCells();
		foreach (HexCell curCell in allAttackableCells) {
			if (curCell.Equals(targetCell)) {
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Perform the attack option, includes basic attack, skills. Depend on attack method and cur target character, execute different actions
	/// </summary>
	public bool PerformAttack() {
		if (curTargetCharacter == null) {
			Debug.Log("No target character! No action performed!");
			return false;
		}
		bool okToProcceed = false;
		// First check if the attack target type matches the chosen attack method
		// You don't want to attack ally!
		SkillSO chosenMethod = AttackAndPrimaryActiveSkills[(int)curChosenAttackMethod];
		switch (chosenMethod.targetType) {
			case SkillTargetType.Ally:
				if (IsMyAlly(curTargetCharacter)) {
					okToProcceed = true;
				}
				break;
			case SkillTargetType.Enemy:
				if (!IsMyAlly(curTargetCharacter)) {
					okToProcceed = true;
				}
				break;
			case SkillTargetType.Self:
				if (charCurHexCell.Equals(curTargetCharacter.charCurHexCell)) {
					okToProcceed = true;
				}
				break;
			default:
				break;
		}

		if (!okToProcceed) {
			Debug.Log("Illegal attack type, skill target type: " + chosenMethod.targetType + ", target tag: " + curTargetCharacter.tag);
			return false;
		}

		// Now the attack type must be ok, we need to check if it's within attack range
		okToProcceed = false;
		List<HexCell> allAttackableCells = GetAllAttackableCells();
		foreach (HexCell cell in allAttackableCells) {
			if (cell.Equals(curTargetCharacter.charCurHexCell)) {
				okToProcceed = true;
				break;
			}
		}
		
		if (!okToProcceed) {
			Debug.Log("Target at cell: " + curTargetCharacter.charCurHexCell.hexCellPos + " not within attackable cells");
			return false;
		}

		// Now it's ok to perform the action on the target
		_hasStartAttack = true;
		Debug.Log("attack method: " + _curChosenAttackMethod);
		float animationDuration = chosenMethod.TriggerAnimation(charAnimator);

		StartCoroutine(WaitForAnimationToFinish(animationDuration));
		return true;
	}

	public void TakeDamage(uint damageAmount) {
		ECharacterActionState curState = _characterCurrentActionState;
		SwitchActionStateTo(ECharacterActionState.Hurting);
		if (_health >= damageAmount) {
			_health = _health - damageAmount;
		} else {
			_health = 0;
		}

		// UI adjustments
		healthBar.SetStatsCurAmount((int)_health);
		TextPopup.Create(transform.position, "-100", damageTextNormalSizeRatio);
	}

	IEnumerator WaitForAnimationToFinish(float duration) {
		yield return new WaitForSeconds(duration);
		// Damage the target using current chosen attack method
		AttackAndPrimaryActiveSkills[(int)(curChosenAttackMethod)].PerformActiveSkill(this, curTargetCharacter);
		_attackDone = true;
	}

	public bool SetActiveSkillToPrimaryBattleSkill(string skillName, int skillBtnNumb) {
		if (skillBtnNumb <= 0 || skillBtnNumb > (int)EAttackAndPrimaryActiveSkillID.AttackAndPrimaryActiveSkillCount) {
			Debug.LogWarning("Invalid skill btn number " + skillBtnNumb);
			return false;
		}
		SkillSO prevPrimeSkill = AttackAndPrimaryActiveSkills[skillBtnNumb];

		foreach (KeyValuePair<SkillSO, SkillStatus> entry in allSkillDict) {
			if (entry.Key.skillName.Equals(skillName)) {
				// Put this skill into the list for primary active skill
				AttackAndPrimaryActiveSkills[skillBtnNumb] = entry.Key;

				// No need to proceed further
				break;
			}
		}

		// we get here because the new skill does not exist.
		return false;
	}

	/// <summary>
	/// Return the primary skills in a list. This will exclude the basic attack
	/// </summary>
	/// <returns></returns>
	public List<SkillSO> GetCurrentBasicAttackAndPrimaryBattleSkills() {
		List<SkillSO> result = new List<SkillSO>();

		for (int i = 0; i < (int)EAttackAndPrimaryActiveSkillID.AttackAndPrimaryActiveSkillCount; i++) {
			result.Add(AttackAndPrimaryActiveSkills[i]);
		}

		return result;
	}

	public bool IsMyAlly(Character chara) {
		return this.tag == chara.tag;
	}
}
