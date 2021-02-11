using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using System;

/// <summary>
/// The whole hex map implementation is studied from https://www.redblobgames.com/grids/hexagons/
/// This is something totally different compare with normal square maps
/// </summary>
public class HexCell : System.IEquatable<HexCell> {
	// HexCell all 6 directions, starting from left, clockwise rotate
	static public List<HexCell> allDirections = new List<HexCell> {new HexCell(-1, 0),   // left
															       new HexCell(0, -1),   // left-top
															       new HexCell(1, -1),   // right-top
																   new HexCell(1, 0),    // right
																   new HexCell(0, 1),    // right-bottom
																   new HexCell(-1, 1)    // left-bottom
	                                                               };

	public readonly Vector3Int hexCellPos;

	public HexCell(int q, int r, int s) {
		Debug.Assert(q + r + s == 0, "HexCell construct fail: Invalid input q, r, s");
		hexCellPos = new Vector3Int(q, r, s);
	}

	public HexCell(int q, int r) {
		hexCellPos = new Vector3Int(q, r, -q-r);
	}

	public HexCell(Vector3Int cellPos) {
		Debug.Assert(cellPos.x + cellPos.y + cellPos.z == 0, "HexCell construct fail: Invalid input cellPos");
		hexCellPos = cellPos;
	}

	public HexCell(HexCell otherCell) {
		if (otherCell == null) {
			return;
		}
		hexCellPos = otherCell.hexCellPos;
	}

	/*******************************************
	 ******************* Equality **************
	 *******************************************/
	/// <summary>
	/// IEquatable required function. Determine if two cells are equal
	/// </summary>
	/// <param name="other">the other AStarCell compared with this one</param>
	/// <returns>true if two cells have same cell position</returns>
	public bool Equals(HexCell other) {
		if (ReferenceEquals(other, null)) {
			return ReferenceEquals(this, null);
		}
		return hexCellPos == other.hexCellPos;
	}

	public override bool Equals(object obj) {
		return this.Equals(obj as HexCell);
	}

	// TODO(OverrideGetHash): Not sure about this default override.
	public override int GetHashCode() {
		return base.GetHashCode();
	}

	public static bool operator ==(HexCell lhs, HexCell rhs) {
		return lhs.Equals(rhs);
	}

	public static bool operator !=(HexCell lhs, HexCell rhs) {
		return !(lhs.Equals(rhs));
	}

	/*******************************************
	***************** Arithmetic ***************
	********************************************/
	/// <summary>
	/// Add two hex cell's corresponding cell position 
	/// </summary>
	/// <returns></returns>
	public HexCell Add(HexCell other) {
		if (other == null) {
			return this;
		}
		return new HexCell(hexCellPos + other.hexCellPos);
	}

	/// <summary>
	/// Current Cell position minus the pass in cell's cell position
	/// </summary>
	/// <returns></returns>
	public HexCell Subtract(HexCell other) {
		if (other == null) {
			return this;
		}
		return new HexCell(hexCellPos - other.hexCellPos);
	}

	/// <summary>
	/// Multiply the current cell position by the multiplier
	/// </summary>
	/// <param name="multiplier"></param>
	/// <returns></returns>
	public HexCell Multiply(int multiplier) {
		return new HexCell(hexCellPos * multiplier);
	}

	/*******************************************
	***************** Neighbors ****************
	********************************************/

	/// <summary>
	/// Get the neighbor cell with provided direction. From left cell rotate clockwise, corresponding directions are 0, 1, 2, 3, 4, 5
	/// </summary>
	/// <param name="direction"></param>
	/// <returns></returns>
	public HexCell GetNeighborWithDirection(int direction) {
		return Add(allDirections[direction]);
	}

	public List<HexCell> GetAllNeighbors() {
		List<HexCell> allNeigh = new List<HexCell>();

		for (int i = 0; i < allDirections.Count(); i++) {
			allNeigh.Add(GetNeighborWithDirection(i));
		}

		return allNeigh;
	}
}

public class HexMap {
	// readonly constants
	public static readonly HexCell kInvalidCell = new HexCell(Int32.MaxValue / 2, Int32.MaxValue / 2);

	// Singleton private variable
	private static HexMap _hm = null;

