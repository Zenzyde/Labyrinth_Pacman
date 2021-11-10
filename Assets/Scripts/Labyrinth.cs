using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Labyrinth : MonoBehaviour
{
	[SerializeField] int labyrinthWidth, labyrinthHeight;
	[SerializeField] int minRoomWidth, minRoomHeight;
	[SerializeField] int maxRecursionDepth;

	[SerializeField] Tilemap labyrinth;
	[SerializeField] TileBase floorTile, wallTile, corridorTile, roomWallTile, roomWallCornerTile;

	Stack<BSPNode> partitionStack = new Stack<BSPNode>();
	List<Vector3Int> walkableTiles = new List<Vector3Int>();
	List<Vector3Int> powerupTiles = new List<Vector3Int>();

	System.Random rand;

	private bool doneGenerating = false;

	[HideInInspector] public Tilemap LABYRINTH { get { return labyrinth; } }

	[HideInInspector] public TileBase FLOOR { get { return floorTile; } }
	[HideInInspector] public TileBase WALL { get { return wallTile; } }
	[HideInInspector] public TileBase CORRIDOR { get { return corridorTile; } }

	[HideInInspector] public Vector2Int LABYRINTHSIZE { get { return new Vector2Int(labyrinthWidth, labyrinthHeight); } }
	[HideInInspector] public List<Vector3Int> WALKABLETILES { get { return walkableTiles; } }

	[HideInInspector] public bool DONE_GENERATING { get { return doneGenerating; } }

	// Start is called before the first frame update
	void Awake()
	{
		rand = new System.Random(System.DateTime.Now.Second);
		SetupWalls();
	}

	public void UpdateLabyrinth(Vector3Int pos, TileBase tileType)
	{
		labyrinth.SetTile(pos, tileType);
	}

	void SetupWalls()
	{
		List<BSPTile> BSPNodeTiles = new List<BSPTile>();

		int minX = (int)transform.position.x;
		int maxX = (int)transform.position.x + labyrinthWidth;
		int minY = (int)transform.position.y;
		int maxY = (int)transform.position.y + labyrinthHeight;

		for (int i = minX; i < maxX; i++)
		{
			for (int j = minY; j < maxY; j++)
			{
				Vector3Int pos = new Vector3Int(i, j, 0);
				labyrinth.SetTile(pos, wallTile);
				BSPNodeTiles.Add(BSPTile.CreateTileInstance(pos, wallTile));
			}
		}

		BSPNode rootNode = new BSPNode(BSPNodeTiles, minX, maxX, minY, maxY);
		Subdivide(maxRecursionDepth, maxRecursionDepth, rootNode, new System.Random().NextDouble() > .5);
		CreateRooms();
	}

	BSPNode CreateNode(int minX, int maxX, int minY, int maxY)
	{
		List<BSPTile> BSPNodeTiles = new List<BSPTile>();

		for (int i = minX; i < maxX; i++)
		{
			for (int j = minY; j < maxY; j++)
			{
				Vector3Int pos = new Vector3Int(i, j, 0);
				BSPNodeTiles.Add(BSPTile.CreateTileInstance(pos, floorTile));
			}
		}

		return new BSPNode(BSPNodeTiles, minX, maxX, minY, maxY);
	}

	void Subdivide(int currentDepth, int maxDepth, BSPNode partition, bool divideVertically)
	{
		if (currentDepth == -1) return;

		if (divideVertically)
		{
			int minSplit = (int)Mathf.Lerp(partition.startX, partition.endX, .4f);
			int maxSplit = (int)Mathf.Lerp(partition.startX, partition.endX, .6f);

			int splitX = rand.Next(minSplit, maxSplit);

			int splitAttempts = 50;
			while (splitX - partition.startX <= minRoomWidth || partition.endX - splitX <= minRoomWidth)
			{
				if (splitAttempts == 0)
					return;
				splitX = rand.Next(minSplit, maxSplit);
				splitAttempts--;
			}

			BSPNode left = CreateNode(partition.startX, splitX, partition.startY, partition.endY);
			partitionStack.Push(left);
			BSPNode right = CreateNode(splitX + 1, partition.endX, partition.startY, partition.endY);
			partitionStack.Push(right);

			left.AddBSPEdge(right);
			right.AddBSPEdge(left);

			if (currentDepth == maxDepth)
			{
				partition.leftChild = left;
				partition.rightChild = right;
			}
			else
			{
				partition.leftChild = left;
				partition.rightChild = right;
				left.parent = partition;
				right.parent = partition;
			}

			Subdivide(currentDepth - 1, maxDepth, left, Mathf.PerlinNoise((float)rand.NextDouble(), UnityEngine.Random.value) <= .25f);
			Subdivide(currentDepth - 1, maxDepth, right, Mathf.PerlinNoise((float)rand.NextDouble(), UnityEngine.Random.value) <= .25f);
		}
		else
		{
			int minSplit = (int)Mathf.Lerp(partition.startY, partition.endY, .4f);
			int maxSplit = (int)Mathf.Lerp(partition.startY, partition.endY, .6f);

			int splitY = rand.Next(minSplit, maxSplit);

			int splitAttempts = 50;
			while (splitY - partition.startY <= minRoomHeight || partition.endY - splitY <= minRoomHeight)
			{
				if (splitAttempts == 0)
					return;
				splitY = rand.Next(minSplit, maxSplit);
				splitAttempts--;
			}

			BSPNode up = CreateNode(partition.startX, partition.endX, partition.startY, splitY);
			partitionStack.Push(up);
			BSPNode down = CreateNode(partition.startX, partition.endX, splitY + 1, partition.endY);
			partitionStack.Push(down);

			up.AddBSPEdge(down);
			down.AddBSPEdge(up);

			if (currentDepth == maxDepth)
			{
				partition.leftChild = up;
				partition.rightChild = down;
			}
			else
			{
				partition.leftChild = up;
				partition.rightChild = down;
				up.parent = partition;
				down.parent = partition;
			}


			Subdivide(currentDepth - 1, maxDepth, up, Mathf.PerlinNoise((float)rand.NextDouble(), UnityEngine.Random.value) > .25f);
			Subdivide(currentDepth - 1, maxDepth, down, Mathf.PerlinNoise((float)rand.NextDouble(), UnityEngine.Random.value) > .25f);
		}
	}

	void CreateRooms()
	{
		List<BSPNode> extraRndCorridors = new List<BSPNode>();

		while (partitionStack.Count > 0)
		{
			BSPNode a = partitionStack.Pop();
			BSPNode b = partitionStack.Pop();

			if (rand.NextDouble() < .5)
				extraRndCorridors.Add(a);
			if (rand.NextDouble() < .5)
				extraRndCorridors.Add(b);

			if (a.leftChild == null && a.rightChild == null)
			{
				int middleX = (a.startX + a.endX) / 2;

				int x1 = rand.Next(a.startX, middleX);
				int x2 = rand.Next(middleX, a.endX);

				int middleY = (a.startY + a.endY) / 2;

				int y1 = rand.Next(a.startY, middleY);
				int y2 = rand.Next(middleY, a.endY);

				int resizeAttempts = 75;

				while ((x2 >= a.endX || x2 >= labyrinthWidth || x1 <= 0 || x2 - x1 < minRoomWidth) ||
					(y2 >= a.endY || y2 >= labyrinthHeight || y1 <= 0 || y2 - y1 < minRoomHeight))
				{
					x1 = rand.Next(a.startX, middleX);
					x2 = rand.Next(middleX, a.endX);
					y1 = rand.Next(a.startY, middleY);
					y2 = rand.Next(middleY, a.endY);
					resizeAttempts--;
					if (resizeAttempts <= 0)
					{
						break;
					}
				}

				a.SetRoomBounds(x1, x2, y1, y2);
				// Debug.DrawLine(new Vector3(x + width / 2, y + height, 0), new Vector3(x + width / 2, y, 0), Color.green, 10f);
				// Debug.DrawLine(new Vector3(x, y + height / 2, 0), new Vector3(x + width, y + height / 2, 0), Color.green, 10f);

				for (int i = x1; i < x2; i++)
				{
					for (int j = y1; j < y2; j++)
					{
						Vector3Int pos = new Vector3Int(i, j, 0);
						// Corner room wall, down-left
						if (i == x1 && j == y1)
						{
							//Matrix rotates counter-clockwise
							Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 180), Vector3.one);
							labyrinth.SetTile(pos, roomWallCornerTile);
							labyrinth.SetTransformMatrix(pos, matrix);
							walkableTiles.Add(pos);
							continue;
						}
						// Corner room wall, up-left
						else if (i == x1 && j == y2 - 1)
						{
							Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 90), Vector3.one);
							labyrinth.SetTile(pos, roomWallCornerTile);
							labyrinth.SetTransformMatrix(pos, matrix);
							walkableTiles.Add(pos);
							continue;
						}
						// Corner room wall, down-right
						else if (i == x2 - 1 && j == y1)
						{
							Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 270), Vector3.one);
							labyrinth.SetTile(pos, roomWallCornerTile);
							labyrinth.SetTransformMatrix(pos, matrix);
							walkableTiles.Add(pos);
							continue;
						}
						// Corner room wall, up-right
						else if (i == x2 - 1 && j == y2 - 1)
						{
							Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 0), Vector3.one);
							labyrinth.SetTile(pos, roomWallCornerTile);
							labyrinth.SetTransformMatrix(pos, matrix);
							walkableTiles.Add(pos);
							continue;
						}
						if (i == x1)
						{
							Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 180), Vector3.one);
							labyrinth.SetTile(pos, roomWallTile);
							labyrinth.SetTransformMatrix(pos, matrix);
							walkableTiles.Add(pos);
							continue;
						}
						else if (i == x2 - 1)
						{
							Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 0), Vector3.one);
							labyrinth.SetTile(pos, roomWallTile);
							labyrinth.SetTransformMatrix(pos, matrix);
							walkableTiles.Add(pos);
							continue;
						}
						else if (j == y1)
						{
							Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 270), Vector3.one);
							labyrinth.SetTile(pos, roomWallTile);
							labyrinth.SetTransformMatrix(pos, matrix);
							walkableTiles.Add(pos);
							continue;
						}
						else if (j == y2 - 1)
						{
							Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 90), Vector3.one);
							labyrinth.SetTile(pos, roomWallTile);
							labyrinth.SetTransformMatrix(pos, matrix);
							walkableTiles.Add(pos);
							continue;
						}
						labyrinth.SetTile(pos, floorTile);
						walkableTiles.Add(pos);
					}
				}

				CreateCorridors(a.corridor);
			}
			else if (a.leftChild != null && a.rightChild != null)
			{
				CreateCorridors(a.corridor, true);
			}

			if (b.leftChild == null && b.rightChild == null)
			{
				int middleX = (b.startX + b.endX) / 2;

				int x1 = rand.Next(b.startX, middleX);
				int x2 = rand.Next(middleX, b.endX);

				int middleY = (b.startY + b.endY) / 2;

				int y1 = rand.Next(b.startY, middleY);
				int y2 = rand.Next(middleY, b.endY);

				int resizeAttempts = 75;

				while ((x2 >= b.endX || x2 >= labyrinthWidth || x1 <= 0 || x2 - x1 < minRoomWidth) ||
					(y2 >= b.endY || y2 >= labyrinthHeight || y1 <= 0 || y2 - y1 < minRoomHeight))
				{
					x1 = rand.Next(b.startX, middleX);
					x2 = rand.Next(middleX, b.endX);
					y1 = rand.Next(b.startY, middleY);
					y2 = rand.Next(middleY, b.endY);
					resizeAttempts--;
					if (resizeAttempts <= 0)
					{
						break;
					}
				}

				b.SetRoomBounds(x1, x2, y1, y2);
				// Debug.DrawLine(new Vector3(x + width / 2, y + height, 0), new Vector3(x + width / 2, y, 0), Color.green, 10f);
				// Debug.DrawLine(new Vector3(x, y + height / 2, 0), new Vector3(x + width, y + height / 2, 0), Color.green, 10f);

				for (int i = x1; i < x2; i++)
				{
					for (int j = y1; j < y2; j++)
					{
						Vector3Int pos = new Vector3Int(i, j, 0);
						if (i == x1 && j == y1)
						{
							Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 180), Vector3.one);
							labyrinth.SetTile(pos, roomWallCornerTile);
							labyrinth.SetTransformMatrix(pos, matrix);
							walkableTiles.Add(pos);
							continue;
						}
						else if (i == x1 && j == y2 - 1)
						{
							Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 90), Vector3.one);
							labyrinth.SetTile(pos, roomWallCornerTile);
							labyrinth.SetTransformMatrix(pos, matrix);
							walkableTiles.Add(pos);
							continue;
						}
						else if (i == x2 - 1 && j == y1)
						{
							Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 270), Vector3.one);
							labyrinth.SetTile(pos, roomWallCornerTile);
							labyrinth.SetTransformMatrix(pos, matrix);
							walkableTiles.Add(pos);
							continue;
						}
						else if (i == x2 - 1 && j == y2 - 1)
						{
							Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 0), Vector3.one);
							labyrinth.SetTile(pos, roomWallCornerTile);
							labyrinth.SetTransformMatrix(pos, matrix);
							walkableTiles.Add(pos);
							continue;
						}
						if (i == x1)
						{
							Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 180), Vector3.one);
							labyrinth.SetTile(pos, roomWallTile);
							labyrinth.SetTransformMatrix(pos, matrix);
							walkableTiles.Add(pos);
							continue;
						}
						else if (i == x2 - 1)
						{
							Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 0), Vector3.one);
							labyrinth.SetTile(pos, roomWallTile);
							labyrinth.SetTransformMatrix(pos, matrix);
							walkableTiles.Add(pos);
							continue;
						}
						else if (j == y1)
						{
							Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 270), Vector3.one);
							labyrinth.SetTile(pos, roomWallTile);
							labyrinth.SetTransformMatrix(pos, matrix);
							walkableTiles.Add(pos);
							continue;
						}
						else if (j == y2 - 1)
						{
							Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 90), Vector3.one);
							labyrinth.SetTile(pos, roomWallTile);
							labyrinth.SetTransformMatrix(pos, matrix);
							walkableTiles.Add(pos);
							continue;
						}
						labyrinth.SetTile(pos, floorTile);
						walkableTiles.Add(pos);
					}
				}

				CreateCorridors(b.corridor);
			}
			else if (b.leftChild != null && b.rightChild != null)
			{
				CreateCorridors(b.corridor, true);
			}
		}

		while (extraRndCorridors.Count > 1)
		{
			int index = rand.Next(extraRndCorridors.Count);
			int lastIndex = index;
			BSPNode a = extraRndCorridors[index];
			while (index == lastIndex)
			{
				index += rand.Next(extraRndCorridors.Count);
				index %= extraRndCorridors.Count;
			}
			BSPNode b = extraRndCorridors[index];

			extraRndCorridors.Remove(a);
			extraRndCorridors.Remove(b);

			if (a.leftChild == null && a.rightChild == null)
			{
				CreateExtraCorridors(a, b);
			}
			else if (a.leftChild != null && a.rightChild != null)
			{
				CreateExtraCorridors(a, b, true);
			}
		}

		OuterWallSecondPass();

		doneGenerating = true;
	}

	void CreateCorridors(BSPEdge edge, bool noRooms = false)
	{
		if (!noRooms)
		{
			Vector3Int roomAMiddle = new Vector3Int(
				(edge.a.startX + edge.a.endX) / 2,
				(edge.a.startY + edge.a.endY) / 2,
				0
			);

			Vector3Int roomBMiddle = new Vector3Int(
				(edge.b.startX + edge.b.endX) / 2,
				(edge.b.startY + edge.b.endY) / 2,
				0
			);

			if (roomAMiddle.x == roomBMiddle.x)
			{
				bool endGreater = roomBMiddle.y > roomAMiddle.y;
				for (int j = roomAMiddle.y; j != roomBMiddle.y; j = endGreater ? j + 1 : j - 1)
				{
					Vector3Int corridorPos = new Vector3Int(roomAMiddle.x, j, 0);
					walkableTiles.Add(corridorPos);
					if (labyrinth.GetTile(corridorPos) == floorTile || labyrinth.GetTile(corridorPos) == roomWallTile || labyrinth.GetTile(corridorPos) == roomWallCornerTile)
						continue;
					labyrinth.SetTile(corridorPos, corridorTile);
				}
			}
			else if (roomAMiddle.y == roomBMiddle.y)
			{
				bool endGreater = roomBMiddle.x > roomAMiddle.x;
				for (int i = roomAMiddle.x; i != roomBMiddle.x; i = endGreater ? i + 1 : i - 1)
				{
					Vector3Int corridorPos = new Vector3Int(i, roomAMiddle.y, 0);
					walkableTiles.Add(corridorPos);
					if (labyrinth.GetTile(corridorPos) == floorTile || labyrinth.GetTile(corridorPos) == roomWallTile || labyrinth.GetTile(corridorPos) == roomWallCornerTile)
						continue;
					labyrinth.SetTile(corridorPos, corridorTile);
				}
			}
		}
		else
		{
			Vector3Int partitionAMiddle = new Vector3Int(
				(edge.a.startX + edge.a.endX) / 2,
				(edge.a.startY + edge.a.endY) / 2,
				0
			);

			Vector3Int partitionBMiddle = new Vector3Int(
				(edge.b.startX + edge.b.endX) / 2,
				(edge.b.startY + edge.b.endY) / 2,
				0
			);

			if (partitionAMiddle.x == partitionBMiddle.x)
			{
				bool endGreater = partitionBMiddle.y > partitionAMiddle.y;
				for (int j = partitionAMiddle.y; j != partitionBMiddle.y; j = endGreater ? j + 1 : j - 1)
				{
					Vector3Int corridorPos = new Vector3Int(partitionAMiddle.x, j, 0);
					walkableTiles.Add(corridorPos);
					if (labyrinth.GetTile(corridorPos) == floorTile || labyrinth.GetTile(corridorPos) == roomWallTile || labyrinth.GetTile(corridorPos) == roomWallCornerTile)
						continue;
					labyrinth.SetTile(corridorPos, corridorTile);
				}
			}
			else if (partitionAMiddle.y == partitionBMiddle.y)
			{
				bool endGreater = partitionBMiddle.x > partitionAMiddle.x;
				for (int i = partitionAMiddle.x; i != partitionBMiddle.x; i = endGreater ? i + 1 : i - 1)
				{
					Vector3Int corridorPos = new Vector3Int(i, partitionAMiddle.y, 0);
					walkableTiles.Add(corridorPos);
					if (labyrinth.GetTile(corridorPos) == floorTile || labyrinth.GetTile(corridorPos) == roomWallTile || labyrinth.GetTile(corridorPos) == roomWallCornerTile)
						continue;
					labyrinth.SetTile(corridorPos, corridorTile);
				}
			}
		}
	}

	void CreateExtraCorridors(BSPNode nodeA, BSPNode nodeB, bool noRooms = false)
	{
		Vector3Int roomAMiddle = new Vector3Int(
				(nodeA.startX + nodeA.endX) / 2,
				(nodeA.startY + nodeA.endY) / 2,
				0
			);

		Vector3Int roomBMiddle = new Vector3Int(
			(nodeB.startX + nodeB.endX) / 2,
			(nodeB.startY + nodeB.endY) / 2,
			0
		);

		bool rnd = UnityEngine.Random.value > .5f;
		Vector3Int meetingPoint = new Vector3Int(
			rnd ? roomAMiddle.x : roomBMiddle.x,
			rnd ? roomBMiddle.y : roomAMiddle.y,
			0
		);

		Vector3Int current = roomAMiddle;
		while (current != meetingPoint)
		{
			current = Vector3.MoveTowards(current, meetingPoint, 1).ToVector3Int();
			walkableTiles.Add(current);
			if (labyrinth.GetTile(current) == floorTile || labyrinth.GetTile(current) == roomWallTile || labyrinth.GetTile(current) == roomWallCornerTile)
				continue;
			labyrinth.SetTile(current, corridorTile);
		}
		while (current != roomBMiddle)
		{
			current = Vector3.MoveTowards(current, roomBMiddle, 1).ToVector3Int();
			walkableTiles.Add(current);
			if (labyrinth.GetTile(current) == floorTile || labyrinth.GetTile(current) == roomWallTile || labyrinth.GetTile(current) == roomWallCornerTile)
				continue;
			labyrinth.SetTile(current, corridorTile);
		}
	}

	//Goes around and adds a second set of outer walls because my room creation isn't perfect
	void OuterWallSecondPass()
	{
		//Left wall, bottom to top
		for (int i = 0; i < labyrinthHeight; i++)
		{
			Vector3Int pos = new Vector3Int(0, i, 0);
			labyrinth.SetTile(pos, wallTile);
		}

		//Upper wall, left to right
		for (int i = 0; i < labyrinthWidth; i++)
		{
			Vector3Int pos = new Vector3Int(i, labyrinthHeight, 0);
			labyrinth.SetTile(pos, wallTile);
		}

		//Right wall, top to bottom
		for (int i = labyrinthHeight; i > 0; i--)
		{
			Vector3Int pos = new Vector3Int(labyrinthWidth, i, 0);
			labyrinth.SetTile(pos, wallTile);
		}

		//Bottom wall, right to left
		for (int i = labyrinthWidth; i > 0; i--)
		{
			Vector3Int pos = new Vector3Int(i, 0, 0);
			labyrinth.SetTile(pos, wallTile);
		}
	}
}
