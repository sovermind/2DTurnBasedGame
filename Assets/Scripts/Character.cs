using System.Collections;
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

public class Character : MonoBehaviour {
	private float waitForIdleTimeSec = 0.5f;

	[SerializeField]
	private ECharacterActionState _characterCurrentActionState;    // Current state of the character
	public ECharacterActionState characterCurrentActionState {
		get {
			return _characterCurrentActionState;
		}
	}

	[SerializeField]
	private float _movingIntervalSec = 0.2f;           // Time interval in sec for character move from current cell to the next
	public float movingIntervalSec {
		get {
			return _movingIntervalSec;
		}
	}


	[SerializeField]
	private int _health;
	public int health {
		set {
			_health = value;
		}
		get {
			return _health;
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

	public bool hasFinishedThisTurn;

	public int attackRangeRadius;

	private Animator charAnimator;

	private HexCell _charCurHexCell;
	public HexCell charCurHexCell {
		get {
			return _charCurHexCell;
		}
	}


	// Start is called before the first frame update
	void Start() {
		_characterCurrentActionState = ECharacterActionState.InActive;
		charAnimator = GetComponent<Animator>();
		hasFinishedThisTurn = false;
		_actionPoints = _maxActionPoints;
		_charCurHexCell = new HexCell(0, 0, 0);
	}

	// Update is called once per frame
	void Update() {
		_charCurHexCell = HexMap.hexMap.GetHexCellFromWorldPos(transform.position);

		switch (_characterCurrentActionState) {
			case ECharacterActionState.InActive:
				charAnimator.SetBool("IsWalking", false);
				StartCoroutine(WaitForIdleThenInactive());
				break;
			case ECharacterActionState.Idle:
				charAnimator.enabled = true;
				charAnimator.SetBool("IsWalking", false);
				break;
			case ECharacterActionState.Moving:
				charAnimator.SetBool("IsWalking", true);
				break;
			default:
				break;
		}
	}

	IEnumerator WaitForIdleThenInactive() {
		yield return new WaitForSeconds(waitForIdleTimeSec);
		charAnimator.enabled = false;
	}

	/// <summary>
	/// Switch the action state to the new state. Will check if the transition is allowed.
	/// If not allowed, will return false
	/// </summary>
	/// <param name="newState"></param>
	/// <returns></returns>
	public bool SwitchActionStateTo(ECharacterActionState newState) {
		_characterCurrentActionState = newState;
		return true;
	}


	/// <summary>
	/// End the current turn. Does not make much a difference for AI. 
	/// But for player controlled characters, player can choose to end the turn despite the current stats of characters
	/// </summary>
	public void EndThisTurn() {
		hasFinishedThisTurn = false;
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
}
