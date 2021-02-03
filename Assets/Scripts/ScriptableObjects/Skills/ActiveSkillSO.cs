using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActiveSkillSO : SkillSO {
	public int attackRangeRadInCell;
	public int effectiveRangeRadInCell;
	public int castCost;
	public SkillDamageType damageType;
}
