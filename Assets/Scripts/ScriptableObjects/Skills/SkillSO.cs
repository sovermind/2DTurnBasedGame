using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum SkillTargetType {
	Ally = 1,
	Enemy = 2,
	Self = 4
}

[System.Flags]
public enum SkillDamageType {
	PhysicalDamage = 1,
	MagicDamage = 2,
	Healing = 4
}

// Scriptable object is essentially a single copy of data. Any one accessing this is getting a pointer to this copy of data
// In that sense, all these variables should be private and provide getter function?
public abstract class SkillSO : ScriptableObject {
	public string skillName;
	public Sprite skillIcon;
	public int maxLevel;
	public int[] costToUpgrade;
	public SkillTargetType targetType;
	public float skillAnimationDuration;
	public int attackRangeRadInCell;
	public int effectiveRangeRadInCell;
	public int castCost;
	public SkillDamageType damageType;

	private void Awake() {
		skillName = this.name;
		Debug.Log("skill awake: " + skillName);
	}

	public abstract void PerformActiveSkill(Character attacker, Character defender);

	/// <summary>
	/// By default, the skill animation is just basic attack
	/// </summary>
	/// <param name="charAnimator"></param>
	/// <returns></returns>
	public virtual float TriggerAnimation(Animator charAnimator) {
		charAnimator.SetTrigger("BasicAttack");
		return skillAnimationDuration;
	}
}
