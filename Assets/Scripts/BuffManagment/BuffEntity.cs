using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EBuffType {
	None,
	ApplyOnceImmediately,
	ApplyOnTurnStart,
	ApplyOnTurnEnd
}

public class BuffEntity {
	private string _buffName;
	public string buffName {
		get {
			return buffName;
		}
	}

	private int _remainingTurns;
	public int remainingTurns {
		get {
			return _remainingTurns;
		}
		set {
			_remainingTurns = value;
        }
	}

	// total turns this buff will be activated
	protected int _totalDurationTurns;
	public int totalDurationTurns {
		get {
			return _totalDurationTurns;
		}
	}

	protected EBuffType _buffType;
	public EBuffType buffType {
		get {
			return _buffType;
		}
	}

	public BuffEntity(string name, int affectTurns) {
		InitializeBuff(name, affectTurns);
	}

	/// <summary>
	/// Each buff should implement this. They should have different initialization.
	/// buff type clear to None. All buff child class should define the type in constructor
	/// </summary>
	public void InitializeBuff(string name, int affectTurns) {
		_buffName = name;
		_totalDurationTurns = affectTurns;
		_remainingTurns = affectTurns;
		_buffType = EBuffType.None;
	}

	/// <summary>
	/// apply buff effect to the charater. The caller should make sure call this based on buff type
	/// </summary>
	/// <returns>true if apply effect successfully; false if not</returns>
	public virtual bool ApplyBuffEffect(Character curChar) {
		return false;
	}
}
