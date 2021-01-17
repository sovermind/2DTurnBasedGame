#define DEBUG_MAP_OUTPUTS

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Flags]
public enum ETileHighlightType {
	NoneHighlight = 0,
	MoveRange = (1 << 0),
	AttackRange = (1 << 1)
}

public struct TileMapData {
	public int tileMapCost;
	public ETileHighlightType highlightType;

	public TileMapData(int cost, ETileHighlightType hlType) {
		tileMapCost = cost;
		highlightType = hlType;
	}
}

[RequireComponent(typeof(Grid))]
[RequireComponent(typeof(List<TileMapDataSO>))]
public class MapManager : MonoBehaviour{
	[SerializeField]
	public bool navigationManagerDebug;

	[SerializeField]
	private List<TileMapDataSO> tileMapDataHelper = new List<TileMapDataSO>();

	public static List<TileMapDataSO> allTileMapData;  // Contains all tilemaps' data including cost, priority, etc.

	private static Grid mapGrid;
	private static bool hasBeenInitialized = false;

	// Maybe store all map related data in this dictionary????? cost, should highlight??
	private static Dictionary<Vector3Int, TileMapData> tileMapDataDict;

	// Get all tilemaps on Grid
	void Start() {
		tileMapDataDict = new Dictionary<Vector3Int, TileMapData>();
		allTileMapData = new List<TileMapDataSO>();
		foreach (TileMapDataSO tm in tileMapDataHelper) {
			allTileMapData.Add(tm);
		}

		mapGrid = GetComponent<Grid>();

		InitializeWorldMap();
	}

	void InitializeWorldMap() {
		hasBeenInitialized = true;
		int xMin = int.MaxValue;
		int yMin = int.MaxValue;

		int xMax = int.MinValue;
		int yMax = int.MinValue;
		foreach (Transform child in transform) {
			//child is your child transform
			Tilemap curTilemap = child.GetComponent<Tilemap>();
			// Compress bounds will get rid of any unnesassary cells
			curTilemap.CompressBounds();

			// Get the x and y min and max for all tilemaps
			if (curTilemap.cellBounds.min.x < xMin) {
				xMin = curTilemap.cellBounds.min.x;
			}

			if (curTilemap.cellBounds.min.y < yMin) {
				yMin = curTilemap.cellBounds.min.y;
			}

			if (curTilemap.cellBounds.max.x > xMax) {
				xMax = curTilemap.cellBounds.max.x;
			}

			if (curTilemap.cellBounds.max.y > yMax) {
				yMax = curTilemap.cellBounds.max.y;
			}

			// Add tile cost into the cost map
			AddTileMapCosts(curTilemap);
		}

		Vector3 worldPosMin = mapGrid.CellToWorld(new Vector3Int(xMin, yMin, 0));
		int widthInCell = xMax - xMin;
		int heightInCell = yMax - yMin;
		HexMap.hexMap.ConstructHexWorldMap(widthInCell, heightInCell, worldPosMin, mapGrid.cellSize.y / 2);
	}

	private void AddTileMapCosts(Tilemap tileMap) {
		for (int i = tileMap.cellBounds.min.x; i <= tileMap.cellBounds.max.x; i ++) {
			for (int j = tileMap.cellBounds.min.y; j <= tileMap.cellBounds.max.y; j++) {
				Vector3Int curTileUnityCellPos = new Vector3Int(i, j, 0);
				TileBase tile = tileMap.GetTile(curTileUnityCellPos);
				foreach (TileMapDataSO tm in allTileMapData) {
					if (tile != null) {
						if (tm.tileMapSprite.name == tile.name) {
							if (tileMapDataDict.ContainsKey(curTileUnityCellPos)) {
								TileMapData curTileData = tileMapDataDict[curTileUnityCellPos];
								curTileData.tileMapCost = curTileData.tileMapCost + tm.cost;
								tileMapDataDict[curTileUnityCellPos] = curTileData;
							}
							else {
								tileMapDataDict.Add(curTileUnityCellPos, new TileMapData(tm.cost, ETileHighlightType.NoneHighlight));
							}
						}
					}
				}
			}
		}
	}

	static public int GetTileCostFromHexCell(HexCell cell) {
		if (!hasBeenInitialized) {
			return 0;
		}

		// If the cell quested is outof map
		if (!HexMap.hexMap.IsValidHexCellInMap(cell)) {
			return Int32.MaxValue;
		}

		// Get the world pos of the hexcell
		Vector3 worldPosCell = HexMap.hexMap.GetWorldPosFromHexCell(cell);
		Vector3Int unityWorldCellPos = mapGrid.WorldToCell(worldPosCell);

		TileMapData resultData = new TileMapData() ;
		if(tileMapDataDict.TryGetValue(unityWorldCellPos, out resultData)) {
			return resultData.tileMapCost;
		} else {
			return 0;
		}
	}

