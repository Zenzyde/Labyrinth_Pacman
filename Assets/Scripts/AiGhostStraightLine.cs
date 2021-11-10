using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiGhostStraightLine : AiGhostBase
{
	protected override void Move()
	{
		timeDelta += Time.deltaTime;
		if (timeDelta > moveRate)
		{
			Vector3Int direction = GetMoveDirection();
			if (labyrinth.LABYRINTH.GetTile(GridPosition + direction) == wall)
				return;
			transform.position += direction;
			timeDelta = 0.0f;
		}
	}

	Vector3Int GetMoveDirection()
	{
		Vector3Int direction = new Vector3Int();
		if (player.GetGridPosition().x > GridPosition.x)
		{
			direction.x = 1;
		}
		else if (player.GetGridPosition().x < GridPosition.x)
		{
			direction.x = -1;
		}

		if (player.GetGridPosition().y > GridPosition.y)
		{
			direction.y = 1;
		}
		else if (player.GetGridPosition().y < GridPosition.y)
		{
			direction.y = -1;
		}

		return direction;
	}
}
