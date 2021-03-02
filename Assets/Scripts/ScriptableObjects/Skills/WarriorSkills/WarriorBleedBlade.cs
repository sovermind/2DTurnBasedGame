using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WarriorBleedBlade", menuName = "Scriptable Objects/Skill/ActiveSkill/WarriorBleedBlade")]
public class WarriorBleedBlade : SkillSO {
	public int initialDamage;
	public int bleedDamagePerTurn;
	public float initialDamageAttackerMultipler;
	public int bleedAffectTurns;

	public override void PerformActiveSkill(Character attacker, Character defender) {
		int totalInitialDamage = initialDamage + (int)(attacker.attack * initialDamageAttackerMultipler) - defender.defend;
		totalInitialDamage = totalInitialDamage > 0 ? totalInitialDamage : 0;
		defender.TakeDamage((uint)(totalInitialDamage));

		// now create the buff and add to the defender
		BleedingBuff bleedBuff = new BleedingBuff("BleedingBuff", bleedAffectTurns, bleedDamagePerTurn);
		defender.charBuffController.AddBuff(bleedBuff);
	}

}
