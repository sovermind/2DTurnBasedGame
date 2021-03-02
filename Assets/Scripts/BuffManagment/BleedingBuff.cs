using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BleedingBuff : BuffEntity {
	private int _damagePerTurn;

	/// <summary>
	/// Constructor to create a bleeding buff.
	/// </summary>
	/// <param name="affectTurns"></param>
	/// <param name="buffT"></param>
	/// <param name="damPT"></param>
	public BleedingBuff(string name, int affectTurns, int damPT) : base(name, affectTurns) {
		_buffType = EBuffType.ApplyOnTurnEnd;
		_damagePerTurn = damPT;
	}

	public override bool ApplyEffectOnTurnEnds(Character curChar) {
		if (buffType != EBuffType.ApplyOnTurnEnd) {
			Debug.LogWarning("Buff type and apply affect method not same!");
			return false;
		}

		// let the character take the damage
		curChar.TakeDamage((uint)_damagePerTurn);
		return true;
	}
}
