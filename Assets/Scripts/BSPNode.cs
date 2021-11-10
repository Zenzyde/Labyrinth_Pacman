using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BSPNode
{
	public BSPNode parent, leftChild, rightChild;
	public List<BSPTile> possibleRoomTiles = new List<BSPTile>();
	public BSPEdge corridor;
	public int startX, endX, startY, endY;
	public int roomStartX, roomEndX, roomStartY, roomEndY;

	public BSPNode(List<BSPTile> tiles, int minX, int maxX, int minY, int maxY, BSPNode left = null, BSPNode right = null, BSPNode parent = null)
	{
		possibleRoomTiles.AddRange(tiles);
		startX = minX;
		endX = maxX;
		startY = minY;
		endY = maxY;
		this.parent = parent;
		this.leftChild = left;
		this.rightChild = right;
	}

	public void AddBSPEdge(BSPNode connectingRoom)
	{
		corridor = new BSPEdge(this, connectingRoom);
	}

	public void SetRoomBounds(int minX, int maxX, int minY, int maxY)
	{
		roomStartX = minX;
		roomEndX = maxX;
		roomStartY = minY;
		roomEndY = maxY;
	}
}

public class BSPEdge
{
	public BSPNode a, b;

	public bool Equals(BSPEdge edge)
	{
		return edge.a == a && edge.b == b || edge.a == b && edge.b == a;
	}

	public BSPEdge(BSPNode a, BSPNode b)
	{
		this.a = a;
		this.b = b;
	}
}

public class BSPTile : TileBase
{
	public Vector3Int pos;
	public TileBase tile;

	public static BSPTile CreateTileInstance(Vector3Int pos, TileBase tile)
	{
		BSPTile tileInstance = CreateInstance<BSPTile>();
		tileInstance.pos = pos;
		tileInstance.tile = tile;

		return tileInstance;
	}
}