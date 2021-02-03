using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DoNothing", menuName = "Scriptable Objects/Skill/ActiveSkill/DoNothing", order = -1)]
public class DoNothing : ActiveSkillSO {
	public override void PerformActiveSkill(Character attacker, Character defender) {
		return;
	}

	public override int TriggerAnimation() {
		throw new System.NotImplementedException();
	}
}