	// Private 2d-list map storage
	private List<List<HexCell>> mapStorage;

	// Singleton private constructor
	private HexMap() {
		mapStorage = new List<List<HexCell>>();
	}

	// The origin of the whole map, world pos of cell(0, 0)
	private Vector3 hexMapOrigin;
	private int mapWidthInCell;
	private int mapHeightInCell;
	private float hexCellSideLength = 0.5f;

	// Singleton style
	public static HexMap hexMap {
		get {
			if (_hm == null) {
				_hm = new HexMap();
			}
			return _hm;
		}
	}

	/// <summary>
	/// Construct a rectangle hex point-up map
	/// For list[row][col], the corresponding cell should be (col - floor(row/2), row)
	/// The hex map cell (0, 0) will locate at lower bottom corner
	/// </summary>
	/// <param name="origin">This should be the expect world position of cell(0, 0)</param>
	public void ConstructHexWorldMap(int widthInCell, int heightInCell, Vector3 origin, float sideLength) {
		// Clear the map before construct new one
		mapStorage.Clear();
		if (origin == null) {
			hexMapOrigin = new Vector3(0, 0, 0);
		} else {
			hexMapOrigin = origin;
		}
		mapHeightInCell = heightInCell;
		mapWidthInCell = widthInCell;
		hexCellSideLength = sideLength;

		for (int row = 0; row < heightInCell; row++) {
			for (int col = 0; col < widthInCell; col++) {
				if (col == 0) {
					mapStorage.Add(new List<HexCell>());
				}
				HexCell newCell = new HexCell(col - Mathf.FloorToInt(row/2.0f), row);
				//int q = col - Mathf.FloorToInt(row / 2.0f);
				//Debug.Log("row: " + row + ", col: " + col + "; q" + q + ", r:" + row);
				mapStorage[row].Add(newCell);
			}
		}
	}

	/// <summary>
	/// Determine if a cell is a valid cell in the hexmap
	/// </summary>
	/// <param name="cell"></param>
	/// <returns></returns>
	public bool IsValidHexCellInMap(HexCell cell) {
		// First check r value is greater or equal to map row count
		if (cell.hexCellPos.y >= mapHeightInCell || cell.hexCellPos.y < 0) {
			return false;
		}
		int colIndex = cell.hexCellPos.x + Mathf.FloorToInt(cell.hexCellPos.y / 2.0f);
		if (colIndex >= mapWidthInCell || colIndex < 0) {
			return false;
		}
		return true;
	}

	public int GetWidth() {
		return mapWidthInCell;
	}

	public int GetHeight() {
		return mapHeightInCell;
	}

	/// <summary>
	/// Mainly used for iterate through all hex cells. Given a row and col of the 2D Array buffer position,
	/// get the corresponding hexCell
	/// </summary>
	/// <param name="row"></param>
	/// <param name="col"></param>
	/// <returns></returns>
	public HexCell GetHexCellFromBufferPosition(int row, int col) {
		if (col >= mapStorage.Count() || row >= mapStorage[0].Count()) {
			return kInvalidCell;
		}
		return mapStorage[row][col];
	}

	/// <summary>
	/// Get the world position of a given hex cell. 
	/// Note: this is depend on that the hex cell has equal sides
	/// </summary>
	/// <param name="cell"></param>
	/// <returns></returns>
	public Vector3 GetWorldPosFromHexCell(HexCell cell) {
		if (!IsValidHexCellInMap(cell)) {
			return kInvalidCell.hexCellPos;
		}
		Vector3 worldPos = new Vector3();

		// First compute distance from cell(0, 0) to the desired cell
		// Note: This depends on the geometry to be equal sided hex
		float xIncrement = Mathf.Sqrt(3.0f) * cell.hexCellPos.x + (Mathf.Sqrt(3.0f)) / 2.0f * cell.hexCellPos.y;
		float yIncrement = (3.0f / 2.0f) * cell.hexCellPos.y;

		// Add the increments to the origin of cell (0, 0)
		worldPos.x = xIncrement * hexCellSideLength + hexMapOrigin.x;
		worldPos.y = yIncrement * hexCellSideLength + hexMapOrigin.y;

		return worldPos;
	}

