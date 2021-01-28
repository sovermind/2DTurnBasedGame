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
public class SkillSO : ScriptableObject {
	public string skillName;
	public Sprite skillIcon;
	public int maxLevel;
	public int[] costToUpgrade;
	public SkillTargetType targetType;
}
