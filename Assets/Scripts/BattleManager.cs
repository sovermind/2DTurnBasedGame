using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager {
	public static uint CalculateBasicAttackDamage(Character attacker, Character defender) {
		int damage = attacker.attack - defender.defend;
		damage = damage <= 0 ? 0 : damage;

		return (uint)(damage);
	}
}
