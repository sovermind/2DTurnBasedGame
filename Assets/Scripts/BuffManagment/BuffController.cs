using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffController {
	// all buff one character has. use the name as the key so that one buff can only appear once
	private Dictionary<string, BuffEntity> allBuffEntities;

	/// <summary>
    /// At the end of the turn, calculate all buff effect on characters and also delete non-effective buff
    /// </summary>
	public void EndTurnCalculation(Character curChar) {
		// set up the list for all potential removal buffs. Can not remove the buff while iterating through the dictionary
		List<string> removalBuffs = new List<string>();
		foreach (BuffEntity curBuff in allBuffEntities.Values) {
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
				removalBuffs.Add(curBuff.buffName);
			}
        }

		foreach (string bn in removalBuffs) {
			DeleteBuff(bn);
        }
    }

	/// <summary>
	/// Function to add new buff
	/// </summary>
	/// <param name="buff"></param>
	public void AddBuff(BuffEntity buff) {

	}

	/// <summary>
	/// Delete one buff from the list given the buff name
	/// </summary>
	/// <param name="buffName"></param>
	public void DeleteBuff(string buffName) {

	}
}
