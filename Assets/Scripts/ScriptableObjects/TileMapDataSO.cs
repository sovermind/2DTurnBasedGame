using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Scriptable Objects/TileMapData")]
public class TileMapDataSO : ScriptableObject {
	public Sprite tileMapSprite;
	public int cost;
	public int priority;
}
