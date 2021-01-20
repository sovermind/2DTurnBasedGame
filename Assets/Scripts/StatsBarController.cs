using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This is the controller for contrl a stats bar (health bar, mana bar, etc.)
/// </summary>
[RequireComponent(typeof(Slider))]
public class StatsBarController : MonoBehaviour {
	[SerializeField]
	private Slider barSlider;

	public void SetStatsCurAmount(int amount) {
		barSlider.value = amount;
	}

	/// <summary>
	/// Set the max amount of the stats, and set the current value of slider to that max amount as well
	/// </summary>
	/// <param name="maxAmount"></param>
	public void SetStatsMaxAmount(int maxAmount) {
		barSlider.maxValue = maxAmount;
		barSlider.value = maxAmount;
	}

    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update() {
        
    }
}
