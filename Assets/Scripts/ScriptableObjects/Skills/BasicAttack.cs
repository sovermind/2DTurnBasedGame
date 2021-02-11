using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BasicAttack", menuName = "Scriptable Objects/Skill/BasicAttack", order = 0)]
public class BasicAttack : SkillSO {
	public override void PerformActiveSkill(Character attacker, Character defender) {
		int damage = attacker.attack - defender.defend;
		damage = damage <= 0 ? 0 : damage;
		defender.TakeDamage((uint)(damage));
	}
}
