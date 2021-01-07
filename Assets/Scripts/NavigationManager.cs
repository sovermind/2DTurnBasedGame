using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class AStarCell : System.IEquatable<AStarCell> {
	public HexCell mapHexCell;
	public int fCost;
	public int hCost;
	public int gCost;
	public AStarCell parentCell;

	public AStarCell(HexCell mc) {
		mapHexCell = mc;
		fCost = 0;
		hCost = 0;
		gCost = MapManager.GetTileCostFromHexCell(mc);
		parentCell = null;
	}

	public AStarCell() {
		fCost = 0;
		hCost = 0;
		gCost = int.MaxValue;
		parentCell = null;
	}

	/// <summary>
	/// IEquatable required function. Determine if two cells are equal
	/// </summary>
	/// <param name="other">the other AStarCell compared with this one</param>
	/// <returns>true if two cells have same cell position</returns>
	public bool Equals(AStarCell other) {
		if (other == null) return false;
		return mapHexCell == other.mapHexCell;
	}
}

/// <summary>
/// Internal class for priority queue implementation on AStarCell.
/// Using the help of binery heap
/// </summary>
internal class AStarCellPQ {
	List<AStarCell> storage = new List<AStarCell>();

	/// <summary>
	/// Insert a cell into the PQ, sorted with least fCost
	/// if cell already exits, will update the cell if the new cell has less fCost
	/// </summary>
	/// <param name="cell">The cell trying to push into the PQ</param>
	public void Push(AStarCell cell) {
		for (int i = 0; i < storage.Count; i++) {
			AStarCell curStorageCell = storage[i];
			if (curStorageCell.Equals(cell)) {
				if (curStorageCell.fCost > cell.fCost) {
					storage[i] = cell;
					HeapifyUp(i);
					return;
				}
			}
		}
		storage.Add(cell);
		HeapifyUp(storage.Count - 1);
	}

	/// <summary>
	/// Grab the top of the cell in the PQ, which should has the least value of fCost
	/// Remove the top cell and heapify the PQ
	/// </summary>
	public AStarCell Pop() {
		if (storage.Count <= 0) {
			return null;
		}
		AStarCell result = storage[0];
		storage[0] = storage[storage.Count - 1];
		storage.RemoveAt(storage.Count - 1);
		HeapifyDown(0);
		return result;
	}

	/// <summary>
	/// Get the top of the PQ, but the element remain in the PQ
	/// </summary>
	/// <returns></returns>
	public AStarCell Top() {
		if (storage.Count > 0) {
			return storage[0];
		} else {
			return null;
		}
	}

	/// <summary>
	/// Clear the PQ internal buffer
	/// </summary>
	public void Clear() {
		storage.Clear();
	}

	/// <summary>
	/// Check if the PQ is empty at this point
	/// </summary>
	/// <returns>true if current no elements in the buffer</returns>
	public bool IsEmpty() {
		return storage.Count == 0;
	}

	/// <summary>
	/// Get the current size of the PQ
	/// </summary>
	/// <returns>current PQ size</returns>
	public int Size() {
		return storage.Count;
	}

	/// <summary>
	/// Swap two cells by their index.
	/// NOTE: this will potentially mess up with the PQ property. Need to call Heapify functions afterwards to ensure the PQ property
	/// </summary>
	/// <param name="a">index a to swap with b</param>
	/// <param name="b">index b to swap with a</param>
	private void Swap(int a, int b) {
		AStarCell temp = storage[a];
		storage[a] = storage[b];
		storage[b] = temp;
	}

	/// <summary>
	/// Get the parent cell's index when given the child index
	/// </summary>
	/// <param name="childIdx"></param>
	/// <returns></returns>
	private int GetParent(int childIdx) {
		return (childIdx - 1) / 2;
	}

	/// <summary>
	/// Get the left child index
	/// </summary>
	/// <param name="parentIdx"></param>
	/// <returns></returns>
	private int  LeftChild(int parentIdx) {
		return (2 * parentIdx + 1);
	}

	/// <summary>
	/// Get the right child index
	/// </summary>
	/// <param name="parentIdx"></param>
	/// <returns></returns>
	private int RightChild(int parentIdx) {
		return (2 * parentIdx) + 2;
	}

