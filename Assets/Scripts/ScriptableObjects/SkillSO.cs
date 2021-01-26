using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSO : ScriptableObject {
	public string skillName;
	public Sprite skillIcon;
	public int level;
	public int maxLevel;
	public int[] costToUpgrade;
}
