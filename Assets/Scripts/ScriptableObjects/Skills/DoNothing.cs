using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DoNothing", menuName = "Scriptable Objects/Skill/DoNothing", order = -1)]
public class DoNothing : SkillSO {
	public override void PerformActiveSkill(Character attacker, Character defender) {
		return;
	}
}