	/// <summary>
	/// Funtion that maintain the PQ properties. Move a cell up in the tree if that still suffice the PQ properties
	/// </summary>
	/// <param name="idx"></param>
	private void HeapifyUp(int idx) {
		int parentIdx = GetParent(idx);
		while (idx > 0 && storage[parentIdx].fCost > storage[idx].fCost) {
			Swap(parentIdx, idx);
			idx = parentIdx;
		}
	}

	/// <summary>
	/// Funtion that maintain the PQ properties. Move a cell down in the tree if that still suffice the PQ properties
	/// </summary>
	/// <param name="idx"></param>
	private void HeapifyDown(int idx) {
		int maxIdx = idx;

		int leftChild = LeftChild(idx);
		if (leftChild < storage.Count && storage[leftChild].fCost < storage[maxIdx].fCost) {
			maxIdx = leftChild;
		}

		int rightChild = RightChild(idx);
		if (rightChild < storage.Count && storage[rightChild].fCost < storage[maxIdx].fCost) {
			maxIdx = rightChild;
		}

		if (idx != maxIdx) {
			Swap(idx, maxIdx);
			HeapifyDown(maxIdx);
		}
	}
}

/// <summary>
/// Navigation manager will take care of moving characters from one map cell to another
/// It will out put a path consisting with different mapcells
/// Will utilize A* to get the most efficient path possible
/// </summary>
public class NavigationManager{

	private List<HexCell> path = new List<HexCell>();
	private int curPathIndex = 0;

	private AStarCellPQ openList;
	private List<AStarCell> closeList;

	public NavigationManager() {
		path = new List<HexCell>();
		curPathIndex = 0;
		openList = new AStarCellPQ();
		closeList = new List<AStarCell>();
	}

	/// <summary>
	/// Compute path from start cell to target cell. Also update the path's total cost.
	/// The path cost is just the tile costs add up. It is different with f, g, h costs
	/// </summary>
	/// <param name="startCell">Start cell</param>
	/// <param name="targetCell">target cell</param>
	/// <param name="pathCost">the current path's total cost</param>
	public bool ComputePath(HexCell startCell, HexCell targetCell, ref int pathCost) {
		// First clear up the previous path
		path = new List<HexCell>();
		curPathIndex = 0;

		// create the cell for A* and run A*
		AStarCell startC = new AStarCell(startCell);
		AStarCell targetC = new AStarCell(targetCell);
		path = AStarSearth(startC, targetC);

		if (path == null) {
			Debug.Log("No path found!!!");
		}

		pathCost = GetTotalPathCost();
		return true;
	}

	/// <summary>
	/// Get the cur path cell's world position.
	/// Use the boolean parameter to control if we are simply query this information and do not need the charater to actually move
	/// In not move case, we do not increment the curPathIndex
	/// </summary>
	/// <param name="move"></param>
	/// <returns></returns>
	public Vector3 GetCurPathCellWorldPos() {
		Vector3 result = HexMap.hexMap.GetWorldPosFromHexCell(path[curPathIndex]);
		return result;
	}

	public HexCell GetNextCellInPath() {
		if (curPathIndex >= path.Count - 1 || curPathIndex < 0) {
			return HexMap.kInvalidCell;
		} else {
			return path[curPathIndex + 1];
		}
	}

	public bool MoveOneStep(bool forward) {
		if (forward && curPathIndex + 1 <= path.Count) {
			curPathIndex++;
			return true;
		} else if (!forward && curPathIndex - 1 >= 0) {
			curPathIndex--;
			return true;
		}

		// If not able to move then return false
		return false;
	}

	/// <summary>
	/// Check if the character has already compelete the path
	/// </summary>
	/// <returns></returns>
	public bool IsPathComplete() {
		if (path == null || path.Count == 0) {
			return true;
		}
		// Debug.Log("cur path index: " + curPathIndex + " path count: " + path.Count);
		return curPathIndex >= path.Count;
	}

	/// <summary>
	/// Go through the internal buffer for the path and add up the tile costs
	/// </summary>
	/// <returns></returns>
	private int GetTotalPathCost() {
		int totalCount = 0;
		// Need to exclude the start cell because we already there and do not need to pay cost
		for (int i = 1; i < path.Count; i ++) {
			//Debug.Log("current cell cost: " + MapManager.GetTileCostFromHexCell(path[i]));
			totalCount += MapManager.GetTileCostFromHexCell(path[i]);
		}

		return totalCount;
	}

