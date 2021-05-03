using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class contains character status. (attack, defense, exp, etc.)
/// Also should take care of character level up
/// </summary>
public class CharacterStatus : MonoBehaviour {
	[SerializeField]
	private int _charLevel;
	public int charLevel {
		get {
			return _charLevel;
		}
	}

	[SerializeField]
	private int _maxLevel;
	public int maxLevel {
		get {
			return _maxLevel;
		}
	}

	[SerializeField]
	private int _currentXP;
	public int currentXP {
		get {
			return _currentXP;
		}
	}

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

	/// <summary>
	/// Each element represents total experience required to next level
	/// 0's element is 0 because lowest level is 1 and do not require any experience to reach
	/// </summary>
	[SerializeField]
	private int[] _nextLevelRequiredXP;
	public int[] nextLevelRequiredXP {
		get {
			return _nextLevelRequiredXP;
		}
	}

	private void Awake() {
		// Set up the experience required
		_nextLevelRequiredXP = new int[_maxLevel];
		_nextLevelRequiredXP[0] = 0;
		_nextLevelRequiredXP[1] = 100;

		for (int i = 2; i < _maxLevel; i ++) {
			_nextLevelRequiredXP[i] = _nextLevelRequiredXP[1] + Mathf.RoundToInt(_nextLevelRequiredXP[i - 1] * 1.2f);
		}

	}

	private void Start() {
		_charLevel = 1;
		_health = _maxHealth;
		_actionPoints = _maxActionPoints;
	}

	/// <summary>
	/// Set the action points. The function will gaurantee that ap is within [0, maxAP]
	/// </summary>
	/// <param name="ap"></param>
	public void SetAP(int ap) {
		if (ap > _maxActionPoints) {
			_actionPoints = _maxActionPoints;
		} else {
			_actionPoints = ap;
		}

		if (_actionPoints < 0) {
			_actionPoints = 0;
		}
	}

	/// <summary>
	/// Decrease health by amount, make sure health is always >= 0
	/// </summary>
	/// <param name="decreaseAmount"></param>
	public void HealthDecrease(uint decreaseAmount) {
		if (_health >= decreaseAmount) {
			_health = _health - decreaseAmount;
		}
		else {
			_health = 0;
		}
	}

	/// <summary>
	/// Add certain amount of XP to the character
	/// </summary>
	/// <param name="xp"></param>
	public void AddXP(int xp) {
		_currentXP += xp;
		// Cap the current xp to the max xp possible for this character
		if (_currentXP > _nextLevelRequiredXP[_maxLevel - 1]) {
			_currentXP = _nextLevelRequiredXP[_maxLevel - 1];
		}

		// Now check if the result xp need level up
		while (_charLevel < _maxLevel && _currentXP > _nextLevelRequiredXP[_charLevel]) {
			performLevelUp();
		}
	}

	/// <summary>
	/// Perform a level up to the char. Should
	/// 1. increase all necessary status values
	/// 2. unlock skills if meet required level
	/// 3. update UI for player info
	/// </summary>
	private void performLevelUp() {

	}
}
