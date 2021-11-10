using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AiGhostAStar : AiGhostBase
{
	[SerializeField] private int roomCost, corridorCost, baseCost;

	private Vector3Int[] offsets = new Vector3Int[]
	{
		new Vector3Int(-1, 0, 0),
		new Vector3Int(1, 0, 0),

		new Vector3Int(0, -1, 0),
		new Vector3Int(0, 1, 0)
	};

	private Vector3Int min, max;

	private Dictionary<Vector3Int, AStarNode> grid = new Dictionary<Vector3Int, AStarNode>();
	private List<AStarNode> openSet = new List<AStarNode>();
	private HashSet<AStarNode> closedSet = new HashSet<AStarNode>();

	protected override void Start()
	{
		base.Start();
		min = new Vector3Int(0, 0, 0);
		max = new Vector3Int(labyrinth.LABYRINTHSIZE.x, labyrinth.LABYRINTHSIZE.y, 0);
		foreach (Vector3Int pos in labyrinth.WALKABLETILES)
		{
			if (!grid.ContainsKey(pos))
				grid.Add(pos, new AStarNode(pos));
		}
	}

	protected override void Move()
	{
		timeDelta += Time.deltaTime;
		if (timeDelta > moveRate)
		{
			AStarNode start = new AStarNode(GridPosition);
			AStarNode end = new AStarNode(player.GetGridPosition());

			//Clear previous pathfinded sets -- needs to be done since the path is getting actively updated
			openSet.Clear();
			closedSet.Clear();

			//Find/Update path
			List<AStarNode> path = FindPath(start, end);

			if (path != null && path.Count > 0)
			{
				//Move to next position in the found path
				transform.position = path[0].position + new Vector3(.5f, .5f, -1);
				//Remove position from found path
				path.RemoveAt(0);
			}
			timeDelta = 0.0f;
		}
	}

	List<AStarNode> FindPath(AStarNode start, AStarNode goal)
	{
		openSet.Add(start);

		while (openSet.Count > 0)
		{
			AStarNode current = openSet[0];
			for (int i = 0; i < openSet.Count; i++)
			{
				if (openSet[i].totalDistFCost < current.totalDistFCost ||
					openSet[i].totalDistFCost == current.totalDistFCost && openSet[i].distFromGoalHCost < current.distFromGoalHCost)
				{
					current = openSet[i];
				}
			}

			openSet.Remove(current);
			closedSet.Add(current);

			if (current.position == goal.position)
			{
				return ReconstructPath(start, current);
			}

			foreach (Vector3Int offset in offsets)
			{
				//* Check if position is walkable, continue if not
				if (!IsWalkable(current.position + offset))
				{
					continue;
				}

				if (grid.TryGetValue(current.position + offset, out AStarNode neighbour))
				{
					if (closedSet.Contains(neighbour) || current.previousSet.Contains(neighbour.position))
					{
						continue;
					}

					PathTraverseCost traverseCost = GetHeuristic(current, neighbour, goal);

					if (!traverseCost.isTraversable)
					{
						continue;
					}

					int newCost = (int)(current.distFromStartGCost + traverseCost.cost);
					if (newCost < neighbour.distFromStartGCost || !openSet.Contains(neighbour))
					{
						neighbour.distFromStartGCost = newCost;
						neighbour.distFromGoalHCost = (int)traverseCost.cost;
						neighbour.previous = current;

						openSet.Add(neighbour);

						neighbour.previousSet.Clear();
						neighbour.previousSet.UnionWith(current.previousSet);
						neighbour.previousSet.Add(current.position);
					}
				}
			}
		}
		return null;
	}

	List<AStarNode> ReconstructPath(AStarNode start, AStarNode current)
	{
		List<AStarNode> path = new List<AStarNode>();
		AStarNode node = current;
		while (node != start)
		{
			path.Add(node);
			node = node.previous;
		}
		path.Reverse();
		return path;
	}

	struct PathTraverseCost
	{
		public PathTraverseCost(bool traversable, float cost)
		{
			isTraversable = traversable;
			this.cost = cost;
		}

		public bool isTraversable;
		public float cost;
	}

	PathTraverseCost GetHeuristic(AStarNode current, AStarNode neighbour, AStarNode goal)
	{
		float cost = 0;
		bool isTraversable = false;

		cost = Vector3.Distance(neighbour.position, goal.position); //* Heuristic

		bool room = neighbour.isRoom;

		bool corridor = neighbour.isCorridor;

		if (room)
		{
			cost += roomCost;
		}
		else if (corridor)
		{
			cost += corridorCost;
		}
		else
		{
			cost += corridorCost;
		}

		isTraversable = true;

		return new PathTraverseCost(isTraversable, cost);
	}

	bool IsWalkable(Vector3Int position)
	{
		return grid.ContainsKey(position);
	}

	internal class AStarNode
	{
		public AStarNode previous;
		public Vector3Int position;
		public float cost;
		public int distFromStartGCost, distFromGoalHCost;
		public HashSet<Vector3Int> previousSet = new HashSet<Vector3Int>();
		public bool isRoom, isCorridor, isEmpty, isTraversable;

		public int totalDistFCost { get { return distFromStartGCost + distFromGoalHCost; } }

		public AStarNode(Vector3Int position)
		{
			this.position = position;
		}
	}
}
