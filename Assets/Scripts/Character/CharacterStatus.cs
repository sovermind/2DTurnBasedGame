using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class contains character status. (attack, defense, exp, etc.)
/// Also should take care of character level up
/// </summary>
public class CharacterStatus : MonoBehaviour {
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

	private void Start() {
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
}