	// Update is called once per frame
	void Update() {
#if DEBUG_MAP_OUTPUTS
		if (Input.GetMouseButtonUp(0) && !GameManager.isClickOnUI) {
			Vector3 mouseClickWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector3Int clickCell = mapGrid.WorldToCell(mouseClickWorldPos);
			Vector3 worldpOS = mapGrid.CellToWorld(clickCell);
			HexCell clickHexCell = HexMap.hexMap.GetHexCellFromWorldPos(worldpOS);
			//Debug.Log("click cell: " + clickCell + " mouseClickWorldPos: " + mouseClickWorldPos + ", world pos from cell: " + worldpOS + ", hex cell: " + clickHexCell.hexCellPos);
			if (tileMapDataDict.ContainsKey(clickCell)) {
				TileMapData curTileData = tileMapDataDict[clickCell];
				curTileData.highlightType = curTileData.highlightType | ETileHighlightType.MoveRange;
				tileMapDataDict[clickCell] = curTileData;
			}
		}
#endif

		if (navigationManagerDebug) {
			NavigationManager navDebugger = new NavigationManager();
			HexCell startCell = new HexCell(1, 7, -8);
			HexCell targetCell = new HexCell(6, 7, -13);
			int pathCost = 0;
			navDebugger.ComputePath(startCell, targetCell, ref pathCost);
			List<HexCell> completePath = navDebugger.GetCurFullPath();
			SetHighlightCells(completePath, ETileHighlightType.MoveRange);

			navigationManagerDebug = false;
		}

		foreach (KeyValuePair<Vector3Int, TileMapData> entry in tileMapDataDict) {
			HighlightThisCell(entry.Key, entry.Value.highlightType);
		}

	}

	private void OnDrawGizmos() {
#if DEBUG_MAP_OUTPUTS
		int mapWidth = HexMap.hexMap.GetWidth();
		int mapHeight = HexMap.hexMap.GetHeight();
		for (int row = 0; row < mapHeight; row++) {
			for (int col = 0; col < mapWidth; col++) {
				HexCell curCell = HexMap.hexMap.GetHexCellFromBufferPosition(row, col);
				Vector3 curCellWorldPos = HexMap.hexMap.GetWorldPosFromHexCell(curCell);
				Gizmos.color = Color.red;
				Gizmos.DrawSphere(curCellWorldPos, 0.05f);
				int cur_cost = MapManager.GetTileCostFromHexCell(curCell);
				//Handles.Label(curCellWorldPos, curCellWorldPos.ToString());
				Handles.Label(curCellWorldPos, curCell.hexCellPos.ToString());
				Vector3 costPos = curCellWorldPos - new Vector3(0, 0.2f, 0);
				Handles.Label(costPos, cur_cost.ToString());
			}
		}
#endif
	}

	/// <summary>
	/// Set a list of cells with desired highlight type. Note that the current highlighted cells in the map will set to non-highlighted first
	/// </summary>
	/// <param name="allHighlightHexCells"></param>
	/// <param name="hlType"></param>
	static public void SetHighlightCells(List<HexCell> allHighlightHexCells, ETileHighlightType hlType) {
		// First set all tile to be not highlighted
		List<Vector3Int> allKeys = new List<Vector3Int>(tileMapDataDict.Keys);
		foreach (Vector3Int curPos in allKeys) {
			ETileHighlightType curType = tileMapDataDict[curPos].highlightType;
			// If the current highlight type contain the highlight type we desired to set, clear it.
			tileMapDataDict[curPos] = new TileMapData(tileMapDataDict[curPos].tileMapCost, (curType & (~hlType)));
		}

		// Go through the one that need to be highlighted
		foreach (HexCell curCell in allHighlightHexCells) {
			Vector3 worldPos = HexMap.hexMap.GetWorldPosFromHexCell(curCell);
			Vector3Int unityCellPos = mapGrid.WorldToCell(worldPos);
			TileMapData curCellData = new TileMapData();
			if (tileMapDataDict.TryGetValue(unityCellPos, out curCellData)) {
				curCellData.highlightType = curCellData.highlightType | hlType;
				tileMapDataDict[unityCellPos] = curCellData;
			}
		}
	}

	/// <summary>
	/// Clear the highlight type on the given tiles, but remain the rest values
	/// </summary>
	/// <param name="allHighlightHexCells"></param>
	/// <param name="clearType"></param>
	static public void ClearHighlightedCells(List<HexCell> allHighlightHexCells, ETileHighlightType clearType) {
		// Go through the one that need to be clear
		foreach (HexCell curCell in allHighlightHexCells) {
			Vector3 worldPos = HexMap.hexMap.GetWorldPosFromHexCell(curCell);
			Vector3Int unityCellPos = mapGrid.WorldToCell(worldPos);
			TileMapData curCellData = new TileMapData();
			if (tileMapDataDict.TryGetValue(unityCellPos, out curCellData)) {
				curCellData.highlightType = (curCellData.highlightType & (~clearType));
				tileMapDataDict[unityCellPos] = curCellData;
			}
		}
	}

	/// <summary>
	/// This is called in update function. It will paint the tile at the position based on the highlight data
	/// </summary>
	/// <param name="unityCell">The unity cell position. This is not a hex cell position.</param>
	/// <param name="hlType">Desired highlight type</param>
	private void HighlightThisCell(Vector3Int unityCell, ETileHighlightType hlType) {
		foreach (Transform child in transform) {
			//child is your child transform
			Tilemap curTilemap = child.GetComponent<Tilemap>();

			curTilemap.SetTileFlags(unityCell, TileFlags.None);
			Color moveRangeColor = new Color(0.0f, 1.0f, 0.0f, 0.8f);
			Color attackRangeColor = new Color(1.0f, 0.0f, 0.0f, 0.8f);
			Color deHighlightColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

			if (hlType == ETileHighlightType.NoneHighlight) {
				curTilemap.SetColor(unityCell, deHighlightColor);
			} else {
				if ((hlType & ETileHighlightType.MoveRange) == ETileHighlightType.MoveRange) {
					curTilemap.SetColor(unityCell, moveRangeColor);
				}

				if ((hlType & ETileHighlightType.AttackRange) == ETileHighlightType.AttackRange) {
					curTilemap.SetColor(unityCell, attackRangeColor);
				}
			}

		}
	}
}