	/// <summary>
	/// Get the hexcell based on the world position.
	/// Note: this depends on the geometry to be equal sided hex
	/// </summary>
	/// <param name="worldPos"></param>
	/// <returns></returns>
	public HexCell GetHexCellFromWorldPos(Vector3 worldPos) {
		// Compute the distance respect to cell (0, 0) position
		float xIncrement = worldPos.x - hexMapOrigin.x;
		float yIncrement = worldPos.y - hexMapOrigin.y;

		float q = (Mathf.Sqrt(3.0f) / 3.0f * xIncrement - 1.0f / 3.0f * yIncrement) / hexCellSideLength;
		float r = (2.0f / 3.0f * yIncrement) / hexCellSideLength;
		return HexRound(q, r);
	}

	/// <summary>
	/// Round the given q and r value of hexcell to the proper hexcell
	/// </summary>
	/// <param name="q"></param>
	/// <param name="r"></param>
	/// <returns></returns>
	private HexCell HexRound(float q, float r) {
		int rq = Mathf.RoundToInt(q);
		int rr = Mathf.RoundToInt(r);
		int rs = Mathf.RoundToInt(1 - q - r);

		float deltaQ = Mathf.Abs(rq - q);
		float deltaR = Mathf.Abs(rr - r);
		float deltaS = Mathf.Abs(rs - (1 - q - r));

		if (deltaQ > deltaR && deltaQ > deltaS) {
			rq = -rr - rs;
		} else if (deltaR > deltaS) {
			rr = -rq - rs;
		} else {
			rs = -rq - rr;
		}

		return new HexCell(rq, rr, rs);
	}


	/// <summary>
	/// Calculate the manhattan distance between two hex cells
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns></returns>
	public int ManhattanDist(HexCell a, HexCell b) {
		if (a == null || b == null) {
			return 0;
		}

		HexCell delta = a.Subtract(b);
		Vector3Int deltaPos = delta.hexCellPos;
		//Debug.Log("manhattan dist from " + a.hexCellPos + " to " + b.hexCellPos + ": " + (int)((Mathf.Abs(deltaPos.x) + Mathf.Abs(deltaPos.y) + Mathf.Abs(deltaPos.z)) / 2));
		return (int)((Mathf.Abs(deltaPos.x) + Mathf.Abs(deltaPos.y) + Mathf.Abs(deltaPos.z)) / 2);
	}

	public List<HexCell> AllCellsWithinRadius(HexCell centerCell, int radius) {
		List<HexCell> allResultCells = new List<HexCell>();

		for (int i = -radius; i <= radius; i++) {
			for (int j = Math.Max(-radius, -i-radius); j <= Math.Min(radius, -i + radius); j++) {
				HexCell translateCell = new HexCell(i, j);
				HexCell resultCell = translateCell.Add(centerCell);
				if (!IsValidHexCellInMap(resultCell)) {
					continue;
				} else {
					allResultCells.Add(resultCell);
				}
			}
		}

		return allResultCells;
	}


	public List<HexCell> AllPossibleDestinationCells(HexCell curCell, int availableAP) {
		List<HexCell> allReachableCells = new List<HexCell>();

		NavigationManager navHelper = new NavigationManager();

		for (int i = -availableAP; i <= availableAP; i++) {
			for (int j = Math.Max(-availableAP, -i - availableAP); j <= Math.Min(availableAP, -i + availableAP); j++) {
				HexCell translateCell = new HexCell(i, j);
				HexCell testCell = translateCell.Add(curCell);

				if (!IsValidHexCellInMap(testCell)) {
					continue;
				}
				int totalPathCost = Int32.MaxValue;
				navHelper.ComputePath(curCell, testCell, ref totalPathCost);
				if (totalPathCost <= availableAP) {
					allReachableCells.Add(testCell);
					//Debug.Log("can reach: " + mapStorage[i][j].hexCellPos);
				}
			}
		}


		return allReachableCells;
	}

	public GameObject DoesHexCellContainGO(HexCell cell, List<GameObject> GOList) {
		foreach (GameObject curGO in GOList) {
			Vector3 curGOPos = curGO.transform.position;
			HexCell curGOCell = GetHexCellFromWorldPos(curGOPos);
			if (curGOCell.Equals(cell)) {
				return curGO;
			}
		}
		return null;
	}

}
