using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BuffController {
	// all buff one character has. use the name as the key so that one buff can only appear once
	private Dictionary<string, BuffEntity> allBuffEntities;

	public BuffController() {
		allBuffEntities = new Dictionary<string, BuffEntity>();
	}

	/// <summary>
	/// At the end of the turn, calculate all buff effect on characters and also delete non-effective buff
	/// </summary>
	public void EndTurnCalculation(Character curChar) {
		foreach (string curBuffName in allBuffEntities.Keys.ToList()) {
			BuffEntity curBuff = allBuffEntities[curBuffName];
			// Apply effect if buff takes effect at end of turn
			if (curBuff.buffType == EBuffType.ApplyOnTurnEnd) {
				curBuff.ApplyBuffEffect(curChar);
            }
			// Decrease the count of this buff
			curBuff.remainingTurns--;
			// Update the buff value in the internal buffer
			allBuffEntities[curBuff.buffName] = curBuff;
			// Now check if need to delete this buff if it's end of its life
			if (curBuff.remainingTurns <= 0) {
				DeleteBuff(curChar, curBuffName);
			}
        }
    }

	/// <summary>
	/// Function to add new buff when given a buff entity
	/// </summary>
	/// <param name="buff"></param>
	public void AddBuff(BuffEntity buff) {
		Debug.Log("Adding buff: " + buff.buffName);
		// Check if the buff already exist
		if (allBuffEntities.ContainsKey(buff.buffName)) {
			BuffEntity theBuff = allBuffEntities[buff.buffName];
			theBuff.StackBuff(buff);
			allBuffEntities[buff.buffName] = theBuff;
		}
		else {
			allBuffEntities.Add(buff.buffName, buff);
		}
	}

	/// <summary>
	/// Delete one buff from the list given the buff name
	/// </summary>
	/// <param name="buffName"></param>
	public void DeleteBuff(Character curChar, string buffName) {
		if (!allBuffEntities.ContainsKey(buffName)) {
			return; 
		}

		BuffEntity buff = allBuffEntities[buffName];
		if (buff.buffType == EBuffType.ApplyOnceImmediately) {
			buff.UnApplyBuffEffect(curChar);
		}
		allBuffEntities.Remove(buffName);
	}
}
