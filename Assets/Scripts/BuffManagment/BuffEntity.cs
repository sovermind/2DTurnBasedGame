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
	/// apply affect on turn start. If buff not of this type, return false
	/// Each buff of this type will need to override this function.
	/// Otherwise if any buff of this type is using this default function, return false to notify user
	/// </summary>
	/// <returns></returns>
	public virtual bool ApplyEffectOnTurnStart(Character curChar) {
		if (buffType != EBuffType.ApplyOnTurnStart) {
			return true;
		}
		return false;
	}

	public virtual bool ApplyEffectOnceImmediately(Character curChar) {
		if (buffType != EBuffType.ApplyOnceImmediately) {
			return true;
		}
		return false;
	}

	public virtual bool ApplyEffectOnTurnEnds(Character curChar) {
		if (buffType != EBuffType.ApplyOnTurnEnd) {
			return true;
		}
		return false;
	}
}
