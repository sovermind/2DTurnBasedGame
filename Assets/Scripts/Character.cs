﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum ECharacterActionState {
	// Decimal       // Binary
	None = 0,        // 000000
	Everything = 1,  // 000001
	InActive = 2,    // 000010
	Idle = 4,        // 000100
	Moving = 8,      // 001000
	Attacking = 16   // 010000
}
[RequireComponent(typeof(SpriteRenderer))]
public class Character : MonoBehaviour {
	[Header("UI Related")]
	public StatsBarController healthBar;

	private float waitForAttackAnimationTimeSec = 1.0f;
	private SpriteRenderer _charSpriteRenderer;
	public SpriteRenderer charSpriteRenderer {
		get {
			return _charSpriteRenderer;
		}
	}

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

	public int attackRangeRadius;

	private Animator charAnimator;

	private HexCell _charCurHexCell;
	public HexCell charCurHexCell {
		get {
			Debug.Log("pos: " + transform.position);
			_charCurHexCell = HexMap.hexMap.GetHexCellFromWorldPos(transform.position);
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

	public void SetCurTargetCharacter(Character targetChar) {
		_curTargetCharacter = targetChar;
	}

	private void Awake() {
		// Game Manager on start will switch character state to Idle so these following variables are needed before that being called
		// Thus these has to be in Awak() (or some function before GameManager's start())
		_characterCurrentActionState = ECharacterActionState.InActive;
		charAnimator = GetComponent<Animator>();
		_charSpriteRenderer = GetComponent<SpriteRenderer>();

		// On start animation should be inactive
		charAnimator.SetBool("IsInActive", true);
		charAnimator.SetBool("IsWalking", false);
	}

	// Start is called before the first frame update
	void Start() {
		hasFinishedThisTurn = false;
		hasStartedThisTurn = false;
		_hasStartAttack = false;
		_attackDone = false;
		_health = _maxHealth;
		_actionPoints = _maxActionPoints;
		_charCurHexCell = new HexCell(0, 0, 0);

		// UI Control init
		healthBar.SetStatsMaxAmount((int)_maxHealth);
		Debug.Log("my current position: " + charCurHexCell.hexCellPos);

	}

	// Update is called once per frame
	void Update() {
		_charCurHexCell = HexMap.hexMap.GetHexCellFromWorldPos(transform.position);
	}

	/// <summary>
	/// Switch the action state to the new state. Will check if the transition is allowed.
	/// If not allowed, will return false
	/// </summary>
	/// <param name="newState"></param>
	/// <returns></returns>
	public bool SwitchActionStateTo(ECharacterActionState newState) {
		Debug.Log("Switch action state from: " + _characterCurrentActionState + " to: " + newState);
		
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
					default:
						Debug.LogWarning("Invalid state transition from Attacking to " + newState);
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
		hasFinishedThisTurn = false;
		hasStartedThisTurn = false;
		_hasStartAttack = false;
		_attackDone = false;
		SwitchActionStateTo(ECharacterActionState.InActive);
		// restore action points
		_actionPoints = maxActionPoints;
	}

	/// <summary>
	/// Calculate all attackable cells. A cell is attackable if
	/// 1. it is within the attack range.
	/// 2. there is no blocking cells between curCharCell and the target cell
	/// </summary>
	/// <returns></returns>
	public List<HexCell> GetAllAttackableCells() {
		// First get all potential cells within attack range
		List<HexCell> allPotentialAttackableCells = HexMap.hexMap.AllCellsWithinRadius(charCurHexCell, attackRangeRadius);

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
	/// Perform the attack option. Currently only the basic attack. Later should has an input parameter with attack type
	/// The character controller should tell the character which attack to perform
	/// </summary>
	public void PerformAttack() {
		_hasStartAttack = true;
		charAnimator.SetTrigger("BasicAttack");

		StartCoroutine(WaitForBasicAttackToFinish());

		// Damage the target
		uint damage = BattleManager.CalculateBasicAttackDamage(this, curTargetCharacter);
		curTargetCharacter.TakeDamage(damage);
	}

	public void TakeDamage(uint damageAmount) {
		if (_health >= damageAmount) {
			_health = _health - damageAmount;
		} else {
			_health = 0;
		}

		// UI adjustments
		healthBar.SetStatsCurAmount((int)_health);
	}

	IEnumerator WaitForBasicAttackToFinish() {
		yield return new WaitForSeconds(waitForAttackAnimationTimeSec);
		_attackDone = true;
	}
}