	/// <summary>
	/// Back trace a path list. Since the path cells are linked using parents, from the target we can back trace to the start cell
	/// </summary>
	/// <param name="targetCell"></param>
	/// <returns></returns>
	private List<HexCell> BackTracePath(AStarCell targetCell) {
		List<HexCell> resultReverse = new List<HexCell>();
		// Grab the actual cell from the world map singleton so all information is available
		resultReverse.Add(targetCell.mapHexCell);

		AStarCell curCell = targetCell;
		while (curCell.parentCell != null) {
			HexCell parentHexCell = curCell.parentCell.mapHexCell;
			resultReverse.Add(parentHexCell);
			curCell = curCell.parentCell;
		}

		// Reverse the path
		resultReverse.Reverse();
		//Debug.Log("NavigationManager: Path length: " + resultReverse.Count);
		return resultReverse;
	}

	/// <summary>
	/// A Star algorithm.
	/// </summary>
	/// <param name="startCell"></param>
	/// <param name="targetCell"></param>
	/// <returns></returns>
	private List<HexCell> AStarSearth(AStarCell startCell, AStarCell targetCell) {
		//Debug.Log("a star search from: " + startCell.mapHexCell.hexCellPos + " to: " + targetCell.mapHexCell.hexCellPos);
		openList.Clear();
		closeList.Clear();

		startCell.hCost = HexMap.hexMap.ManhattanDist(startCell.mapHexCell, targetCell.mapHexCell);
		startCell.gCost = 0;
		startCell.fCost = startCell.hCost;
		openList.Push(startCell);

		while (!openList.IsEmpty()) {
			AStarCell curLeastCostCell = openList.Pop();
			if (curLeastCostCell.Equals(targetCell)) {
				// We have found the destination cell, compute the path and exit
				//Debug.Log("find path!!! target cell: " + targetCell.mapHexCell.hexCellPos);
				return BackTracePath(curLeastCostCell);
			}

			// Check if the close list has this cell already
			if (!closeList.Contains(curLeastCostCell)) {
				closeList.Add(curLeastCostCell);

				// Expand all neighbors within the map
				List<AStarCell> allNeighbors = GenerateAllNeighbors(curLeastCostCell);
				//Debug.Log("all neighbors count: " + allNeighbors.Count);

				for (int i = 0; i < allNeighbors.Count; i++) {
					AStarCell curNeighCell = allNeighbors[i];
					// make sure close list does not already have this cell
					// if it is in the close list already, it is impossible that curNeighCell's f cost is less than the one in close list.
					if (!closeList.Contains(curNeighCell)) {
						// Update the cell cost
						curNeighCell.hCost = HexMap.hexMap.ManhattanDist(curNeighCell.mapHexCell, targetCell.mapHexCell);

						curNeighCell.gCost = curLeastCostCell.gCost + MapManager.GetTileCostFromHexCell(curNeighCell.mapHexCell) + CalculatePathCostBetweenCells(curLeastCostCell, curNeighCell);
						curNeighCell.fCost = curNeighCell.gCost + curNeighCell.hCost;
						// Push the neighbor to the open list. If already exit, update it if this neighbor has less f value
						openList.Push(curNeighCell);
					}
				}
			}
			
		}
		return null;
	}

	/// <summary>
	/// This function calculates path cost between two cells, will be used to update g cost of AStarCell
	/// TODO: The cost should depend on MapCell type or if there are enemies present on cell
	/// right now it return 1. 
	/// </summary>
	/// <param name="fromCell"></param>
	/// <param name="toCell"></param>
	/// <returns></returns>
	private int CalculatePathCostBetweenCells(AStarCell fromCell, AStarCell toCell) {
		return 2;
	}

	/// <summary>
	/// Function to generate all neighbors of current cell. Also check if the generated neighbor is valid in the map
	/// Will update the neighbors to have current cell as parent
	/// </summary>
	/// <param name="curCell"></param>
	/// <returns></returns>
	private List<AStarCell> GenerateAllNeighbors(AStarCell curCell) {
		List<AStarCell> result = new List<AStarCell>();
		List<HexCell> allNeighHexCells = curCell.mapHexCell.GetAllNeighbors();

		for (int i = 0; i < 6; i++) {
			HexCell curNeigh = allNeighHexCells[i];
			if (HexMap.hexMap.IsValidHexCellInMap(curNeigh)) {
				AStarCell neigh = new AStarCell();
				neigh.mapHexCell = curNeigh;
				neigh.parentCell = curCell;
				result.Add(neigh);
			}
		}

		return result;
	}
}
