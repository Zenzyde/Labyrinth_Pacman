using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiGhostDrunkenWalk : AiGhostBase
{
	private HashSet<Vector3Int> visitedTiles = new HashSet<Vector3Int>();
	private Vector3Int[] directions = new Vector3Int[]
	{
		Vector3Int.up,
		Vector3Int.down,
		Vector3Int.left,
		Vector3Int.right
	};

	protected override void Move()
	{
		timeDelta += Time.deltaTime;
		if (timeDelta > moveRate)
		{
			Vector3Int direction = GetRndMoveDirection();
			if (labyrinth.LABYRINTH.GetTile(GridPosition + direction) == wall)
				return;
			transform.position += direction;
			timeDelta = 0.0f;
		}
	}

	Vector3Int GetRndMoveDirection()
	{
		//Clear visited tiles if we've stumbled into a corner and can't get back out to select a new direction
		if (!CheckValidDirections())
			visitedTiles.Clear();

		Vector3Int direction = directions[rand.Next(0, directions.Length)];
		while (visitedTiles.Contains(GridPosition + direction))
		{
			direction = directions[rand.Next(0, directions.Length)];
		}
		visitedTiles.Add(GridPosition + direction);

		return direction;
	}

	bool CheckValidDirections()
	{
		return !visitedTiles.Contains(GridPosition + directions[0]) || !visitedTiles.Contains(GridPosition + directions[1]) ||
			!visitedTiles.Contains(GridPosition + directions[2]) || !visitedTiles.Contains(GridPosition + directions[3]);
	}
}
