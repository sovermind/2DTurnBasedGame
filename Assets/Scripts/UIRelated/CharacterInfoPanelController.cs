using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class CharacterInfoPanelController : MonoBehaviour {
	GameObject nameDisplay;
	GameObject levelDisplay;
	GameObject occupasionDisplay;
	GameObject experienceDisplay;

	GameObject attackDisplay;
	GameObject defendDisplay;


    // Start is called before the first frame update
    void Start() {
		nameDisplay = transform.Find("Name").gameObject;
		levelDisplay = transform.Find("Level").gameObject;
		occupasionDisplay = transform.Find("Occupasion").gameObject;
		experienceDisplay = transform.Find("Experience").gameObject;
		attackDisplay = transform.Find("Attack").gameObject;
		defendDisplay = transform.Find("Defend").gameObject;
	}

	public void SetLevelDisplay(int setLevel, int maxLevel) {
		TextMeshProUGUI charLevelText = levelDisplay.GetComponent<TextMeshProUGUI>();
		charLevelText.SetText("Lv. " + setLevel.ToString() + " / " + maxLevel.ToString());
	}

	public void SetExperienceDisplay(int curXP, int nextLevelXP) {
		TextMeshProUGUI charXPText = experienceDisplay.GetComponent<TextMeshProUGUI>();
		charXPText.SetText("Exp. " + curXP.ToString() + " / " + nextLevelXP.ToString());
	}

	public void SetAttackDisplay(int attackValue) {
		Text attackText = attackDisplay.GetComponent<Text>();
		attackText.text = "Att: " + attackValue.ToString();
	}

	public void SetDefendDisplay(int defendValue) {
		Text defendText = defendDisplay.GetComponent<Text>();
		defendText.text = "Def: " + defendValue.ToString();
	}
}
